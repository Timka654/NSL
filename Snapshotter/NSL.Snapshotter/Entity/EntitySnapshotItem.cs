namespace NSL.Snapshotter.Entity
{
    [SnapshotKind("entity")]
    public sealed record EntitySnapshotItem(
        string fullName,
        IReadOnlyList<EntityMemberSnapshot> members
    ) : SnapshotItem("entity",fullName);

}
