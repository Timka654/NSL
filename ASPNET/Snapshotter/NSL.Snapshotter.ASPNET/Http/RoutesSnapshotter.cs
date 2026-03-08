using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using NSL.Snapshotter;

namespace NSL.Snapshotter.ASPNET.Http
{
    // =============================
    // 8) RoutesSnapshotter (ASP.NET MVC action descriptors)
    // =============================
    public class RoutesSnapshotter(string basePath, SnapshotItemTypeRegistry? reg, IActionDescriptorCollectionProvider actions) : BaseRoutesSnapshotter<RouteSnapshotItem>(basePath, reg, actions)
    {
        public RoutesSnapshotter(string basePath, IActionDescriptorCollectionProvider actions)
            : this(basePath, null, actions)
        {
                
        }
        protected override RouteSnapshotItem BuildRoute(ControllerActionDescriptor d)
        {
            var httpMethod = GetHttpMethod(d);

            var (urlNormalized, actionParams, paramList) = ProcessRoute(d);

            var key = $"{httpMethod} {urlNormalized}";

            ModelSnapshot? requestSchema = null;
            foreach (var p in d.Parameters.OfType<ControllerParameterDescriptor>())
            {
                if (p.ParameterInfo.GetCustomAttribute<FromBodyAttribute>() != null)
                {
                    requestSchema = ModelSchemaBuilder.Build(p.ParameterInfo.ParameterType, 5);
                    break;
                }
            }

            var responseClr = UnwrapResponseClrType(d.MethodInfo.ReturnType); // см. ниже
            ModelSnapshot? responseSchema = responseClr == null ? null : ModelSchemaBuilder.Build(responseClr, 5);


            return new RouteSnapshotItem(
                fullName: key,
                method: httpMethod,
                url: urlNormalized,
                @params: paramList,
                request: requestSchema,
                response: responseSchema
            );
        }

        protected override IEnumerable<RouteSnapshotItem> OrderSnapshot(IEnumerable<RouteSnapshotItem> input)
            => input
            .Cast<RouteSnapshotItem>()
            .OrderBy(x => x.url, StringComparer.Ordinal)
            .ThenBy(x => x.method, StringComparer.Ordinal);
    }
}
