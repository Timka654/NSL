using System.Collections.Generic;

namespace SelectGraphTool
{
    public class ModelNode
    {
        public string Name { get; set; }
        public List<string> Properties { get; set; } = new(); // Свойства (Include, Proxy, и т.д.)

        public override string ToString()
            => $"{Name}\n" + string.Join("\n", Properties);
    }

}