using BinarySerializer.DefaultTypes;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

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
}
