using BinarySerializer.DefaultTypes;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace BinarySerializer.Builder
{
    public class StructBuilderPartialTypeProperty<T, PrevT> : StructBuilderProperty<T>
    {
        StructBuilderPartialType<T, PrevT> prevType;
        public StructBuilderPartialTypeProperty(StructBuilder<T> builder, PropertyInfo prop, StructBuilderPartialType<T, PrevT> pType) : base(builder, prop)
        {
            prevType = pType;
        }

        public new StructBuilderPartialType<T, PrevT> SaveProperty()
        {
            currentBuilder.Propertyes.Add(this);
            return prevType;
        }
        public new StructBuilderPartialTypeProperty<T, PrevT> SetBinaryType<Q>()
            where Q : IBasicType
        {
            return (StructBuilderPartialTypeProperty<T, PrevT>)base.SetBinaryType<Q>();
        }

        public new StructBuilderPartialTypeProperty<T, PrevT> SetTypeSize(int size)
        {
            return (StructBuilderPartialTypeProperty<T, PrevT>)base.SetTypeSize(size);
        }

        public new StructBuilderPartialTypeProperty<T, PrevT> SetArraySize(int size)
        {
            return (StructBuilderPartialTypeProperty<T, PrevT>)base.SetArraySize(size);
        }

        public new StructBuilderPartialTypeProperty<T, PrevT> SetSchemes(params string[] scheme)
        {
            return (StructBuilderPartialTypeProperty<T, PrevT>)base.SetSchemes(scheme);
        }

        public new StructBuilderPartialTypeProperty<T, PrevT> AppendScheme(string scheme)
        {
            return (StructBuilderPartialTypeProperty<T, PrevT>)base.AppendScheme(scheme);
        }

        public new StructBuilderPartialTypeProperty<T, PrevT> SetTypeSize(Expression<Func<T, object>> GetPropertyLambda)
        {
            return (StructBuilderPartialTypeProperty<T, PrevT>)base.SetTypeSize(GetPropertyLambda);
        }

        public new StructBuilderPartialTypeProperty<T, PrevT> SetArraySize(Expression<Func<T, object>> GetPropertyLambda)
        {
            return (StructBuilderPartialTypeProperty<T, PrevT>)base.SetArraySize(GetPropertyLambda);
        }

    }

    public class StructBuilderProperty<T> : StructBuilderProperty
    {
        protected StructBuilder<T> currentBuilder { get; set; }

        public StructBuilderProperty(StructBuilder<T> builder, PropertyInfo prop)
        {
            currentBuilder = builder;

            property = new BuilderPropertyData(prop, builder.CurrentStorage);

            property.BinarySchemeAttrList = builder.Schemes.Select(x => new BinarySchemeAttribute(x)).ToList();
        }

        public StructBuilderProperty<T> SetBinaryType<Q>()
            where Q : IBasicType
        {
            property.BinaryAttr = new BinaryAttribute(typeof(Q));

            return this;
        }

        public StructBuilderPartialType<Q, T> SetPartialType<Q>()
        {
            property.BinaryAttr = new BinaryAttribute(typeof(Q));

            var temp = StructBuilderPartialType<Q, T>.GetStruct(this, currentBuilder.CurrentStorage);

              property.CurrentBuilder = temp;
            return temp;
        }

        public StructBuilderProperty<T> SetTypeSize(int size)
        {
            property.BinaryAttr.TypeSize = size;
            return this;
        }

        public StructBuilderProperty<T> SetArraySize(int size)
        {
            property.BinaryAttr.ArraySize = size;
            return this;
        }

        public StructBuilderProperty<T> SetSchemes(params string[] scheme)
        {
            property.BinarySchemeAttrList = scheme.Select(x => new BinarySchemeAttribute(x)).ToList();

            return this;
        }

        public StructBuilderProperty<T> AppendScheme(string scheme)
        {
            property.BinarySchemeAttrList.Add(new BinarySchemeAttribute(scheme));

            return this;
        }

        public StructBuilderProperty<T> SetTypeSize(Expression<Func<T, object>> GetPropertyLambda)
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

            property.TypeSizeProperty = currentBuilder.Propertyes.Find(x => x.property.PropertyInfo.Name == p.Name).property;

            return this;
        }

        public StructBuilderProperty<T> SetArraySize(Expression<Func<T, object>> GetPropertyLambda)
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

            property.ArraySizeProperty = currentBuilder.Propertyes.Find(x => x.property.PropertyInfo.Name == p.Name).property;

            return this;
        }

        public virtual StructBuilder<T> SaveProperty()
        {
            currentBuilder.Propertyes.Add(this);
            return currentBuilder;
        }
    }

    public class StructBuilderProperty
    {
        internal BuilderPropertyData property { get; set; }
    }
}
