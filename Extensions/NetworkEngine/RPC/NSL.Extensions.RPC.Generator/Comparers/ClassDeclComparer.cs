using NSL.Extensions.RPC.Generator.Declarations;
using System.Collections.Generic;

namespace NSL.Extensions.RPC.Generator.Comparers
{
    internal class ClassDeclComparer : EqualityComparer<ClassDecl>
    {
        public override bool Equals(ClassDecl x, ClassDecl y)
        {
            return x.Class.Identifier.Text.Equals(y.Class.Identifier.Text);
        }

        public override int GetHashCode(ClassDecl obj)
        {
            return obj.Class.GetHashCode();
        }
    }
}
