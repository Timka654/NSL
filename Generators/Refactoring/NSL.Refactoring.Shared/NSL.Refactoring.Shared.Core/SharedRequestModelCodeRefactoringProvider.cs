using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NSL.Refactoring.Shared
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(SharedRequestModelCodeRefactoringProvider)), Shared]
    internal class SharedRequestModelCodeRefactoringProvider : CodeRefactoringProvider
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

            if (typeDecl.Identifier.Text.EndsWith("RequestModel") || !typeDecl.Identifier.Text.EndsWith("Model"))
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

            //var item = SharedBuilder.SharedModelsData["RequestModel"];

            string key = SharedBuilder.SharedModelsData["RequestModel"].path;

            var modelsFullPath = sharedRootPath;

            if (options.TryGetValue(key, out var sharedModelsRelPath))
                modelsFullPath = Path.Combine(modelsFullPath, sharedModelsRelPath);

            var modelIdStartIdx = typeDecl.Identifier.Text.LastIndexOf("Model");

            var defaultIdentifier = typeDecl.Identifier.Text.Substring(0, modelIdStartIdx);

            var createIdentifier = $"Create{defaultIdentifier}RequestModel";

            context.RegisterRefactoring(PreviewedCodeAction.Create(
                string.Format("Generate create request model \"{0}\"", createIdentifier),
                (c, preview) => SharedBuilder.CreateSharedModel(sharedProj, createIdentifier, modelsFullPath, sharedRootPath, context.Document, c, preview, doc => BuildDocumentContent(typeDecl, doc))));

            var editIdentifier = $"Edit{defaultIdentifier}RequestModel";

            context.RegisterRefactoring(PreviewedCodeAction.Create(
                string.Format("Generate edit request model \"{0}\"", editIdentifier),
                (c, preview) => SharedBuilder.CreateSharedModel(sharedProj, editIdentifier, modelsFullPath, sharedRootPath, context.Document, c, preview, doc => BuildDocumentContent(typeDecl, doc))));

            context.RegisterRefactoring(PreviewedCodeAction.Create(
                string.Format("Generate both request models for \"{0}\"", defaultIdentifier),
                (c, preview) => BothGenerate(sharedProj, defaultIdentifier, modelsFullPath, sharedRootPath, context.Document, c, preview, typeDecl)));
        }

        private async Task<Solution> BothGenerate(Project sharedProj, string defaultIdentifier, string modelsFullPath, string sharedRootPath, Document sourceDoc, CancellationToken cancellationToken, bool preview, ClassDeclarationSyntax rootNode)
        {
            var createIdentifier = $"Create{defaultIdentifier}RequestModel";

            var sol = await SharedBuilder.CreateSharedModel(sharedProj, createIdentifier, modelsFullPath, sharedRootPath, sourceDoc, cancellationToken, preview, doc => BuildDocumentContent(rootNode, doc));

            var editIdentifier = $"Edit{defaultIdentifier}RequestModel";

            sol = await SharedBuilder.CreateSharedModel(sharedProj, editIdentifier, modelsFullPath, sharedRootPath, sourceDoc, cancellationToken, preview, doc => BuildDocumentContent(rootNode, doc), sol);

            return sol;
        }

        private async Task<Document> BuildDocumentContent(ClassDeclarationSyntax srcRootNode, Document document)
        {
            if (!document.FilePath.EndsWith("Model.cs"))
                return document;

            var properties = srcRootNode.Members.Where(x => x is PropertyDeclarationSyntax).Select(x => x as PropertyDeclarationSyntax).ToArray();

            var droot = await document.GetSyntaxRootAsync() as CompilationUnitSyntax;

            var ns = droot.Members.First() as NamespaceDeclarationSyntax;

            var c = ns.Members.First() as ClassDeclarationSyntax;

            c = c.WithMembers(new SyntaxList<MemberDeclarationSyntax>(properties.Select(x=>x.WithAttributeLists(new SyntaxList<AttributeListSyntax>()))));

            ns = ns.WithMembers(new SyntaxList<MemberDeclarationSyntax>(c));

            droot = droot.WithMembers(new SyntaxList<MemberDeclarationSyntax>(ns));

            return document.WithSyntaxRoot(droot);
        }

    }
}
