using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSL.Generators.BinaryTypeIOGenerator.Models
{
    internal class MethodInfoModel
    {
        public MethodDeclarationSyntax MethodDeclarationSyntax { get; set; }

        public IOTypeEnum IOType { get; set; }

        public string ForGroup { get; set; }

        public Dictionary<string, string> parameters { get; set; }
    }

    internal enum IOTypeEnum
    {
        Read,
        Write
    }
}
