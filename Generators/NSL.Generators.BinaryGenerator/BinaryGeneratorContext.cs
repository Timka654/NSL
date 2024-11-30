using Microsoft.CodeAnalysis;
using NSL.Generators.Utils;
using System.Collections.Generic;

namespace NSL.Generators.BinaryGenerator
{
    public class BinaryGeneratorContext
    {
        public string For { get; set; }

        internal int ProcessingLevel { get; set; }

        public List<(ISymbol, string)> CurrentPath { get; set; } = new List<(ISymbol, string)>();

        public SemanticModel SemanticModel;

        public GeneratorExecutionContext Context { get; set; }
        public ISymbol CurrentMember { get; internal set; }

        public virtual bool IsIgnore(ISymbol symbol, string path) => false;

        public virtual string GetExistsReadHandleCode(ISymbol symbol, string path, CodeBuilder codeBuilder) => default;

        public virtual string GetExistsWriteHandleCode(ISymbol symbol, string path, CodeBuilder codeBuilder) => default;


        public virtual bool OpenTypeEntry(ISymbol symbol, string path, CodeBuilder codeBuilder, bool read) => true;
        public virtual void CloseTypeEntry(ISymbol symbol, string path) { }
    }
}
