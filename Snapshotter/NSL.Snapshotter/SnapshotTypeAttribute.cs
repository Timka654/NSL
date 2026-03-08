namespace NSL.Snapshotter
{
    // ==========================================================
    // Snapshotter v0 (working baseline) — .NET 8/9, ASP.NET Core
    // Spec implemented (from our chat):
    // - basePath/{type}/current.json|current.hash|current.meta.json
    // - basePath/{type}/v{n}.json|v{n}.hash    (history/changelog, only validated for sequential presence)
    // - current.hash: single line "sha256:<lowercase-hex>" (no newline required)
    // - TryActualize(version) called from Program (clients never trigger it)
    // - strict "no jumps" for history: must have v1..vN (+hash) for CurrentVersion=N
    // - validate runtime canonical snapshot hash == current.hash (no diff)
    // - meta exposed via /meta; current contract via /
    // - routes normalization: leading '/', collapse '//' via while replace, keep '{name}' tokens
    // - op in changelog is "append|removed|replaced" (lower_case trimmed) — served as-is, not parsed here.
    // ==========================================================



    // =============================
    // 1) Attributes (Entities side)
    // =============================
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class SnapshotTypeAttribute : Attribute { }

}
