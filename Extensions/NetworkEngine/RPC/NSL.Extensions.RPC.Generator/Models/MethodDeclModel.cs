using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace NSL.Extensions.RPC.Generator.Declarations
{
    internal class MethodDeclModel
    {
        public ClassDeclModel Class { get; set; }

        public string Name { get; set; }

        public IEnumerable<MethodDeclarationSyntax> Overrides { get; set; }
    }
}
