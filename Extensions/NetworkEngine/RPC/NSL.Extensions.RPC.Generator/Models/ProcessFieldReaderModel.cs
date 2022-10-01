using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSL.Extensions.RPC.Generator.Models
{
    internal class ProcessFieldReaderModel : MethodContextModel
    {
        public ParameterSyntax Parameter { get; set; }
    }
}
