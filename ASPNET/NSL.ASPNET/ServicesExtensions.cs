using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NSL.ASPNET.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static NSL.ASPNET.registerServiceItemRecord;

namespace NSL.ASPNET
{
    public static class ServicesExtensions
    {
        public delegate Task InvokeInScopeAsyncDelegate(IServiceProvider provider);
        public delegate void InvokeInScopeDelegate(IServiceProvider provider);

        public static async Task InvokeInScopeAsync(this IServiceProvider provider, InvokeInScopeAsyncDelegate action)
        {
            await using var scope = provider.CreateAsyncScope();

            await action(scope.ServiceProvider);
        }

        public static void InvokeInScope(this IServiceProvider provider, InvokeInScopeDelegate action)
        {
            using var scope = provider.CreateScope();

            action(scope.ServiceProvider);
        }

        /// <summary>
        /// <code>
        /// <Project InitialTargets="UpdateHtml" Sdk=...>
        /// <Target Name="UpdateHtml" Condition="'$(Configuration)'=='Release'">
        /// 	<Message Text="Try move index.html for update" />
        /// 	<ItemGroup>
        /// 		<StateFiles Include="wwwroot\index.html" />
        /// 	</ItemGroup>
        /// 	<Copy SourceFiles="@(StateFiles)" DestinationFiles="@(StateFiles-&gt;Replace('.html','.html.update'))" />
        /// </Target>
        /// </code>
        /// </summary>
        /// <param name="host"></param>
        /// <param name="removeInputAfterProcess"></param>
        /// <returns></returns>
        public static bool RebuildHtmlIndexFileVersion(this IHost host, bool removeInputAfterProcess = true)
        {
            var updateIndexFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "index.html.update");

            var indexFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "index.html");

