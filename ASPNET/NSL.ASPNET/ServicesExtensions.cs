using HtmlAgilityPack;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Threading.Tasks;

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
        public static bool RebuildIndexFileVersion(this IHost host, bool removeInputAfterProcess = true)
        {
            var updateIndexFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "index.html.update");

            var indexFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "index.html");

            return host.RebuildFileVersion(updateIndexFilePath, indexFilePath, removeInputAfterProcess);
        }

        public static bool RebuildFileVersion(this IHost host, string inputFilePath, string outputFilePath, bool removeInputAfterProcess = true)
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
                        src = $"{src}?ver={ver}";
                        item.SetAttributeValue("src", src);
                    }
                }
                else if (item.Name == "link")
                {
                    var href = item.GetAttributeValue("href", string.Empty);

                    if (!href.StartsWith("http") && !string.IsNullOrEmpty(href))
                    {
                        href = $"{href}?ver={ver}";
                        item.SetAttributeValue("href", href);
                    }
                }

                ProcessIndexHtmlElement(item, ver);
            }
        }
    }
}
