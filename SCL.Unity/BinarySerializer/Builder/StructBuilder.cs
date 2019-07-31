using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace BinarySerializer.Builder
{
    public class StructBuilder
    {
        internal Encoding coding = UTF8Encoding.UTF8;

        internal List<StructBuilderProperty> Propertyes;

        internal TypeStorage CurrentStorage;

        internal List<string> Schemes = new List<string>();

        internal Type CurrentType;

        internal BinaryStruct CurrentStruct;

        internal List<BinaryPreCompileAttribute> PreCompiled = new List<BinaryPreCompileAttribute>();

        protected StructBuilder(TypeStorage storage)
        {
            CurrentStorage = storage;

            Propertyes = new List<StructBuilderProperty>();
        }

        public BinaryStruct Compile()
        {
            CurrentStruct = new BinaryStruct(CurrentType, "", Propertyes.Select(x => (PropertyData)x.property).ToList(), coding, CurrentStorage, true);
            return CurrentStorage.AppendPreCompile(this);
        }
    }

    public class StructBuilder<T> : StructBuilder
    {
        protected StructBuilder(TypeStorage storage) : base(storage)
        {
            CurrentType = typeof(T);
        }

        public void SetEncoding(Encoding coding)
        {
            base.coding = coding;
        }

        public static StructBuilder<T> GetStruct(TypeStorage storage = null)
        {
            if (storage == null)
                storage = TypeStorage.Instance;

            return new StructBuilder<T>(storage);
        }

        public StructBuilderProperty<T> GetProperty(string propertyName)
        {
            var prop = typeof(T).GetProperty(propertyName, BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.Instance |
                BindingFlags.DeclaredOnly);
            if (prop == null)
                throw new NullReferenceException();

            return new StructBuilderProperty<T>(this, prop);
        }

        public StructBuilderProperty<T> GetProperty(Expression<Func<T, object>> GetPropertyLambda)
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

            return new StructBuilderProperty<T>(this, (PropertyInfo)Exp.Member);
        }

        public StructBuilder<T> SetSchemes(params string[] scheme)
        {
            Schemes = scheme.ToList();
            return this;
        }

        public StructBuilder<T> AppendPreCompile(string scheme, int initialLen)
        {
            PreCompiled.Add(new BinaryPreCompileAttribute(scheme, initialLen));

            return this;
        }
    }
}
