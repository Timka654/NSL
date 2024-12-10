using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace NSL.Refactoring.Shared
{
    internal class SharedBuilder
    {
        public static readonly Dictionary<string, (string path, string title)> SharedModelsData = new Dictionary<string, (string, string)>()
        {
            { "RequestModel",("shared_request_models_rel_path", "Generate request model \"{0}\"") },
            { "ResponseModel",("shared_response_models_rel_path", "Generate response model \"{0}\"") },
            { "DataModel", ("shared_data_models_rel_path", "Generate data model \"{0}\"") },
            { "EntityModel", ("shared_entity_models_rel_path", "Generate entity model \"{0}\"") },
            { "Model", ("shared_models_rel_path", "Generate model \"{0}\"") }
        };

        public const string SharedTemplateRelativePath = "Templates.Shared.";
        public const string EntityTemplateRelativePath = "Templates.Entity.";

        public delegate Task<Document> ModelProcessingDelegate(Document document);

        public delegate string DocumentContentPostProcessingDelegate(string fileName, string sourceCode);

        public static async Task<Solution> CreateSharedModel(
            Project sharedProj,
            string newName,
            string modelsFullPath,
            string sharedRootPath,
            Document sourceDoc,
            CancellationToken cancellationToken,
            bool preview,
            ModelProcessingDelegate onModelProcessing = null,
            Solution sharedSolution = null,
            string templateRelPath = SharedTemplateRelativePath,
            DocumentContentPostProcessingDelegate onPostDocumentContent = null)
        {
            // Produce a reversed version of the type declaration's identifier token.

            var relPath = modelsFullPath.Substring(sharedRootPath.Length).Replace('\\', '/').TrimStart('/');

            //namespace

            var ns = relPath.Replace('\\', '.').Replace('/', '.');

            ns = string.Join(".", sharedProj.DefaultNamespace, ns).Trim('.');

            var ns_sp = ns.Split('.');

            for (int i = 0; i < ns_sp.Length; i++)
            {
                if (char.IsDigit(ns_sp[i][0]))
                    ns_sp[i] = $"_{ns_sp[i]}";

                for (int ci = 0; ci < ns_sp[i].Length; ci++)
                {
                    if (ns_sp[i][ci] == '_')
                        continue;

                    if (!char.IsLetterOrDigit(ns_sp[i][ci]))
                        ns_sp[i] = ns_sp[i].Replace(ns_sp[i][ci], '_');
                }
            }

            ns = string.Join(".", ns_sp); // clear namespace

            //templates

            if (!Directory.Exists(modelsFullPath) && !preview)
                Directory.CreateDirectory(modelsFullPath);

            var assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

            var resPrefix = string.Join(".", assemblyName, templateRelPath);

            var names =
    System
    .Reflection
    .Assembly
    .GetExecutingAssembly()
    .GetManifestResourceNames()
    .Where(x => x.StartsWith(resPrefix))
    .ToArray();

            Solution s = sharedSolution ?? sharedProj.Solution;

            sharedProj = s.GetProject(sharedProj.Id);

            foreach (var item in names)
            {
                using (var stream = Assembly
                .GetExecutingAssembly()
    .GetManifestResourceStream(item))
                {
                    var name = item.Substring(resPrefix.Length);

                    name = string.Join(".", newName, string.Join(".", name.Split('.').Skip(1).ToArray()));

                    var endPath = Path.Combine(modelsFullPath, name);

                    if (sharedProj.Documents.Any(x => Equals(x.FilePath, endPath)))
                        continue;

                    var srcContent = new StreamReader(stream).ReadToEnd();

                    srcContent = srcContent.Replace("$rootnamespace$", ns);
                    srcContent = srcContent.Replace("$safeitemname$", newName);

                    if (onPostDocumentContent != null)
                        srcContent = onPostDocumentContent(name, srcContent);

                    var doc = sharedProj.AddDocument(name, srcContent, filePath: endPath);


                    if (onModelProcessing != null)
                        doc = await onModelProcessing(doc);

                    sharedProj = doc.Project;

                    s = sharedProj.Solution;
                }
            }

            //link

            var srcProj = s.GetProject(sourceDoc.Project.Id);

            if (!srcProj.ProjectReferences.Any(x => x.ProjectId == sharedProj.Id) && !srcProj.Id.Equals(sharedProj.Id))
                srcProj = srcProj.AddProjectReference(new ProjectReference(sharedProj.Id));

            sourceDoc = srcProj.GetDocument(sourceDoc.Id);

            var synRoot = await sourceDoc.GetSyntaxRootAsync(cancellationToken);

            if (synRoot is CompilationUnitSyntax cus)
            {
                if (cus.Members.FirstOrDefault() is NamespaceDeclarationSyntax cns && cns.Name.ToString() == ns)
                {

                }
                else if (!cus.Usings.Any(x => Equals(x.Name.ToString(), ns)))
                {
                    var us = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(ns));

                    cus = cus.AddUsings(us);

                    srcProj = sourceDoc.WithSyntaxRoot(cus).Project;
                }
            }

            s = srcProj.Solution;


            return s;
        }
    }
}
