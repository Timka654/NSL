using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSL.Generators.BinaryTypeIOGenerator.Models
{
    internal class MethodInfoModel
    {
        public ClassDeclarationSyntax ClassDeclarationSyntax { get; internal set; }

        public MethodDeclarationSyntax MethodDeclarationSyntax { get; set; }

        public IOTypeEnum IOType { get; set; }

        public string ForGroup { get; set; }

        public List<parametermodel> Parameters { get; set; }
    }


    public class parametermodel
    {
        public string name;
        public ParameterSyntax parameter;
    }
    internal enum IOTypeEnum
    {
        Read,
        Write
    }
}
