using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSL.Generators.BinaryGenerator
{
    public class BinaryGeneratorContext
    {
        public SemanticModel SemanticModel;

        public virtual bool IsIgnore(ISymbol symbol) => false;
    }
}
