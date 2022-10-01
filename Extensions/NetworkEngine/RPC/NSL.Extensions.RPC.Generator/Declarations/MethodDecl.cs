using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace NSL.Extensions.RPC.Generator.Declarations
{
    internal class MethodDecl
    {
        public ClassDecl Class { get; set; }

        public string Name { get; set; }

        public IEnumerable<MethodDeclarationSyntax> Overrides { get; set; }
    }
}
