using Microsoft.CodeAnalysis;
using NSL.Generators.BinaryGenerator;
using System;
using System.Linq;

namespace NSL.Extensions.RPC.Generator
{
    internal class RPCBinaryGeneratorContext : BinaryGeneratorContext
    {
        public string[] IgnorePaths { get; set; }

        public override bool IsIgnore(ISymbol symbol, string path)
        {
            path = string.Join(".", path.Split('.').Skip(1));

            return RPCGenerator.IsIgnoreMember(symbol) || IgnorePaths?.Any(x => x.Equals(path, StringComparison.InvariantCultureIgnoreCase)) == true || IgnorePaths.Any(x => x.Equals("*"));
        }
    }
}
