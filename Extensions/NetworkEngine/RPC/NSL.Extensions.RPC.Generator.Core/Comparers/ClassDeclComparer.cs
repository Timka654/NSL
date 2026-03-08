using NSL.Extensions.RPC.Generator.Core.Models;
using System.Collections.Generic;

namespace NSL.Extensions.RPC.Generator.Core.Comparers
{
    internal class ClassDeclComparer : EqualityComparer<ClassDeclModel>
    {
        public override bool Equals(ClassDeclModel x, ClassDeclModel y)
        {
            return x.Class.Identifier.Text.Equals(y.Class.Identifier.Text);
        }

        public override int GetHashCode(ClassDeclModel obj)
        {
            return obj.Class.GetHashCode();
        }
    }
}
