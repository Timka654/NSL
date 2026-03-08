using System.Reflection;

namespace NSL.Snapshotter
{
    public sealed class SnapshotItemTypeRegistry
    {
        public static SnapshotItemTypeRegistry Instance { get; } = new ();

        private readonly Dictionary<string, Type> _map = new(StringComparer.OrdinalIgnoreCase);

        public void Register(Type t)
        {
            if (!typeof(SnapshotItem).IsAssignableFrom(t) || t.IsAbstract) return;

            var kind = t.GetCustomAttribute<SnapshotKindAttribute>()?.Kind?.Trim();
            if (string.IsNullOrWhiteSpace(kind))
                throw new InvalidOperationException($"{t.FullName} missing [SnapshotKind].");

            _map[kind] = t;
        }

        public void RegisterFromAssemblies(params Assembly[] assemblies)
        {
            foreach (var asm in assemblies)
                foreach (var t in asm.GetTypes())
                    if (typeof(SnapshotItem).IsAssignableFrom(t) && !t.IsAbstract)
                        Register(t);
        }

        public Type Resolve(string kind)
            => _map.TryGetValue(kind, out var t)
                ? t
                : throw new InvalidOperationException($"Unknown snapshot item kind '{kind}'.");
    }
}
