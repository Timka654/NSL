using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NSL.Refactoring.Shared
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(SharedModelCodeFixProvider)), Shared]
    internal class SharedModelCodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(
            "CS0246");

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
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


            foreach (var diag in context.Diagnostics)
            {
                foreach (var item in SharedBuilder.SharedModelsData)
                {
                    if (!typeDecl.Identifier.Text.Contains(item.Key) || item.Key.Equals("EntityModel"))
                        continue;

                    string key = item.Value.path;

                    var modelsFullPath = sharedRootPath;

                    if (options.TryGetValue(key, out var sharedModelsRelPath))
                        modelsFullPath = Path.Combine(modelsFullPath, sharedModelsRelPath);


                    var className = typeDecl.Identifier.Text;

                    context.RegisterCodeFix(PreviewedCodeAction.Create(string.Format(item.Value.title, typeDecl.Identifier.Text), async (c, preview) => await SharedBuilder.CreateSharedModel(node, sharedProj, className, modelsFullPath, sharedRootPath, context.Document,
                    await context.Document.GetSemanticModelAsync(), c, preview)), diag);
                }
            }
        }

    }
}
