using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace NSL.Snapshotter
{
    public enum SnapshotMode
    {
        Validate = 0,
        Build = 1
    }
    // =============================
    // 6) BaseSnapshotter
    // =============================
    public abstract class BaseSnapshotter : ISnapshotter
    {
        private readonly string _basePath;
        private readonly JsonSerializerOptions _metaJsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        protected BaseSnapshotter(string basePath, SnapshotItemTypeRegistry? reg)
        {
            _basePath = basePath ?? throw new ArgumentNullException(nameof(basePath));

            SnapshotJsonOptions.Converters.Add(new SnapshotItemJsonConverter(reg ?? SnapshotItemTypeRegistry.Instance));
        }

        public abstract string Type { get; }

        public long CurrentVersion { get; private set; }
        public DateTimeOffset? PublishedAtUtc { get; private set; }

        protected string TypeDir => Path.Combine(_basePath, Type);

        protected string CurrentJsonPath => Path.Combine(TypeDir, "current.json");
        protected string CurrentHashPath => Path.Combine(TypeDir, "current.hash");
        protected string CurrentMetaPath => Path.Combine(TypeDir, "current.meta.json");

        protected string VersionJsonPath(long v) => Path.Combine(TypeDir, $"v{v}.json");
        protected string VersionHashPath(long v) => Path.Combine(TypeDir, $"v{v}.hash");

        // caching (optional but handy)
        protected ConcurrentDictionary<long, string> VersionFileCache { get; } = new();
        protected volatile string? CurrentJsonCache;
        protected volatile CurrentMeta? CurrentMetaCache; 
        
        
        private static readonly JsonSerializerOptions SnapshotJsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.Never,
        };

        public async Task TryActualize(long version, DateTime publishTime, SnapshotMode mode, CancellationToken ct = default)
        {
            if (version <= 0) throw new InvalidOperationException("CurrentVersion must be >= 1.");

            EnsureTypeDir();

            bool hasVersion = true;

            // 1) No jumps: v1..vN + hashes must exist
            for (long v = 1; v <= version; v++)
            {
                var jp = VersionJsonPath(v);
                var hp = VersionHashPath(v);
                if ((!File.Exists(jp) || !File.Exists(hp)))
                {
                    if (mode == SnapshotMode.Build && v == version)
                    {
                        hasVersion = false;
                        break;
                    }

                    throw new InvalidOperationException($"Snapshot history is not sequential. Missing v{v}.json or v{v}.hash for type '{Type}'.");
                }
            }

            if (mode == SnapshotMode.Build && !hasVersion)
                await BuildVersion(version, publishTime,ct);


            // 2) Current artifacts must exist
            if (!File.Exists(CurrentJsonPath) || !File.Exists(CurrentHashPath) || !File.Exists(CurrentMetaPath))
                throw new InvalidOperationException($"Missing current artifacts for type '{Type}'. Required: current.json, current.hash, current.meta.json");

            // 3) Meta must match current version + parse publishedAtUtc
            var meta = await ReadMeta(ct).ConfigureAwait(false);
            if (meta.version != version)
                throw new InvalidOperationException($"current.meta.json version mismatch for type '{Type}'. Expected {version}, got {meta.version}.");

            // 4) Compare runtime hash with current.hash (no diff)
            var runtimeDoc = await BuildRuntimeSnapshot(ct).ConfigureAwait(false);
            var runtimeJson = CanonicalJson.SerializeUtf8(runtimeDoc, SnapshotJsonOptions);
            var runtimeHex = HashUtil.ComputeSha256Hex(runtimeJson);
            var expectedLine = await File.ReadAllTextAsync(CurrentHashPath, ct).ConfigureAwait(false);
            var (_, expectedHex) = HashUtil.ParseHashLine(expectedLine);

            if (!string.Equals(runtimeHex, expectedHex, StringComparison.Ordinal))
                throw new InvalidOperationException($"Snapshot mismatch for type '{Type}' (current). Rebuild snapshots before release.");

            // success
            CurrentVersion = version;
            PublishedAtUtc = meta.publishedAtUtc;
        }

        private async Task BuildVersion(long version, DateTime publishTime, CancellationToken ct)
        {
            // 0) runtime новый "final"
            var newDoc = await BuildRuntimeSnapshot(ct).ConfigureAwait(false);
            newDoc = CanonicalJson.Canonicalize(newDoc);

            // 1) old берём из current.json если есть, иначе считаем пустым (первый билд)
            SnapshotDocument oldDoc;
            if (File.Exists(CurrentJsonPath))
            {
                var oldJson = await File.ReadAllTextAsync(CurrentJsonPath, ct).ConfigureAwait(false);
                oldDoc = JsonSerializer.Deserialize<SnapshotDocument>(oldJson, SnapshotJsonOptions)
                         ?? throw new InvalidOperationException($"Invalid current.json for type '{Type}'.");
                oldDoc = CanonicalJson.Canonicalize(oldDoc);
            }
            else
            {
                oldDoc = new SnapshotDocument(Type, Array.Empty<SnapshotItem>());
            }

            // 2) diff -> changelog v{version}.json
            var changeSet = BuildChangeSet(oldDoc, newDoc, SnapshotJsonOptions);
            var deltaUtf8 = JsonSerializer.SerializeToUtf8Bytes(changeSet, SnapshotJsonOptions);
            var deltaHex = HashUtil.ComputeSha256Hex(deltaUtf8);

            await WriteAtomic(VersionJsonPath(version), deltaUtf8, ct);
            await WriteAtomicText(VersionHashPath(version), HashUtil.FormatHashLine("sha256", deltaHex), ct);

            // 3) перезаписываем current.* под новую версию
            var currentUtf8 = CanonicalJson.SerializeUtf8(newDoc, SnapshotJsonOptions);
            var currentHex = HashUtil.ComputeSha256Hex(currentUtf8);

            await WriteAtomic(CurrentJsonPath, currentUtf8, ct);
            await WriteAtomicText(CurrentHashPath, HashUtil.FormatHashLine("sha256", currentHex), ct);

            var meta = new CurrentMeta(version, publishTime, notes: "");
            var metaJson = JsonSerializer.Serialize(meta, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            await WriteAtomicText(CurrentMetaPath, metaJson, ct);

            // сброс кешей, если есть
            CurrentJsonCache = null;
            CurrentMetaCache = null;
            VersionFileCache.TryRemove(version, out _);
        }
        public sealed record SnapshotChangeSet(
            IReadOnlyList<SnapshotChange> changes
        );

        public sealed record SnapshotChange(
            string op,          // "append" | "removed" | "replaced"
            string key,         // item.fullName
            SnapshotItem? value // null for removed
        ); 
        
        private static byte[] CanonicalItemBytes(SnapshotItem item, JsonSerializerOptions opts)
        {
            // item должен быть уже в каноничном виде по внутренним спискам.
            // Если ты CanonicalJson.Canonicalize(doc) используешь — items внутри уже отсортированы.
            // Но для item можно дополнительно (и дёшево) прогнать ту же логику, если вынесешь её.
            return JsonSerializer.SerializeToUtf8Bytes((object)item, item.GetType(), opts);
        }

        private static SnapshotChangeSet BuildChangeSet(
    SnapshotDocument oldDoc,
    SnapshotDocument newDoc,
    JsonSerializerOptions snapshotJsonOptions)
        {
            oldDoc = CanonicalJson.Canonicalize(oldDoc);
            newDoc = CanonicalJson.Canonicalize(newDoc);

            var oldMap = oldDoc.items.ToDictionary(i => i.fullName, i => i, StringComparer.Ordinal);
            var newMap = newDoc.items.ToDictionary(i => i.fullName, i => i, StringComparer.Ordinal);

            var changes = new List<SnapshotChange>();

            // removed
            foreach (var key in oldMap.Keys)
            {
                if (!newMap.ContainsKey(key))
                    changes.Add(new SnapshotChange("removed", key, null));
            }

            // append + modified
            foreach (var (key, newItem) in newMap)
            {
                if (!oldMap.TryGetValue(key, out var oldItem))
                {
                    changes.Add(new SnapshotChange("append", key, newItem));
                    continue;
                }

                // compare internal contents
                var oldBytes = CanonicalItemBytes(oldItem, snapshotJsonOptions);
                var newBytes = CanonicalItemBytes(newItem, snapshotJsonOptions);

                if (!oldBytes.AsSpan().SequenceEqual(newBytes))
                    changes.Add(new SnapshotChange("modified", key, newItem));
            }

            // deterministic order for delta file
            changes.Sort((a, b) =>
            {
                var c = string.CompareOrdinal(a.key, b.key);
                if (c != 0) return c;
                return string.CompareOrdinal(a.op, b.op);
            });

            return new SnapshotChangeSet(changes);
        }



        private static async Task WriteAtomic(string path, byte[] data, CancellationToken ct)
        {
            var tmp = path + ".tmp";
            await File.WriteAllBytesAsync(tmp, data, ct);
            File.Move(tmp, path, overwrite: true);
        }

        private static async Task WriteAtomicText(string path, string text, CancellationToken ct)
        {
            var tmp = path + ".tmp";
            await File.WriteAllTextAsync(tmp, text, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), ct);
            File.Move(tmp, path, overwrite: true);
        }


        protected virtual Task<SnapshotDocument> BuildRuntimeSnapshot(CancellationToken ct)
            => Task.FromResult(new SnapshotDocument(Type, Array.Empty<SnapshotItem>()));

        protected void EnsureTypeDir()
        {
            if (!Directory.Exists(TypeDir))
                Directory.CreateDirectory(TypeDir);
        }

        protected async Task<CurrentMeta> ReadMeta(CancellationToken ct)
        {
            if (CurrentMetaCache != null) return CurrentMetaCache;

            var json = await File.ReadAllTextAsync(CurrentMetaPath, ct).ConfigureAwait(false);
            var meta = JsonSerializer.Deserialize<CurrentMeta>(json, _metaJsonOptions)
                       ?? throw new InvalidOperationException($"Invalid current.meta.json for type '{Type}'.");

            CurrentMetaCache = meta;
            return meta;
        }

        // Used by HTTP layer
        public async Task<string> ReadCurrentJson(CancellationToken ct = default)
        {
            var cached = CurrentJsonCache;
            if (cached != null) return cached;

            var json = await File.ReadAllTextAsync(CurrentJsonPath, ct).ConfigureAwait(false);
            CurrentJsonCache = json;
            return json;
        }

        public async Task<string> ReadMetaJson(CancellationToken ct = default)
            => await File.ReadAllTextAsync(CurrentMetaPath, ct).ConfigureAwait(false);

        public async Task<string> ReadVersionJson(long version, CancellationToken ct = default)
        {
            if (version <= 0) throw new ArgumentOutOfRangeException(nameof(version));

            if (VersionFileCache.TryGetValue(version, out var cached))
                return cached;

            var path = VersionJsonPath(version);
            if (!File.Exists(path))
                throw new FileNotFoundException($"Snapshot version file not found: v{version}.json", path);

            var json = await File.ReadAllTextAsync(path, ct).ConfigureAwait(false);
            VersionFileCache[version] = json;
            return json;
        }
    }

}
