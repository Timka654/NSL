using QuickGraph;

namespace SelectGraphTool
{
    public class ModelEdge : Edge<ModelNode>
    {
        public string Type { get; }
        public ModelEdge(ModelNode source, ModelNode target, string type)
            : base(source, target) => Type = type;
    }

}