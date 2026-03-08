using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace NSL.Snapshotter.ASPNET.Http
{
    public abstract class BaseRoutesSnapshotter<TType>(string basePath, SnapshotItemTypeRegistry? reg, IActionDescriptorCollectionProvider actions) : BaseSnapshotter(basePath,reg)
        where TType : SnapshotItem
    {
        public override string Type => "routes";

        protected virtual string GetHttpMethod(ControllerActionDescriptor d)
        {
            var httpMethod = d.ActionConstraints?
                .OfType<Microsoft.AspNetCore.Mvc.ActionConstraints.HttpMethodActionConstraint>()
                .SelectMany(x => x.HttpMethods)
                .FirstOrDefault()
                ?? "POST"; // your pattern: POST by default

            return httpMethod.ToUpperInvariant();
        }

        protected virtual string BuildFullUrl(ControllerActionDescriptor d)
        {
            // URL normalize: leading '/', collapse '//', keep '/'
            // Use the combined Template if present; else fallback to "/{Controller}/{Action}"
            var template = d.AttributeRouteInfo?.Template;

            string fullUrl;

            if (!string.IsNullOrWhiteSpace(template))
                fullUrl = RouteNormalize.NormalizeToAbsolute(template, "");
            else
                fullUrl = RouteNormalize.NormalizeToAbsolute($"/{d.ControllerName}", d.ActionName);

            return fullUrl;
        }

        protected virtual (string normalizedUrl, List<(string name, Type type)> actionParams, List<RouteParamSnapshot> paramList) ProcessRoute(ControllerActionDescriptor d)
        {
            // Extract action param types (for fallback)
            var actionParams = d.Parameters
                .OfType<ControllerParameterDescriptor>()
                .Select(p => (p.Name, p.ParameterInfo.ParameterType))
                .ToList();

            // Normalize route params: keep "{name}" in URL; extract type from "{name:type}" and/or signature
            var (urlNormalized, paramList) = RouteNormalize.NormalizeParams(BuildFullUrl(d), actionParams);

            return (urlNormalized, actionParams, paramList);
        }

        protected virtual Type? GetRoutePostRequestType(ControllerActionDescriptor d)
        {
            foreach (var p in d.Parameters.OfType<ControllerParameterDescriptor>())
            {
                if (p.ParameterInfo.GetCustomAttribute<FromBodyAttribute>() != null)
                {
                    return p.ParameterInfo.ParameterType;
                }
            }

            return null;
        }

        protected virtual Type? GetRouteReturnType(ControllerActionDescriptor d)
            => UnwrapResponseClrType(d.MethodInfo.ReturnType);

        protected virtual IEnumerable<TType> OrderSnapshot(IEnumerable<TType> input)
            => input.OrderBy(x => x.fullName, StringComparer.Ordinal);


        protected virtual List<TType> BuildRoutes(IEnumerable<ControllerActionDescriptor> descriptions)
        {
            var items = new List<TType>();

            foreach (var d in descriptions)
            {
                items.Add(BuildRoute(d));
            }

            return items;
        }

        protected abstract TType BuildRoute(ControllerActionDescriptor d);

        protected override Task<SnapshotDocument> BuildRuntimeSnapshot(CancellationToken ct)
        {
            var descriptors = actions.ActionDescriptors.Items
                .OfType<ControllerActionDescriptor>()
                .ToArray();

            var items = BuildRoutes(descriptors);

            // Sort by url (and method), as agreed
            var ordered = OrderSnapshot(items);

            return Task.FromResult(CanonicalJson.Canonicalize(new SnapshotDocument(Type, ordered.Cast<SnapshotItem>().ToArray())));
        }

        protected static Type? UnwrapResponseClrType(Type t)
        {
            if (typeof(Task).IsAssignableFrom(t))
            {
                if (t.IsGenericType) return UnwrapResponseClrType(t.GetGenericArguments()[0]);
                return null;
            }

            if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(ActionResult<>))
                return t.GetGenericArguments()[0];

            if (t == typeof(IResult) || typeof(IActionResult).IsAssignableFrom(t) || t == typeof(ActionResult))
                return null;

            return t;
        }
    }
}