            return host.RebuildHtmlFileVersion(updateIndexFilePath, indexFilePath, removeInputAfterProcess);
        }

        public static bool RebuildHtmlFileVersion(this IHost host, string inputFilePath, string outputFilePath, bool removeInputAfterProcess = true)
        {
            if (!System.IO.File.Exists(inputFilePath))
                return false;

            var doc = new HtmlDocument();

            doc.Load(inputFilePath);

            var ver = DateTime.UtcNow.ToString("yyyyMMddHHmmss");

            ProcessIndexHtmlElement(doc.DocumentNode, ver);

            if (System.IO.File.Exists(outputFilePath))
                System.IO.File.Delete(outputFilePath);

            doc.Save(outputFilePath);

            if (removeInputAfterProcess)
                System.IO.File.Delete(inputFilePath);

            return true;
        }

        private static void ProcessIndexHtmlElement(HtmlNode node, string ver)
        {
            foreach (var item in node.ChildNodes)
            {
                if (item.NodeType != HtmlNodeType.Element)
                    continue;

                if (item.Name == "script")
                {
                    var src = item.GetAttributeValue("src", string.Empty);

                    if (!src.StartsWith("http") && !string.IsNullOrEmpty(src))
                    {
                        if (!src.Contains("ver"))
                        {
                            src = $"{src}?ver={ver}";
                            item.SetAttributeValue("src", src);
                        }
                    }
                }
                else if (item.Name == "link")
                {
                    var href = item.GetAttributeValue("href", string.Empty);

                    if (!href.StartsWith("http") && !string.IsNullOrEmpty(href))
                    {
                        if (!href.Contains("ver"))
                        {
                            href = $"{href}?ver={ver}";
                            item.SetAttributeValue("href", href);
                        }
                    }
                }

                ProcessIndexHtmlElement(item, ver);
            }
        }

        public static IServiceCollection RegisterServices(this IServiceCollection services, params string[] models)
            => services.RegisterServices(Assembly.GetCallingAssembly(), models);

        public static IServiceCollection RegisterServices(this IServiceCollection services, Assembly assembly, params string[] models)
        {
            var register = new List<registerServiceItemRecord>();

            var haveModels = models.Any();

            foreach (var type in assembly.GetTypes())
            {
                var attrs = type.GetCustomAttributes<RegisterServiceAttribute>();

                var inherits = type.GetCustomAttributes<RegisterServiceInheritsAttribute>();

                foreach (var attr in attrs)
                {
                    if (!haveModels || models.Any(m => attr.Models.Contains(m)))
                    {
                        var item = new registerServiceItemRecord(type, type.IsGenericType ? type.GetGenericTypeDefinition() : null, attr.Type, attr.Key, attr.HostedService, inherits.ToArray());

                        var constrs = type.GetConstructors();

                        var constr = constrs
                            .Where(x => x.GetCustomAttributes<RegisterServiceConstructorAttribute>().Count() > 0)
                            .FirstOrDefault();

                        if (constr == null && constrs.Length > 1)
                        {
                            throw new InvalidOperationException($"Cannot load type {type.FullName} - this type have multiple constructors. Mark type by {nameof(RegisterServiceConstructorAttribute)} attribute for set constructor for DI");
                        }
                        else
                            constr = constrs.First();

                        item.NeedTypes = constr.GetParameters()
                            .Where(x => !registerServiceSkipped.Contains(x.ParameterType))
                            .Select(p =>
                        {
                            var key = p.GetCustomAttribute<FromKeyedServicesAttribute>()?.Key ?? KeyedService.AnyKey;

                            return new NeedTypeRecord(p.ParameterType, key == KeyedService.AnyKey ? default : key, p.ParameterType.IsGenericType ? p.ParameterType.GetGenericTypeDefinition() : null);

                        }).ToArray();

                        register.Add(item);
                    }
                }
            }

            string formatUnresolvedTypes(params registerServiceItemRecord[] items)
            {
                var sb = new StringBuilder();

                foreach (var item in items)
                {
                    sb.AppendLine($"> {item.Type.FullName}[{item.Lifetime}] have unresolved dependency");
                    var unresolved = item.NeedTypes.Where(nt =>
                    !services.Any(x => (x.ServiceType == nt.type || x.ServiceType == nt.genericParent)
                    && Equals(x.ServiceKey, nt.key)
                    && x.Lifetime <= item.Lifetime));

                    foreach (var u in unresolved)
                    {
                        var s = $"--> {u.type.FullName}";

                        if (u.genericParent != default)
                            s += $"/{u.genericParent.FullName}";

                        if (u.key != default)
                        {
                            s += $"(key:{u.key})";

                        }
                        sb.AppendLine(s);
                    }
                }

                return sb.ToString().Trim();
            }


            var registered = new List<registerServiceItemRecord>();

            do
            {
                var s = registered.Count;

                foreach (var type in register)
                {
                    if (!type.NeedTypes.All(nt =>
                    services.Any(x => (x.ServiceType == nt.type || x.ServiceType == nt.genericParent)
                    && Equals(x.ServiceKey, nt.key)
                    && x.Lifetime <= type.Lifetime)))
                        continue;

                    if (registered.Contains(type))
                        continue;


                    void registerService(ServiceLifetime lifeTime, Type type, object? key, Type? baseType = default)
                    {
                        switch (lifeTime)
                        {
                            case ServiceLifetime.Singleton:
                                if (key != default)
                                {
                                    if (baseType != default)
                                        services.AddKeyedSingleton(baseType, key, (x, o)=>x.GetRequiredKeyedService(type, key));
                                    else
                                        services.AddKeyedSingleton(type, key);
                                }
                                else
                                {
                                    if (baseType != default)
                                        services.AddSingleton(baseType, (x) => x.GetRequiredService(type));
                                    else
                                        services.AddSingleton(type);
                                }
                                break;
                            case ServiceLifetime.Scoped:
                                if (key != default)
                                {
                                    if (baseType != default)
                                        services.AddKeyedScoped(baseType, key, (x, o) => x.GetRequiredKeyedService(type, key));
                                    else
                                        services.AddKeyedScoped(type, key);
                                }
                                else
                                {
                                    if (baseType != default)
                                        services.AddScoped(baseType, (x) => x.GetRequiredService(type));
                                    else
                                        services.AddScoped(type);
                                }
                                break;
                            case ServiceLifetime.Transient:
                                if (key != default)
                                {
                                    if (baseType != default)
                                        services.AddKeyedTransient(baseType, key, (x, o) => x.GetRequiredKeyedService(type, key));
                                    else
                                        services.AddKeyedTransient(type, key);
                                }
                                else
                                {
                                    if (baseType != default)
                                        services.AddTransient(baseType, (x) => x.GetRequiredService(type));
                                    else
                                        services.AddTransient(type);
                                }
                                break;
                            default:
                                throw new InvalidOperationException($"Type \"{type.FullName}\" have invalid lifetime cycle {lifeTime}");
                        }
                    }

                    registerService(type.Lifetime, type.Type, type.Key);

                    foreach (var inh in type.Inherits)
                    {
                        registerService(inh.Type, type.Type, inh.Key, inh.Inherit);
                    }

                    if (type.HostedService)
                    {
                        if (type.Lifetime != ServiceLifetime.Singleton)
                            throw new InvalidOperationException($"Type \"{type.Type.FullName}\" have invalid lifetime cycle {type.Lifetime} for register as HostedService");

                        if (!type.Type.IsAssignableTo(typeof(IHostedService)))
                            throw new InvalidOperationException($"Type \"{type.Type.FullName}\" is not assignable to {typeof(IHostedService).FullName} for register as HostedService");

                        services.AddSingleton<IHostedService>(x => x.GetRequiredService(type.Type) is IHostedService hs ? hs : null);
                    }

                    registered.Add(type);
                }

                if (s == registered.Count)
                {
                    register.RemoveAll(registered.Contains);

                    var unresolvedType = register.FirstOrDefault(x =>
                        !register.Any(o =>
                            x.NeedTypes.Any(nt => (nt.type == o.Type || nt.genericParent == o.GenericDeclarationType) && Equals(nt.key, o.Key))));

                    if (unresolvedType != null)
                        throw new InvalidOperationException($"Register services - have unresolved internal dependency\n{formatUnresolvedTypes(register.ToArray())}");

                    var conflictType = register.FirstOrDefault(x =>
                        services.Any(o =>
                        x.NeedTypes.Any(nt => (nt.type == o.ServiceType || nt.genericParent == o.ServiceType) && Equals(nt.key, o.ServiceKey))));

                    if (conflictType != null)
                        throw new InvalidOperationException($"Register services - have unresolved external dependency conflict\n{formatUnresolvedTypes(register.ToArray())}");


                    throw new InvalidOperationException($"Register services - cannot resolve types\n{formatUnresolvedTypes(register.ToArray())}");
                }
            } while (register.Count > registered.Count);


            return services;
        }

        static readonly Type[] registerServiceSkipped =
        [
            typeof(IServiceProvider),
            typeof(IServiceScope),
            typeof(IServiceScopeFactory),
            typeof(IHostEnvironment),
            typeof(IHostApplicationLifetime),
        ];
    }


    internal class registerServiceItemRecord(Type type, Type? genericDeclarationType, ServiceLifetime lifetime, object? key, bool hostedService, RegisterServiceInheritsAttribute[] inherits)
    {
        public Type Type { get; } = type;

        public Type GenericDeclarationType { get; } = genericDeclarationType;

        public ServiceLifetime Lifetime { get; } = lifetime;

        public RegisterServiceInheritsAttribute[] Inherits { get; } = inherits;

        public object Key { get; } = key;

        public bool HostedService { get; } = hostedService;

        public NeedTypeRecord[] NeedTypes { get; set; }

        public record NeedTypeRecord(Type type, object key, Type? genericParent);
    }
}
