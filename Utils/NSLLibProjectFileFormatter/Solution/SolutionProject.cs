namespace NSLLibProjectFileFormatter.Solution
{
    public class SolutionProject
    {
        private Guid id;

        public ProjectFileInfo Info { get; set; }

        public Guid Id { get => id; internal set { id = value; UppedId = id.ToString().ToUpper(); } }

        public string UppedId { get; private set; }

        public string Path { get; set; }

        public string Name { get; set; }
    }
}
