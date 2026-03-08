using System;
using System.Reflection;

namespace NSL.Snapshotter.Entity
{
    // =============================
    // 7) EntitiesSnapshotter (Reflection + [SnapshotType])
    // =============================
    public class EntitiesSnapshotter(string basePath, string type = "entities", params Assembly[] assemblies) : BaseEntitiesSnapshotter(basePath, type, assemblies)
    {
        protected override bool ValidateType(Type t)
            => t.GetCustomAttribute<SnapshotTypeAttribute>() != null;
    }

}
