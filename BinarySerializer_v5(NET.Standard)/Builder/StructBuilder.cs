using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace BinarySerializer.Builder
{
    /// <summary>
    /// Генератор описания стурктуры для типа
    /// </summary>
    public class StructBuilder
    {
        /// <summary>
        /// Кодировка используемая при компиляции
        /// </summary>
        internal Encoding coding = UTF8Encoding.UTF8;

        /// <summary>
        /// Список учасников стурктуры
        /// </summary>
        internal List<StructBuilderMember> Propertyes;

        /// <summary>
        /// Хранилище которое будет использоваться для связи сложных типов и в которое будет компилироваться эта структура
        /// </summary>
        internal TypeStorage CurrentStorage;

        /// <summary>
        /// Текущие схемы используемые для установки для обьявляемых членов
        /// </summary>
        internal List<string> Schemes = new List<string>();

        /// <summary>
        /// Тип класса
        /// </summary>
        internal Type CurrentType;

        /// <summary>
        /// Скомпилированная структура связана с текущим билдером
        /// </summary>
        internal BinaryStruct CurrentStruct;

        /// <summary>
        /// Список данных для предкомпиляции схем 
        /// </summary>
        internal List<BinaryPreCompileAttribute> PreCompiled = new List<BinaryPreCompileAttribute>();

        protected StructBuilder(TypeStorage storage)
        {
            CurrentStorage = storage;
            coding = storage.Coding;

            Propertyes = new List<StructBuilderMember>();
        }

        public BinaryStruct Compile()
        {
            CurrentStruct = new BinaryStruct(CurrentType, "", Propertyes.Select(x => (BinaryMemberData)x.data).ToList(), coding, CurrentStorage, true);

            foreach (var item in PreCompiled)
            {
                CurrentStorage.GetTypeInfo(CurrentType, item.Scheme, item.InitialSize);
            }

            return CurrentStorage.AppendPreCompile(this);
        }
    }

    /// <summary>
    /// Генератор описания стурктуры для типа
    /// </summary>
    public class StructBuilder<T> : StructBuilder
    {
        protected StructBuilder(TypeStorage storage) : base(storage)
        {
            CurrentType = typeof(T);
        }

        /// <summary>
        /// Установка кодировки
        /// </summary>
        /// <param name="coding"></param>
        public void SetEncoding(Encoding coding)
        {
            base.coding = coding;
        }

        /// <summary>
        /// Получить новый StructBuilder с указанных привязанным хранилищем
        /// </summary>
        /// <param name="storage">Хранилище, по умолчанию TypeStorage.Instance</param>
        /// <returns></returns>
        public static StructBuilder<T> GetStruct(TypeStorage storage = null)
        {
            if (storage == null)
                storage = TypeStorage.Instance;

            return new StructBuilder<T>(storage);
        }

        /// <summary>
        /// Получить свойство класса для дальнейшего описания
        /// </summary>
        /// <param name="propertyName">Название свойства</param>
        /// <returns></returns>
        public StructBuilderMember<T> GetProperty(string propertyName)
        {
            var prop = typeof(T).GetProperty(propertyName, BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.Instance |
                BindingFlags.DeclaredOnly);
            if (prop == null)
                throw new NullReferenceException();

            return new StructBuilderMember<T>(this, prop);
        }

        /// <summary>
        /// Получить свойство класса для дальнейшего описания
        /// </summary>
        /// <param name="GetPropertyLambda">лямбда выражение результатом которого должно быть свойство класса</param>
        /// <returns></returns>
        public StructBuilderMember<T> GetProperty(Expression<Func<T, object>> GetPropertyLambda)
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

            return new StructBuilderMember<T>(this, (PropertyInfo)Exp.Member);
        }

        /// <summary>
        /// Получить переменную класса для дальнейшего описания
        /// </summary>
        /// <param name="fieldName">Название переменной</param>
        /// <returns></returns>
        public StructBuilderMember<T> GetField(string fieldName)
        {
            var prop = typeof(T).GetField(fieldName, BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.Instance |
                BindingFlags.DeclaredOnly);
            if (prop == null)
                throw new NullReferenceException();

            return new StructBuilderMember<T>(this, prop);
        }

        /// <summary>
        /// Получить переменную класса для дальнейшего описания
        /// </summary>
        /// <param name="GetFieldLambda">лямбда выражение результатом которого должна быть переменная класса</param>
        /// <returns></returns>
        public StructBuilderMember<T> GetField(Expression<Func<T, object>> GetFieldLambda)
        {
            MemberExpression Exp = null;

            //this line is necessary, because sometimes the expression comes in as Convert(originalexpression)
            if (GetFieldLambda.Body is UnaryExpression)
            {
                var UnExp = (UnaryExpression)GetFieldLambda.Body;
                if (UnExp.Operand is MemberExpression)
                {
                    Exp = (MemberExpression)UnExp.Operand;
                }
                else
                    throw new ArgumentException();
            }
            else if (GetFieldLambda.Body is MemberExpression)
            {
                Exp = (MemberExpression)GetFieldLambda.Body;
            }
            else
            {
                throw new ArgumentException();
            }

            return new StructBuilderMember<T>(this, (FieldInfo)Exp.Member);
        }

        /// <summary>
        /// Установка схем с которыми будут связаны дальнейшие свойства
        /// </summary>
        /// <param name="scheme"></param>
        /// <returns></returns>
        public StructBuilder<T> SetSchemes(params string[] scheme)
        {
            Schemes = scheme.ToList();
            return this;
        }

        /// <summary>
        /// Добавить данные предкомпиляции для схемы
        /// </summary>
        /// <param name="scheme">название схемы</param>
        /// <param name="initialLen">размер буффера при инициаизации</param>
        /// <returns></returns>
        public StructBuilder<T> AppendPreCompile(string scheme, int initialLen)
        {
            PreCompiled.Add(new BinaryPreCompileAttribute(scheme, initialLen));

            return this;
        }
    }
}
