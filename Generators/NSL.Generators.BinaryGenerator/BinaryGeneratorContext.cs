using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSL.Generators.BinaryGenerator
{
    public class BinaryGeneratorContext
    {
        internal int ProcessingLevel { get; set; }

        public SemanticModel SemanticModel;

        public virtual bool IsIgnore(ISymbol symbol, string path) => false;

        public virtual string GetExistsReadHandleCode(ISymbol symbol, string path) => default;

        public virtual string GetExistsWriteHandleCode(ISymbol symbol, string path) => default;
    }
}
