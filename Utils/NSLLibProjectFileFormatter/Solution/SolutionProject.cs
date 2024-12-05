namespace NSLLibProjectFileFormatter.Solution
{
    public class SolutionProject
    {
        public ProjectFileInfo Info { get; set; }

        public Guid Id { get; internal set; }

        public string Path { get; set; }

        public string Name { get; set; }
    }
}
