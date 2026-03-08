using Microsoft.AspNetCore.Http;

namespace NSL.Snapshotter.ASPNET.Http
{
    public interface IHttpRouteSnapshotter
    {
        Task<IResult> GetCurrentVersion(HttpContext ctx);
        Task<IResult> GetCurrentSnapshot(HttpContext ctx);
        Task<IResult> GetMeta(HttpContext ctx);
        Task<IResult> GetVersionSnapshot(HttpContext ctx, long version);
    }

}
