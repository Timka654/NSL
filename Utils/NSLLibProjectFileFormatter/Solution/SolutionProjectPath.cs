namespace NSLLibProjectFileFormatter.Solution
{
    public class SolutionProjectPath
    {
        public List<SolutionProject> Projects { get; set; } = new();

        public string Dir { get; set; }

        public Guid Id { get; set; } = Guid.NewGuid();

        public Dictionary<string, SolutionProjectPath> Pathes { get; set; } = new();

        public SolutionProjectPath Parent { get; set; }

        public override string ToString()
        => $"{Dir} (Parent = {Parent?.Dir ?? "{none}"})";
    }
}
