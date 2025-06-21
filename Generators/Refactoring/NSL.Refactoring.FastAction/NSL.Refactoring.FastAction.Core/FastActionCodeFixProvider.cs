using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.ObjectModel;
using System.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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