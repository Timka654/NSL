using System.Collections.Generic;

namespace NSL.Snapshotter.ASPNET.Http
{
    [SnapshotKind("route")]
    public sealed record RouteSnapshotItem(
        string fullName,                 // "{METHOD} {URL}"
        string method,
        string url,
        IReadOnlyList<RouteParamSnapshot> @params,
        ModelSnapshot? request,
        ModelSnapshot? response
    ) : SnapshotItem("route", fullName);

}
