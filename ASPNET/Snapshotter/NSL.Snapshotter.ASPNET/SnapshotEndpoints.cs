using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using NSL.Snapshotter.ASPNET.Http;

namespace NSL.Snapshotter.ASPNET
{
    // =============================
    // 10) Minimal API wiring helper
    // =============================
    public static class SnapshotEndpoints
    {
        public static IEndpointRouteBuilder MapSnapshotEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/nsl/snapshots/{type}/current_version",
                async (HttpContext ctx, IHttpRouteSnapshotter http) => await http.GetCurrentVersion(ctx));

            app.MapGet("/nsl/snapshots/{type}",
                async (HttpContext ctx, IHttpRouteSnapshotter http) => await http.GetCurrentSnapshot(ctx));

            app.MapGet("/nsl/snapshots/{type}/meta",
                async (HttpContext ctx, IHttpRouteSnapshotter http) => await http.GetMeta(ctx));

            app.MapGet("/nsl/snapshots/{type}/{version:long}",
                async (HttpContext ctx, long version, IHttpRouteSnapshotter http) => await http.GetVersionSnapshot(ctx, version));

            return app;
        }
    }

}
