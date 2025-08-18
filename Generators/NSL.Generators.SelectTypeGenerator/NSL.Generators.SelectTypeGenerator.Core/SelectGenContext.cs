using Microsoft.CodeAnalysis;
using NSL.Generators.Utils;
using System.Collections.Generic;

namespace NSL.Generators.SelectTypeGenerator
{
    public class SelectGenContext
    {
        public ITypeSymbol OriginType { get; set; }
        public ITypeSymbol Type { get; set; }

        public IEnumerable<ISymbol> Symbols { get; set; }

        public string Model { get; set; }


        public string GenericDefinition { get; set; }

        public bool Typed { get; set; }

        public string MemberName { get; set; }

        public List<SelectGenContext> SubTypeList { get; set; }

        public virtual string GetTypeIdentifier(bool canNullable = true)
            => Type.GetTypeFullName(canNullable);

        public SelectGenContext Parent { get; internal set; }
    }
}
