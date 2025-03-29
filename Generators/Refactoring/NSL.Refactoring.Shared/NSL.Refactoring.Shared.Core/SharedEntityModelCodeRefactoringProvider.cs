using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NSL.Refactoring.Shared
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(SharedEntityModelCodeRefactoringProvider)), Shared]
    internal class SharedEntityModelCodeRefactoringProvider : CodeRefactoringProvider
    {
        public override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
        {
            // TODO: Replace the following code with your own analysis, generating a CodeAction for each refactoring to offer

            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            // Find the node at the selection.
            var node = root.FindNode(context.Span);

            // Only offer a refactoring if the selected node is a type declaration node.
            var typeDecl = node as ClassDeclarationSyntax;

            if (typeDecl == null)
                return;
#if DEBUGEXAMPLES
            else
                Debug.WriteLine($"type is not IdentifierNameSyntax - {node.GetType().Name}");
#endif

            if (typeDecl.Identifier.Text.EndsWith("EntityModel") || !typeDecl.Identifier.Text.EndsWith("Model"))
                return;

            Dictionary<string, string> options = NSLGenOptions.TryLoadOptions(context.Document.Project);

            if (options == null)
                return;

            options.TryGetValue("shared_project_name", out var sharedProjName);

            var sharedProj = context.Document.Project.Solution.Projects.FirstOrDefault(x => x.Name.Equals(sharedProjName));

            if (sharedProj == null)
            {
                sharedProj = context.Document.Project;
            }

            var sharedRootPath = Path.GetDirectoryName(sharedProj.FilePath);

            //var item = SharedBuilder.SharedModelsData["EntityModel"];

            string key = SharedBuilder.SharedModelsData["EntityModel"].path;

            var modelsFullPath = sharedRootPath;

            if (options.TryGetValue(key, out var entityModelsRelPath))
                modelsFullPath = Path.Combine(modelsFullPath, entityModelsRelPath);

            var modelIdStartIdx = typeDecl.Identifier.Text.LastIndexOf("Model");

            var defaultIdentifier = typeDecl.Identifier.Text.Substring(0, modelIdStartIdx);

            var createIdentifier = $"{defaultIdentifier}EntityModel";

            context.RegisterRefactoring(PreviewedCodeAction.Create(
                string.Format("Generate entity model \"{0}\"", createIdentifier),
                async (c, preview) => await SharedBuilder.CreateSharedModel(
                    typeDecl,
                    sharedProj,
                    createIdentifier,
                    modelsFullPath,
                    sharedRootPath,
                    context.Document,
                    await context.Document.GetSemanticModelAsync(),
                    c,
                    preview,
                    templateRelPath: SharedBuilder.EntityTemplateRelativePath,
                    onPostDocumentContent: (fileName, srcContent) =>
                    {
                        return srcContent.Replace("$safesrcitemname$", typeDecl.Identifier.Text);
                    },
                    onModelProcessing: async (endDocument, sem) =>
                    {
                        var synRoot = await endDocument.GetSyntaxRootAsync(c);

                        var srcNamespace = (typeDecl.Parent as NamespaceDeclarationSyntax);

                        var ns = srcNamespace.Name.ToString();

                        if (synRoot is CompilationUnitSyntax cus)
                        {
                            if (cus.Members.FirstOrDefault() is NamespaceDeclarationSyntax cns && cns.Name.ToString() == srcNamespace.Name.ToString())
                            {

                            }
                            else if (!cus.Usings.Any(x => Equals(x.Name.ToString(), ns)))
                            {
                                var us = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(ns));

                                cus = cus.AddUsings(us);

                                endDocument = endDocument.WithSyntaxRoot(cus);
                            }
                        }

                        return endDocument;
                    })));

        }

    }
}
