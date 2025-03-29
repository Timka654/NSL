using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NSL.Generators.Utils;
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
                async (c, preview) => await SharedBuilder.CreateSharedModel(typeDecl, sharedProj, createIdentifier, modelsFullPath, sharedRootPath, context.Document,
                    await context.Document.GetSemanticModelAsync(), c, preview, (doc, sem) => BuildDocumentContent(typeDecl, doc, sem))));

            var editIdentifier = $"Edit{defaultIdentifier}RequestModel";

            context.RegisterRefactoring(PreviewedCodeAction.Create(
                string.Format("Generate edit request model \"{0}\"", editIdentifier),
                async (c, preview) => await SharedBuilder.CreateSharedModel(typeDecl, sharedProj, editIdentifier, modelsFullPath, sharedRootPath, context.Document,
                    await context.Document.GetSemanticModelAsync(), c, preview, (doc, sem) => BuildDocumentContent(typeDecl, doc, sem))));

            context.RegisterRefactoring(PreviewedCodeAction.Create(
                string.Format("Generate both request models for \"{0}\"", defaultIdentifier),
                async (c, preview) => await BothGenerate(sharedProj, defaultIdentifier, modelsFullPath, sharedRootPath, context.Document,
                    await context.Document.GetSemanticModelAsync(), c, preview, typeDecl)));
        }

        private async Task<Solution> BothGenerate(Project sharedProj, string defaultIdentifier, string modelsFullPath, string sharedRootPath, Document sourceDoc, SemanticModel semanticModel, CancellationToken cancellationToken, bool preview, ClassDeclarationSyntax rootNode)
        {
            var createIdentifier = $"Create{defaultIdentifier}RequestModel";

            var sol = await SharedBuilder.CreateSharedModel(rootNode, sharedProj, createIdentifier, modelsFullPath, sharedRootPath, sourceDoc, semanticModel, cancellationToken, preview, (doc, sem) => BuildDocumentContent(rootNode, doc, sem));

            var editIdentifier = $"Edit{defaultIdentifier}RequestModel";

            sol = await SharedBuilder.CreateSharedModel(rootNode, sharedProj, editIdentifier, modelsFullPath, sharedRootPath, sourceDoc, semanticModel , cancellationToken, preview, (doc, sem) => BuildDocumentContent(rootNode, doc, sem), sol);

            return sol;
        }

        private async Task<Document> BuildDocumentContent(ClassDeclarationSyntax srcRootNode, Document document, SemanticModel semanticModel)
        {
            if (!document.FilePath.EndsWith("Model.cs"))
                return document;

            var t = semanticModel.GetDeclaredSymbol(srcRootNode) as ITypeSymbol;

            var properties = t.GetAllMembers().Where(x => x is IPropertySymbol).Select(x => x as IPropertySymbol).ToArray();

            var droot = await document.GetSyntaxRootAsync() as CompilationUnitSyntax;

            var ns = droot.Members.First() as NamespaceDeclarationSyntax;

            var c = ns.Members.First() as ClassDeclarationSyntax;

            c = c.WithMembers(new SyntaxList<MemberDeclarationSyntax>(properties.Select(x => SyntaxFactory.PropertyDeclaration(SyntaxFactory.ParseTypeName(x.Type.GetTypeFullName()), x.Name)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddAccessorListAccessors(
                    SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                        .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                    SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                        .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
            ))));

            ns = ns.WithMembers(new SyntaxList<MemberDeclarationSyntax>(c));

            droot = droot.WithMembers(new SyntaxList<MemberDeclarationSyntax>(ns));

            return document.WithSyntaxRoot(droot);
        }

    }
}
