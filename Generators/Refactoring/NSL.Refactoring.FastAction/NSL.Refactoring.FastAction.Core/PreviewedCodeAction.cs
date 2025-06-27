using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NSL.Refactoring.FastAction
{
    internal class PreviewedCodeAction : CodeAction
    {
        public delegate Task<Solution> ActionDelegate(CancellationToken cancellationToken, bool isPreview);

        public override string Title { get; }
        public ActionDelegate Action { get; }

        public PreviewedCodeAction(string title, ActionDelegate action)
        {
            Title = title;
            Action = action;
        }

        protected override async Task<IEnumerable<CodeActionOperation>> ComputePreviewOperationsAsync(
            CancellationToken cancellationToken)
        {
            // Content copied from http://sourceroslyn.io/#Microsoft.CodeAnalysis.Workspaces/CodeActions/CodeAction.cs,81b0a0866b894b0e,references
            var changedSolution = await Action(cancellationToken, true);
            if (changedSolution == null)
                return null;

            return new CodeActionOperation[] { new ApplyChangesOperation(changedSolution) };
        }

        protected override Task<Solution> GetChangedSolutionAsync(
            CancellationToken cancellationToken)
        {
            return Action(cancellationToken, false);
        }

        protected override CodeActionPriority ComputePriority()
        {
            return Priority;
        }

        public CodeActionPriority Priority { get; set; } = CodeActionPriority.Default;

        public static PreviewedCodeAction Create(string title, ActionDelegate action, CodeActionPriority priority = CodeActionPriority.Default)
            => new PreviewedCodeAction(title, action) { Priority = priority};
    }
}
