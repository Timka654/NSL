using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

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
