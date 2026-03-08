namespace NSL.Snapshotter
{
    // =============================
    // 9) Registry + HTTP adapter
    // =============================
    public sealed class SnapshotterRegistry : ISnapshotterRegistry
    {
        private readonly Dictionary<string, ISnapshotter> _map;

        public SnapshotterRegistry(IEnumerable<ISnapshotter> snapshotters)
        {
            _map = snapshotters.ToDictionary(x => x.Type, StringComparer.OrdinalIgnoreCase);
        }

        public IReadOnlyCollection<ISnapshotter> All => _map.Values.ToArray();

        public ISnapshotter Get(string type)
        {
            if (!_map.TryGetValue(type, out var s))
                throw new KeyNotFoundException($"Snapshotter for type '{type}' not registered.");
            return s;
        }
    }

}
