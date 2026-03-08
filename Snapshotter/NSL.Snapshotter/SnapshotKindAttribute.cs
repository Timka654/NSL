namespace NSL.Snapshotter
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class SnapshotKindAttribute : Attribute
    {
        public SnapshotKindAttribute(string kind) => Kind = kind;
        public string Kind { get; }
    }

}
