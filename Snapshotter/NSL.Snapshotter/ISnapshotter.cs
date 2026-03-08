using System.Reflection;

namespace NSL.Snapshotter
{
    // =============================
    // 4) Interfaces
    // =============================
    public interface ISnapshotter
    {
        string Type { get; }
        long CurrentVersion { get; }              // set after successful TryActualize
        DateTimeOffset? PublishedAtUtc { get; }   // from current.meta.json after actualize

        Task TryActualize(long version, DateTime publishTime, SnapshotMode mode, CancellationToken ct = default);
    }


    public sealed record ModelSnapshot(
        string fullName,
        IReadOnlyList<ModelMemberSnapshot> members
    );

    public sealed record ModelMemberSnapshot(
        string name,
        string type,
        ModelSnapshot? nested
    );
    public static class ModelSchemaBuilder
    {
        // “базовые/специфические” (то, что не разворачиваем)
        public static bool IsLeaf(Type t)
        {
            t = Nullable.GetUnderlyingType(t) ?? t;

            if (t.IsEnum) return true;

            if (t == typeof(string) ||
                t == typeof(Guid) ||
                t == typeof(DateTime) ||
                t == typeof(DateTimeOffset) ||
                t == typeof(decimal) ||
                t == typeof(double) ||
                t == typeof(float) ||
                t == typeof(bool) ||
                t == typeof(byte) ||
                t == typeof(short) ||
                t == typeof(int) ||
                t == typeof(long))
                return true;

            return false;
        }

        public static ModelSnapshot? Build(Type? root, int maxDepth)
        {
            if (root == null) return null;
            if (maxDepth <= 0) maxDepth = 1;

            var visited = new HashSet<Type>();
            return BuildCore(root, maxDepth, visited);
        }

        private static ModelSnapshot? BuildCore(Type t, int depthLeft, HashSet<Type> visited)
        {
            t = Nullable.GetUnderlyingType(t) ?? t;

            if (IsLeaf(t))
                return new ModelSnapshot(TypeNameFormatter.Format(t), Array.Empty<ModelMemberSnapshot>());

            // коллекции: разворачиваем элемент (но не делаем members у самой коллекции)
            if (TryGetElementType(t, out var elementType))
            {
                var nested = depthLeft > 1 ? BuildCore(elementType!, depthLeft - 1, visited) : null;
                // сама коллекция как модель без members, но с nested у единственного "item" можно (упростим: пусто)
                return new ModelSnapshot(TypeNameFormatter.Format(t), Array.Empty<ModelMemberSnapshot>());
                // Если хочешь прямо показать, что внутри:
                // return new ModelSnapshot(TypeNameFormatter.Format(t), new[] { new ModelMemberSnapshot("item", TypeNameFormatter.Format(elementType!), nested) });
            }

            if (!visited.Add(t))
            {
                // цикл — останавливаем
                return new ModelSnapshot(TypeNameFormatter.Format(t), Array.Empty<ModelMemberSnapshot>());
            }

            if (depthLeft <= 0)
                return new ModelSnapshot(TypeNameFormatter.Format(t), Array.Empty<ModelMemberSnapshot>());

            var props = t.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => p.CanRead && p.GetIndexParameters().Length == 0)
                .OrderBy(p => p.Name, StringComparer.Ordinal)
                .ToArray();

            var members = new List<ModelMemberSnapshot>(props.Length);

            foreach (var p in props)
            {
                var pt = p.PropertyType;
                var typeStr = TypeNameFormatter.Format(pt);

                ModelSnapshot? nested = null;
                var effective = Nullable.GetUnderlyingType(pt) ?? pt;

                if (depthLeft > 1 && !IsLeaf(effective))
                {
                    // если коллекция — разворачиваем элемент, но nested отдаём по элементу
                    if (TryGetElementType(effective, out var elem))
                        nested = BuildCore(elem!, depthLeft - 1, visited);
                    else
                        nested = BuildCore(effective, depthLeft - 1, visited);
                }

                members.Add(new ModelMemberSnapshot(p.Name, typeStr, nested));
            }

            visited.Remove(t);

            return new ModelSnapshot(TypeNameFormatter.Format(t), members);
        }

        private static bool TryGetElementType(Type t, out Type? elementType)
        {
            elementType = null;

            if (t.IsArray)
            {
                elementType = t.GetElementType();
                return elementType != null;
            }

            if (t == typeof(string)) return false;

            // IEnumerable<T>
            var enumIface = t.GetInterfaces()
                .Concat(new[] { t })
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));

            if (enumIface != null)
            {
                elementType = enumIface.GetGenericArguments()[0];
                return true;
            }

            // non-generic IEnumerable
            if (typeof(IEnumerable<>).IsAssignableFrom(t))
            {
                elementType = typeof(object);
                return true;
            }

            return false;
        }
    }

}
