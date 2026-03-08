namespace NSL.Snapshotter
{
    public interface ISnapshotterRegistry
    {
        ISnapshotter Get(string type);
        IReadOnlyCollection<ISnapshotter> All { get; }
    }

}
