using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Composition;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CodeRefactoring1
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

            //var opt = context.Document.Project.;

            var sol = context.Document.Project.Solution;

            var sol_dir = Path.GetDirectoryName(sol.FilePath);

            var confPath = Path.Combine(sol_dir, "NSLGen.options");

            if (!File.Exists(confPath))
                return;

            var options = File.ReadAllLines(confPath).Select(x => x.Split('=')).ToDictionary(x => x[0], x => string.Join("=", x.Skip(1)));



            var proj = context.Document.Project;

            var proj_dir = Path.GetDirectoryName(proj.FilePath);

            confPath = Path.Combine(proj_dir, "NSLGen.options");

            if (File.Exists(confPath))
            {
                var poptions = File.ReadAllLines(confPath).Select(x => x.Split('=')).ToDictionary(x => x[0], x => string.Join("=", x.Skip(1)));

                foreach (var item in poptions)
                {
                    options[item.Key] = item.Value;
                }
            }


                options.TryGetValue("shared_project_name", out var sharedProjName);

            var sharedProj = sol.Projects.FirstOrDefault(x => x.Name.Equals(sharedProjName));

            if (sharedProj == null)
                return;

            var sharedRootPath = Path.GetDirectoryName(sharedProj.FilePath);

            var modelsFullPath = sharedRootPath;

            foreach (var item in models)
            {
                if (!typeDecl.Identifier.Text.Contains(item.Key))
                    continue;

                string key = item.Value.path;
                if (options.TryGetValue(key, out var sharedModelsRelPath))
                    modelsFullPath = Path.Combine(modelsFullPath, sharedModelsRelPath);

                context.RegisterRefactoring(CodeAction.Create(string.Format(item.Value.btn, typeDecl.Identifier.Text), c => CreateSharedModel(sharedProj, typeDecl, modelsFullPath, sharedRootPath, context.Document, c)));
            }



            //if (typeDecl == null)
            //{
            //    return;
            //}

            //// For any type declaration node, create a code action to reverse the identifier text.
            //var action = CodeAction.Create("Reverse type name", c => ReverseTypeNameAsync(context.Document, typeDecl, c));

            //// Register this code action.
            //context.RegisterRefactoring(action);
        }

        private async Task<Solution> CreateSharedModel(Project sharedProj, IdentifierNameSyntax typeDecl, string modelsFullPath, string sharedRootPath, Document sourceDoc, CancellationToken cancellationToken)
        {
            // Produce a reversed version of the type declaration's identifier token.
            var identifierToken = typeDecl.Identifier;
            var newName = identifierToken.Text;

            var relPath = modelsFullPath.Substring(sharedRootPath.Length);

            var ns = relPath.Replace('\\', '.').Replace('/', '.');

            ns = string.Join(".", sharedProj.DefaultNamespace, ns).Trim('.');
            //// Get the symbol representing the type to be renamed.
            //var semanticModel = await sourceDoc.GetSemanticModelAsync(cancellationToken);

            //semanticModel.SyntaxTree.TryGetRoot(out var root);

            //var typeSymbol = semanticModel.GetDeclaredSymbol(typeDecl, cancellationToken);
            //semanticModel.
            //// Produce a new solution that has all references to that type renamed, including the declaration.
            //var originalSolution = document.Project.Solution;
            //var optionSet = originalSolution.Workspace.Options;
            var names =
    System
    .Reflection
    .Assembly
    .GetExecutingAssembly()
    .GetManifestResourceNames()
    .Where(x => x.StartsWith("CodeRefactoring1.Templates.Shared."))
    .ToArray();

            Solution s = sharedProj.Solution;

            foreach (var item in names)
            {
                using (var stream = Assembly
                .GetExecutingAssembly()
    .GetManifestResourceStream(item))
                {
                    var name = item.Substring("CodeRefactoring1.Templates.Shared.".Length);

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

            var srcProj = s.GetProject(sourceDoc.Project.Id);

            if (!srcProj.ProjectReferences.Any(x => x.ProjectId == sharedProj.Id))
                srcProj = srcProj.AddProjectReference(new ProjectReference(sharedProj.Id));

            sourceDoc = srcProj.GetDocument(sourceDoc.Id);

            var synRoot = await sourceDoc.GetSyntaxRootAsync(cancellationToken);

            if (synRoot is CompilationUnitSyntax cus)
            {
                if (!cus.Usings.Any(x => Equals(x.Name.ToString(), ns)))
                {
                    var us = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(ns));

                    cus = cus.AddUsings(us);
                    srcProj = sourceDoc.WithSyntaxRoot(cus).Project;
                    //semanticModel.sy semanticModel.SyntaxTree.WithRootAndOptions(cus, semanticModel.SyntaxTree.Options)
                    //SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(ns));
                }
            }

            s = srcProj.Solution;



            // Return the new solution with the now-uppercase type name.
            return s;
        }

        private async Task<Solution> ReverseTypeNameAsync(Document document, TypeDeclarationSyntax typeDecl, CancellationToken cancellationToken)
        {
            // Produce a reversed version of the type declaration's identifier token.
            var identifierToken = typeDecl.Identifier;
            var newName = new string(identifierToken.Text.ToCharArray().Reverse().ToArray());

            // Get the symbol representing the type to be renamed.
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
            var typeSymbol = semanticModel.GetDeclaredSymbol(typeDecl, cancellationToken);

            // Produce a new solution that has all references to that type renamed, including the declaration.
            var originalSolution = document.Project.Solution;
            var optionSet = originalSolution.Workspace.Options;
            var newSolution = await Renamer.RenameSymbolAsync(document.Project.Solution, typeSymbol, newName, optionSet, cancellationToken).ConfigureAwait(false);

            // Return the new solution with the now-uppercase type name.
            return newSolution;
        }
    }
}
