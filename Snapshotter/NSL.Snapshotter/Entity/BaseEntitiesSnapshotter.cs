using System.Reflection;

namespace NSL.Snapshotter.Entity
{
    public abstract class BaseEntitiesSnapshotter : BaseSnapshotter
    {
        private readonly Assembly[] _assemblies;
        private readonly string _type;

        public BaseEntitiesSnapshotter(string basePath, string type, params Assembly[] assemblies) : this(basePath, type, null, assemblies) { }

        public BaseEntitiesSnapshotter(string basePath, string type, SnapshotItemTypeRegistry? reg, params Assembly[] assemblies) : base(basePath, reg)
        {
            _assemblies = assemblies?.Length > 0 ? assemblies : throw new ArgumentException("assemblies required");
            _type = type;
        }

        protected virtual bool ValidateType(Type t) => true;

        public override string Type => _type;

        protected IEnumerable<Type> GetTypes()
        {
            foreach (var asm in _assemblies)
            {
                Type[] types;
                try { types = asm.GetTypes(); }
                catch (ReflectionTypeLoadException ex) { types = ex.Types.Where(t => t != null).Cast<Type>().ToArray(); }

                foreach (var t in types)
                {
                    if (!t.IsClass) continue;
                    //if (t.GetCustomAttribute<SnapshotTypeAttribute>() == null) continue;
                    if (!ValidateType(t)) continue;
                    yield return t;
                }
            }
        }

        protected virtual IEnumerable<MemberInfo> GetMembers(Type type)
        {
            // v0: public instance properties (no indexers)
            return type
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => p.CanRead && p.GetIndexParameters().Length == 0)
                .Cast<MemberInfo>();
        }

        protected override Task<SnapshotDocument> BuildRuntimeSnapshot(CancellationToken ct)
        {
            var items = new List<SnapshotItem>();

            foreach (var t in GetTypes())
            {
                var fullName = (t.FullName ?? t.Name).Replace('+', '.');

                var members = new List<EntityMemberSnapshot>();
                foreach (var m in GetMembers(t))
                {
                    if (m is not PropertyInfo p) continue;

                    // if someday you switch to whitelist: check [SnapshotMember]
                    // for now: include all public properties
                    var name = p.Name;
                    var typeStr = TypeNameFormatter.Format(p.PropertyType);
                    members.Add(new EntityMemberSnapshot(name, typeStr));
                }

                items.Add(new EntitySnapshotItem(fullName, members.OrderBy(m => m.name, StringComparer.Ordinal).ToArray()));
            }

            return Task.FromResult(CanonicalJson.Canonicalize(new SnapshotDocument(Type, items)));
        }
    }
}
