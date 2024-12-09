using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NSL.Extensions.RPC.Generator.Models
{
    internal class ProcessFieldReaderModel : MethodContextModel
    {
        public ParameterSyntax Parameter { get; set; }
    }
}
