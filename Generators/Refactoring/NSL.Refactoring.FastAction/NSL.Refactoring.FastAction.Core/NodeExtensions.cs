using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
namespace NSL.Refactoring.FastAction.Core
{
    internal static class NodeExtensions
    {
        public static NamespaceDeclarationSyntax GetParentNamespace(this SyntaxNode node)
        {
            return GetParentOfType<NamespaceDeclarationSyntax>(node);
        }

        public static TType GetParentOfType<TType>(this SyntaxNode node, bool startsFromParent = false)
            where TType : SyntaxNode
        {
            SyntaxNode currentNode = startsFromParent ? node.Parent : node;

            while (currentNode != null && !(currentNode is TType))
            {
                currentNode = currentNode.Parent;
            }

            return currentNode as TType;
        }
    }
}
