﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NSL.Generators.SelectTypeGenerator
{
    internal class GenTypeGroup
    {
        public string Name { get; set; }

        public string Namespace { get; set; }

        public ITypeSymbol TypeSymbol { get; set; }

        public TypeDeclarationSyntax TypeDeclaration { get; set; }

        //public SemanticModel SemanticModel { get; set; }

        public GenAttribute[] Attributes { get; set; }

        public ISymbol[] Members { get; set; }
    }
}
