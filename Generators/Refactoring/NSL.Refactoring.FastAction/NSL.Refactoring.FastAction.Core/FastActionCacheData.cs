namespace NSL.Refactoring.FastAction.Core
{
    internal class FastActionCacheData { 
        public string ProjectPath { get; set; } = default;
        
        public string SolutionPath { get; set; } = default;

        public FastActionData Data { get; set; }
    }


}
