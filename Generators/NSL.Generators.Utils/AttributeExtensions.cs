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
            var val = semantic.GetConstantValue(syntax.Expression).Value;

            return (TResult)val;
        }

        public static ITypeSymbol GetAttributeTypeParameterValueSymbol(this AttributeArgumentSyntax syntax, SemanticModel semantic)
        {
            return semantic.GetSymbolInfo((syntax.Expression as TypeOfExpressionSyntax).Type).Symbol as ITypeSymbol;
        }

        public static TypeSyntax GetAttributeTypeParameterValueSyntax(this AttributeArgumentSyntax syntax)
        {
            return (syntax.Expression as TypeOfExpressionSyntax).Type;
        }

        public static string GetAttributeTypeParameterValueName(this AttributeArgumentSyntax syntax)
        {
            return (syntax.Expression as TypeOfExpressionSyntax).Type.GetText().ToString();
        }
    }
}
