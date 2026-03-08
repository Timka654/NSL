using Microsoft.AspNetCore.Http;
using NSL.Snapshotter;
using System.Text;

namespace NSL.Snapshotter.ASPNET.Http
{
    public sealed class HttpRouteSnapshotter : IHttpRouteSnapshotter
    {
        private readonly ISnapshotterRegistry _registry;

        public HttpRouteSnapshotter(ISnapshotterRegistry registry)
        {
            _registry = registry;
        }

        // ctx.Request.RouteValues["type"]
        private BaseSnapshotter GetBase(HttpContext ctx)
        {
            var type = (ctx.Request.RouteValues.TryGetValue("type", out var v) ? v?.ToString() : null) ?? "";
            var s = _registry.Get(type);
            if (s is not BaseSnapshotter b)
                throw new InvalidOperationException($"Snapshotter '{type}' is not a BaseSnapshotter.");
            return b;
        }

        public async Task<IResult> GetCurrentVersion(HttpContext ctx)
        {
            var s = GetBase(ctx);

            // publishedAtUtc is optional for clients; they can compute "time remaining"
            var obj = new
            {
                currentVersion = s.CurrentVersion,
                publishedAtUtc = s.PublishedAtUtc
            };
            return Results.Json(obj);
        }

        public async Task<IResult> GetCurrentSnapshot(HttpContext ctx)
        {
            var s = GetBase(ctx);
            var json = await s.ReadCurrentJson(ctx.RequestAborted);
            return Results.Text(json, "application/json", Encoding.UTF8);
        }

        public async Task<IResult> GetMeta(HttpContext ctx)
        {
            var s = GetBase(ctx);
            var json = await s.ReadMetaJson(ctx.RequestAborted);
            return Results.Text(json, "application/json", Encoding.UTF8);
        }

        public async Task<IResult> GetVersionSnapshot(HttpContext ctx, long version)
        {
            var s = GetBase(ctx);
            var json = await s.ReadVersionJson(version, ctx.RequestAborted);
            return Results.Text(json, "application/json", Encoding.UTF8);
        }
    }

}
