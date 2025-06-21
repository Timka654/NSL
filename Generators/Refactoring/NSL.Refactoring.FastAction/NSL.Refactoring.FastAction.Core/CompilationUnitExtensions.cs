using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace NSL.Refactoring.FastAction.Core
{
    public static class CompilationUnitExtensions
    {
        public static CompilationUnitSyntax AddUsingSafe(this CompilationUnitSyntax root, string namespaceName)
        {
            if (root.Usings.Any(u => u.Name.ToString() == namespaceName))
                return root;

            var newUsing = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(namespaceName))
                .WithTrailingTrivia(SyntaxFactory.ElasticCarriageReturnLineFeed);

            return root.WithUsings(root.Usings.Insert(0, newUsing));
        }
    }
}
