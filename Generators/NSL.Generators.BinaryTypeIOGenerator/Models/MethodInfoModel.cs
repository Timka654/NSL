using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;

namespace NSL.Generators.BinaryTypeIOGenerator.Models
{
    internal class MethodInfoModel
    {
        public ClassDeclarationSyntax ClassDeclarationSyntax { get; internal set; }

        public IOTypeEnum IOType { get; set; }

        public string ForGroup { get; set; }

        public Func<string, bool> ModelSelector { get; set; }

        public string MethodModifier { get; set; }

        public string MethodName { get; set; }

        public string ReadType { get; set; }

        public List<parametermodel> Parameters { get; set; }

        public MethodInfoModel()
        {
            ModelSelector = n => string.Equals(ForGroup, "*") || string.Equals(ForGroup, n); // old implementation - BIO has replace this
        }
    }


    public class parametermodel
    {
        public string name;
        //public ParameterSyntax parameter;

        //public IdentifierNameSyntax type;
        public string typeName;
    }
    internal enum IOTypeEnum
    {
        Read,
        Write
    }
}
