using NSL.Snapshotter;
using System.Reflection;

namespace Nsl.Snapshotter.Enum
{
    public abstract class BaseEnumsSnapshotter : BaseSnapshotter
    {
        private readonly Assembly[] _assemblies;
        private readonly string _type;


        public BaseEnumsSnapshotter(string basePath, string type, SnapshotItemTypeRegistry? registry, params Assembly[] assemblies) : base(basePath, registry)
        { 
            _assemblies = assemblies?.Length > 0 ? assemblies : throw new ArgumentException("assemblies required");
            _type = type;
        }
            
        public BaseEnumsSnapshotter(string basePath, string type, params Assembly[] assemblies) : this(basePath, type, null, assemblies)
        {
        }

        public override string Type => _type;

        protected virtual bool ValidateType(Type t) => true;

        protected override Task<SnapshotDocument> BuildRuntimeSnapshot(CancellationToken ct)
        {
            var items = new List<SnapshotItem>();

            foreach (var asm in _assemblies)
            {
                Type[] types;
                try { types = asm.GetTypes(); }
                catch (ReflectionTypeLoadException ex) { types = ex.Types.Where(t => t != null).Cast<Type>().ToArray(); }

                foreach (var t in types)
                {
                    if (!t.IsEnum) continue;
                    //if (t.GetCustomAttribute<SnapshotEnumAttribute>() == null) continue;
                    if (!ValidateType(t)) continue;

                    var fullName = (t.FullName ?? t.Name).Replace('+', '.');
                    var underlying = TypeNameFormatter.Format(System.Enum.GetUnderlyingType(t));

                    // значения
                    var names = System.Enum.GetNames(t);
                    var values = new List<EnumValueSnapshot>(names.Length);

                    foreach (var name in names)
                    {
                        var raw = System.Enum.Parse(t, name);
                        // строкой, чтобы не зависеть от типа подложки
                        var num = Convert.ToUInt64(raw);
                        values.Add(new EnumValueSnapshot(name, num.ToString()));
                    }

                    // стабильный порядок
                    values = values
                        .OrderBy(v => ulong.Parse(v.value))
                        .ThenBy(v => v.name, StringComparer.Ordinal)
                        .ToList();

                    items.Add(new EnumSnapshotItem(fullName, underlying, values));
                }
            }

            return Task.FromResult(CanonicalJson.Canonicalize(new SnapshotDocument(Type, items)));
        }
    }
}
