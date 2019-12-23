using BinarySerializer.DefaultTypes;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace BinarySerializer.Builder
{
    public class StructBuilderMember : BinaryMemberData
    {
        public IBuilderMemberData data { get; set; }
    }

    public class StructBuilderMember<T> : StructBuilderMember
    {
        protected StructBuilder<T> currentBuilder { get; set; }

        public StructBuilderMember(StructBuilder<T> builder, MemberInfo prop)
        {
            currentBuilder = builder;

            if (prop is PropertyInfo p)
                data = new BuilderPropertyData(p, builder.CurrentStorage);
            else if (prop is FieldInfo f)
                data = new BuilderFieldData(f, builder.CurrentStorage);

            data.Member.BinarySchemeAttrList = builder.Schemes.Select(x => new BinarySchemeAttribute(x)).ToList();
        }

        public StructBuilderMember<T> SetBinaryType<Q>()
            where Q : IBasicType
        {
            data.Member.BinaryAttr = new BinaryAttribute(typeof(Q));

            return this;
        }

        public StructBuilderPartialType<Q, T> SetPartialType<Q>()
        {
            data.Member.BinaryAttr = new BinaryAttribute(typeof(Q));

            var temp = StructBuilderPartialType<Q, T>.GetStruct(this, currentBuilder.CurrentStorage);

            data.CurrentBuilder = temp;
            return temp;
        }

        public StructBuilderMember<T> SetTypeSize(int size)
        {
            data.Member.BinaryAttr.TypeSize = size;
            return this;
        }

        public StructBuilderMember<T> SetArraySize(int size)
        {
            data.Member.BinaryAttr.ArraySize = size;
            return this;
        }

        public StructBuilderMember<T> SetSchemes(params string[] scheme)
        {
            data.Member.BinarySchemeAttrList = scheme.Select(x => new BinarySchemeAttribute(x)).ToList();

            return this;
        }

        public StructBuilderMember<T> AppendScheme(string scheme)
        {
            data.Member.BinarySchemeAttrList.Add(new BinarySchemeAttribute(scheme));

            return this;
        }

        public StructBuilderMember<T> SetTypeSize(Expression<Func<T, object>> GetPropertyLambda)
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

            var p = (PropertyInfo)Exp.Member;

            data.Member.TypeSizeProperty = currentBuilder.Propertyes.Find(x => x.data.Member.Name == p.Name).data.Member;

            return this;
        }

        public StructBuilderMember<T> SetArraySize(Expression<Func<T, object>> GetPropertyLambda)
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

            var p = (PropertyInfo)Exp.Member;

            data.Member.ArraySizeProperty = currentBuilder.Propertyes.Find(x => x.data.Member.Name == p.Name).data.Member;

            return this;
        }

        public virtual StructBuilder<T> SaveProperty()
        {
            currentBuilder.Propertyes.Add(this);
            return currentBuilder;
        }
    }
}
