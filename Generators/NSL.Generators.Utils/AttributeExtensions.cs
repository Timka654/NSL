using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSL.Generators.Utils
{
    public static class AttributeExtensions
    {
        public static TResult GetAttributeParameterValue<TResult>(this AttributeArgumentSyntax syntax, SemanticModel semantic)
        {
            return (TResult)semantic.GetConstantValue(syntax.Expression).Value;
        }
    }
}
