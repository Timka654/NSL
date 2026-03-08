using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;

namespace NSL.Refactoring.FastAction.Core
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(FastActionCodeRefactoringProvider)), Shared]
    internal class FastActionCodeRefactoringProvider : CodeRefactoringProvider
    {
        public override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
        {
            var actions = await FastActionBuilder.BuildActions(context.Document, context.Span, context.CancellationToken);

            if (!actions.Any())
                return;

            var groups = CodeActionGrouper.GroupActions(actions);

            foreach (var action in groups)
            {
                context.RegisterRefactoring(action);
            }
        }
    }
}