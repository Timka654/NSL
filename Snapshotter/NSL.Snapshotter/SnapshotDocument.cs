namespace NSL.Snapshotter
{
    // =============================
    // 2) DTO: Current "final model"
    // =============================

    public sealed record SnapshotDocument(
        string type,
        IReadOnlyList<SnapshotItem> items
    );

}
