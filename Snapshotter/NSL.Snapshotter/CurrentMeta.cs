using System;

namespace NSL.Snapshotter
{
    // =============================
    // 3) DTO: current.meta.json
    // =============================
    public sealed record CurrentMeta(
        long version,
        DateTimeOffset publishedAtUtc,
        string? notes
    );

}
