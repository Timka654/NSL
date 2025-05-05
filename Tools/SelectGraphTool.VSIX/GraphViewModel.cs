using GraphSharp.Controls;
using QuickGraph;
using System.Collections.Generic;
using System.Windows;

namespace SelectGraphTool
{
    public class ModelGraphLayout : GraphLayout<ModelNode, ModelEdge, IBidirectionalGraph<ModelNode, ModelEdge>> {
    }


    public class GraphViewModel
    {
        public readonly BidirectionalGraph<ModelNode, ModelEdge> Graph = new();


        private Dictionary<string, ModelNode> _nodes = new Dictionary<string, ModelNode>();

        private ModelNode GetOrAddNode(string name)
        {
            if (!_nodes.TryGetValue(name, out var node))
            {
                node = new ModelNode { Name = name };
                Graph.AddVertex(node);
                _nodes[name] = node;
            }
            return node;
        }

        public void AddEdge(string from, string to, string type)
        {
            var source = GetOrAddNode(from);
            var target = GetOrAddNode(to);
            Graph.AddEdge(new ModelEdge(source, target, type));
        }
    }

}