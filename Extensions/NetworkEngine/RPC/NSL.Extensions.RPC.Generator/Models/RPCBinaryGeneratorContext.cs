using Microsoft.CodeAnalysis;
using NSL.Generators.BinaryGenerator;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSL.Extensions.RPC.Generator.Models
{
    internal class RPCBinaryGeneratorContext : BinaryGeneratorContext
    {
        public override bool IsIgnore(ISymbol symbol)
            => RPCGenerator.IsIgnoreMember(symbol);
    }
}
