using NSL.Snapshotter;
using System.Collections.Generic;

namespace NSL.Snapshotter.Enum
{
    [SnapshotKind("enum")]
    public sealed record EnumSnapshotItem(
        string fullName,
        string underlyingType,
        IReadOnlyList<EnumValueSnapshot> values
    ) : SnapshotItem("enum",fullName);
}
