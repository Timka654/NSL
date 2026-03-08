using System;
using System.Reflection;

namespace Nsl.Snapshotter.Enum
{
    public sealed class EnumsSnapshotter(string basePath, string type = "enums", params Assembly[] assemblies) : BaseEnumsSnapshotter(basePath, type, assemblies)
    {
        protected override bool ValidateType(Type t)
            => t.GetCustomAttribute<SnapshotEnumAttribute>() != null;
    }
}
