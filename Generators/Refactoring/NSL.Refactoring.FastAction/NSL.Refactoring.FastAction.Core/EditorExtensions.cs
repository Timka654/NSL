using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using System.Collections.Generic;

namespace NSL.Refactoring.FastAction.Core
{
    public static class EditorExtensions
    {
        public static void AddAttributes(this SyntaxEditor e, SyntaxNode n, IEnumerable<AttributeListSyntax> attributes)
        {
            foreach (var item in attributes)
            {
                e.AddAttribute(n, item);
            }
        }
    }
}
