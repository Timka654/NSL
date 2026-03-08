using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Nsl.Snapshotter.Enum
{
    public sealed class EnumsSnapshotter(string basePath, string type = "enums", params Assembly[] assemblies) : BaseEnumsSnapshotter(basePath, type, assemblies)
    {
        protected override bool ValidateType(Type t)
            => t.GetCustomAttribute<SnapshotEnumAttribute>() != null;
    }
}
