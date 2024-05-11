using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace NSL.Refactoring.Shared
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(SharedModelCodeRefactoringProvider)), Shared]
    internal class SharedModelCodeRefactoringProvider : CodeRefactoringProvider
    {
        static Dictionary<string, (string path, string btn)> models = new Dictionary<string, (string, string)>()
        {
            { "RequestModel",("shared_request_models_rel_path", "Generate request model \"{0}\"") },
            { "ResponseModel",("shared_response_models_rel_path", "Generate response model \"{0}\"") },
            { "DataModel", ("shared_data_models_rel_path", "Generate data model \"{0}\"") },
            { "Model", ("shared_models_rel_path", "Generate shared model \"{0}\"") }
        };

        public sealed override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
        {
            // TODO: Replace the following code with your own analysis, generating a CodeAction for each refactoring to offer

            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            // Find the node at the selection.
            var node = root.FindNode(context.Span);

            // Only offer a refactoring if the selected node is a type declaration node.
            var typeDecl = node as IdentifierNameSyntax;

            if (typeDecl == null)
                return;
#if DEBUGEXAMPLES
            else
                Debug.WriteLine($"type is not IdentifierNameSyntax - {node.GetType().Name}");
#endif

            //var opt = context.Document.Project.;
            Dictionary<string, string> options = null;

            var sol = context.Document.Project.Solution;

            var sol_dir = Path.GetDirectoryName(sol.FilePath);

            var confPath = Path.Combine(sol_dir, "NSLGen.options");

            if (File.Exists(confPath))
            {
                options = File.ReadAllLines(confPath).Select(x => x.Split('=')).ToDictionary(x => x[0], x => string.Join("=", x.Skip(1)));
            }


            var proj = context.Document.Project;

            var proj_dir = Path.GetDirectoryName(proj.FilePath);

            confPath = Path.Combine(proj_dir, "NSLGen.options");

            if (File.Exists(confPath))
            {
                var poptions = File.ReadAllLines(confPath).Select(x => x.Split('=')).ToDictionary(x => x[0], x => string.Join("=", x.Skip(1)));

                if (options == null)
                    options = poptions;
                else
                    foreach (var item in poptions)
                    {
                        options[item.Key] = item.Value;
                    }
            }
            else if (options == null)
            {
                return;
            }

            options.TryGetValue("shared_project_name", out var sharedProjName);

            var sharedProj = sol.Projects.FirstOrDefault(x => x.Name.Equals(sharedProjName));

            if (sharedProj == null)
                return;

            var sharedRootPath = Path.GetDirectoryName(sharedProj.FilePath);

            foreach (var item in models)
            {
                if (!typeDecl.Identifier.Text.Contains(item.Key))
                    continue;

                string key = item.Value.path;

                var modelsFullPath = sharedRootPath;

                if (options.TryGetValue(key, out var sharedModelsRelPath))
                    modelsFullPath = Path.Combine(modelsFullPath, sharedModelsRelPath);

                context.RegisterRefactoring(PreviewedCodeAction.Create(string.Format(item.Value.btn, typeDecl.Identifier.Text), (c, preview) => CreateSharedModel(sharedProj, typeDecl, modelsFullPath, sharedRootPath, context.Document, c, preview)));
            }
        }

        private async Task<Solution> CreateSharedModel(Project sharedProj, IdentifierNameSyntax typeDecl, string modelsFullPath, string sharedRootPath, Document sourceDoc, CancellationToken cancellationToken, bool preview)
        {
            // Produce a reversed version of the type declaration's identifier token.
            var identifierToken = typeDecl.Identifier;
            var newName = identifierToken.Text;

            var relPath = modelsFullPath.Substring(sharedRootPath.Length).Replace('\\','/').TrimStart('/');

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

            var assemblyName = this.GetType().Assembly.GetName().Name;

            var resPrefix = $"{assemblyName}.Templates.Shared.";

            var names =
    System
    .Reflection
    .Assembly
    .GetExecutingAssembly()
    .GetManifestResourceNames()
    .Where(x => x.StartsWith(resPrefix))
    .ToArray();

            Solution s = sharedProj.Solution;

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

                    var content = new StreamReader(stream).ReadToEnd();

                    content = content.Replace("$rootnamespace$", ns);
                    content = content.Replace("$safeitemname$", newName);
                    var doc = sharedProj.AddDocument(name, content, filePath: endPath);

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
