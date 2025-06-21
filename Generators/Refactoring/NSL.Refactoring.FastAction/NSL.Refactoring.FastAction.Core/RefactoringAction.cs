namespace NSL.Refactoring.FastAction.Core
{
    internal class RefactoringAction
    {
        public PreviewedCodeAction.ActionDelegate Action { get; set; }

        public string Path { get; set; }
    }
}
