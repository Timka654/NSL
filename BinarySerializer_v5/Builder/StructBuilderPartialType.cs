using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace BinarySerializer.Builder
{
    public class StructBuilderPartialType<T, PrevT> : StructBuilder<T>
    {
        private StructBuilderProperty<PrevT> prevProp;

        protected StructBuilderPartialType(TypeStorage storage, StructBuilderProperty<PrevT> p) : base(storage)
        {
            prevProp = p;
        }

        public static StructBuilderPartialType<T, PrevT> GetStruct(StructBuilderProperty<PrevT> p, TypeStorage storage = null)
        {
            if (storage == null)
                storage = TypeStorage.Instance;

            return new StructBuilderPartialType<T, PrevT>(storage, p);
        }

        public new StructBuilderPartialTypeProperty<T, PrevT> GetProperty(Expression<Func<T, object>> GetPropertyLambda)
        {
            MemberExpression Exp = null;

            //this line is necessary, because sometimes the expression comes in as Convert(originalexpression)
            if (GetPropertyLambda.Body is UnaryExpression)
            {
                var UnExp = (UnaryExpression)GetPropertyLambda.Body;
                if (UnExp.Operand is MemberExpression)
                {
                    Exp = (MemberExpression)UnExp.Operand;
                }
                else
                    throw new ArgumentException();
            }
            else if (GetPropertyLambda.Body is MemberExpression)
            {
                Exp = (MemberExpression)GetPropertyLambda.Body;
            }
            else
            {
                throw new ArgumentException();
            }

            return new StructBuilderPartialTypeProperty<T, PrevT>(this, (PropertyInfo)Exp.Member, this);
        }

        public StructBuilderProperty<PrevT> SavePartialType()
        {
            base.Compile();
            return prevProp;
        }

        public StructBuilderProperty<PrevT> Back()
        {
            return prevProp;
        }
    }
}
