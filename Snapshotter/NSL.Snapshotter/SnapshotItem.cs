using Nsl.Snapshotter.Enum;
using NSL.Snapshotter.Entity;
using System.Text.Json.Serialization;

namespace NSL.Snapshotter
{
    public abstract record SnapshotItem(string kind, string fullName);

}
