using BinarySerializer.DefaultTypes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace BinarySerializer
{
    /// <summary>
    /// Член класса участвующий в структуре серриализации
    /// </summary>
    public class BinaryMemberData
    {
        /// <summary>
        /// Метаданные учасника структуры
        /// </summary>
        public MemberInfo MemberInfo { get; set; }

        /// <summary>
        /// Метод для получения значения учасника структуры
        /// </summary>
        public virtual MethodInfo Getter { get; set; }

        /// <summary>
        /// Метод для установки значения учасника структуры
        /// </summary>
        public MethodInfo Setter { get; set; }

        /// <summary>
        /// Бинарный обработчик типа
        /// </summary>
        public IBasicType BinaryType { get; set; }

        public object[] BinaryTypeGenerics { get; set; }

        /// <summary>
        /// Бинарный аттрибут с данными для серриализации
        /// </summary>
        public BinaryAttribute BinaryAttr { get; set; }

        /// <summary>
        /// Список схем в которых участвует текущий учасник
        /// </summary>
        public List<BinarySchemeAttribute> BinarySchemeAttrList { get; set; }

        /// <summary>
        /// Идентификатор простого типа имеющего бинарный обработчик
        /// </summary>
        public bool IsBaseType { get; set; }

        /// <summary>
        /// Данный о стурктуре которой пренадлежит этот учасник
        /// </summary>
        public BinaryStruct BinaryStruct { get; internal set; }

        /// <summary>
        /// Данные о учаснике который содержит размер массива
        /// </summary>
        public BinaryMemberData ArraySizeProperty { get; internal set; }

        /// <summary>
        /// Данные о учаснике который содержит размер для текущего учасника
        /// </summary>
        public BinaryMemberData TypeSizeProperty { get; internal set; }

        /// <summary>
        /// Статичный размер массива для текущего учасника
        /// </summary>
        public int ArraySize => BinaryAttr.ArraySize;

        /// <summary>
        /// Статичный размер типа для текущего учасника
        /// </summary>
        public int TypeSize => BinaryAttr.TypeSize;

        public Func<int> ArraySizeGetter = () => { return 0; };

        public Func<int> TypeSizeGetter = () => { return 0; };

        /// <summary>
        /// Название учасника
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// Тип члена класса
        /// </summary>
        public Type Type { get; protected set; }

        /// <summary>
        /// Gets the class that declares this member.
        /// </summary>
        public Type DeclaringType { get; protected set; }

        public BinaryMemberData()
        {

        }

        public BinaryMemberData(BinaryMemberData binaryMemberData, string scheme, TypeStorage storage)
        {
            BinaryAttr = binaryMemberData.BinaryAttr;

            BinarySchemeAttrList = binaryMemberData.BinarySchemeAttrList;

            IsBaseType = typeof(IBasicType).IsAssignableFrom(BinaryAttr.Type);

            if (IsBaseType)
                BinaryType = (IBasicType)Activator.CreateInstance(BinaryAttr.Type);
            else
            {
                BinaryStruct = storage.GetTypeInfo(BinaryAttr.Type, scheme);

                BinaryTypeGenerics = BinaryAttr.Type.GetGenericArguments().Select(x =>
                {
                    if (typeof(IBasicType).IsAssignableFrom(x))
                        return (object)Activator.CreateInstance(x);
                    else
                        return (object)storage.GetTypeInfo(x, scheme);
                }).ToArray();
            }
            Getter = binaryMemberData.Getter;

            Setter = binaryMemberData.Setter;

            Name = binaryMemberData.Name;

            Type = binaryMemberData.Type;

            MemberInfo = binaryMemberData.MemberInfo;

        }
    }

    /// <summary>
    /// Свойство класса участвующее в стурктуре серриализации
    /// </summary>
    public class PropertyData : BinaryMemberData
    {
        /// <summary>
        /// Метаданные свойства
        /// </summary>
        public PropertyInfo PropertyInfo { get; set; }

        public PropertyData(PropertyInfo propertyInfo, TypeStorage storage, bool builder = false)
        {
            PropertyInfo = propertyInfo;
            MemberInfo = propertyInfo;

            BinaryAttr = propertyInfo.GetCustomAttribute<BinaryAttribute>();

            BinarySchemeAttrList = propertyInfo.GetCustomAttributes<BinarySchemeAttribute>().ToList();

            if (builder == false)
            {
                IsBaseType = typeof(IBasicType).IsAssignableFrom(BinaryAttr.Type);

                //if (PropertyInfo.Name == "CharacterPart")
                //    Debugger.Break();

                if (IsBaseType)
                    BinaryType = (IBasicType)Activator.CreateInstance(BinaryAttr.Type);
                //else
                //    BinaryStruct = storage.GetTypeInfo(BinaryAttr.Type,"");

            }
            Getter = propertyInfo.GetGetMethod();

            Setter = propertyInfo.GetSetMethod();

            Name = propertyInfo.Name;

            Type = propertyInfo.PropertyType;

            DeclaringType = propertyInfo.DeclaringType;

        }

        public PropertyData(PropertyData propertyData, string scheme, TypeStorage storage) : base(propertyData, scheme, storage)
        {
            PropertyInfo = propertyData.PropertyInfo;
        }
    }

    /// <summary>
    /// Переменная класса участвующая в стурктуре серриализации
    /// </summary>
    public class FieldData : BinaryMemberData
    {
        //private static MethodInfo SetterMethodInfo = typeof(FieldInfo).GetMethod("SetValue");

        /// <summary>
        /// Метаданные переменной
        /// </summary>
        public FieldInfo FieldInfo { get; set; }

        public FieldData(FieldInfo fieldInfo, TypeStorage storage, bool builder = false)
        {
            //new FieldInfo().GetValue()
            FieldInfo = fieldInfo;
            MemberInfo = fieldInfo;

            BinaryAttr = fieldInfo.GetCustomAttribute<BinaryAttribute>();

            BinarySchemeAttrList = fieldInfo.GetCustomAttributes<BinarySchemeAttribute>().ToList();

            if (builder == false)
            {
                IsBaseType = typeof(IBasicType).IsAssignableFrom(BinaryAttr.Type);

                //if (PropertyInfo.Name == "CharacterPart")
                //    Debugger.Break();

                if (IsBaseType)
                    BinaryType = (IBasicType)Activator.CreateInstance(BinaryAttr.Type);
                //else
                //    BinaryStruct = storage.GetTypeInfo(BinaryAttr.Type,"");

            }

            Getter = CreateGetter(fieldInfo);

            Setter = CreateSetter(fieldInfo);

            Name = fieldInfo.Name;

            Type = fieldInfo.FieldType;

            DeclaringType = fieldInfo.DeclaringType;
        }

        public FieldData(FieldData fieldInfo, string scheme, TypeStorage storage) : base(fieldInfo, scheme, storage)
        {
            FieldInfo = fieldInfo.FieldInfo;
        }



        static MethodInfo CreateGetter(FieldInfo field)
        {
            string methodName = field.ReflectedType.FullName + ".get_" + field.Name;
            DynamicMethod setterMethod = new DynamicMethod(methodName, field.FieldType, new Type[1] { field.DeclaringType }, true);
            ILGenerator gen = setterMethod.GetILGenerator();
            if (field.IsStatic)
            {
                gen.Emit(OpCodes.Ldsfld, field);
            }
            else
            {
                gen.Emit(OpCodes.Ldarg_0);
                gen.Emit(OpCodes.Ldfld, field);
            }
            gen.Emit(OpCodes.Ret);

            return setterMethod.CreateDelegate(typeof(Func<,>).MakeGenericType(field.DeclaringType, field.FieldType)).GetMethodInfo();
        }

        static MethodInfo CreateSetter(FieldInfo field)
        {
            string methodName = field.ReflectedType.FullName + ".set_" + field.Name;
            DynamicMethod setterMethod = new DynamicMethod(methodName, null, new Type[2] { field.DeclaringType, field.FieldType }, true);
            ILGenerator gen = setterMethod.GetILGenerator();
            if (field.IsStatic)
            {
                gen.Emit(OpCodes.Ldarg_1);
                gen.Emit(OpCodes.Stsfld, field);
            }
            else
            {
                gen.Emit(OpCodes.Ldarg_0);
                gen.Emit(OpCodes.Ldarg_1);
                gen.Emit(OpCodes.Stfld, field);
            }
            gen.Emit(OpCodes.Ret);

            return setterMethod.CreateDelegate(typeof(Action<,>).MakeGenericType(field.DeclaringType, field.FieldType)).GetMethodInfo();
        }
    }
}
