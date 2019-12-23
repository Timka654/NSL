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
    public class BinaryMemberData
    {
        public MemberInfo MemberInfo { get; set; }

        public virtual MethodInfo Getter { get; set; }

        public MethodInfo Setter { get; set; }

        public IBasicType BinaryType { get; set; }

        public BinaryAttribute BinaryAttr { get; set; }

        public List<BinarySchemeAttribute> BinarySchemeAttrList { get; set; }

        public bool IsBaseType { get; set; }

        public BinaryStruct BinaryStruct { get; internal set; }

        public BinaryMemberData ArraySizeProperty { get; internal set; }

        public BinaryMemberData TypeSizeProperty { get; internal set; }

        public int ArraySize => BinaryAttr.ArraySize;

        public int TypeSize => BinaryAttr.TypeSize;

        public string Name { get; protected set; }

        public Type Type { get; protected set; }

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
                BinaryStruct = storage.GetTypeInfo(BinaryAttr.Type, scheme);


            Getter = binaryMemberData.Getter;

            Setter = binaryMemberData.Setter;

            Name = binaryMemberData.Name;

            Type = binaryMemberData.Type;
            MemberInfo = binaryMemberData.MemberInfo;

        }
    }

    public class PropertyData : BinaryMemberData
    {
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
            Getter = propertyInfo.GetMethod;

            Setter = propertyInfo.SetMethod;

            Name = propertyInfo.Name;

            Type = propertyInfo.PropertyType;

            DeclaringType = propertyInfo.DeclaringType;

        }

        public PropertyData(PropertyData propertyData, string scheme, TypeStorage storage) : base(propertyData,scheme,storage)
        {
            PropertyInfo = propertyData.PropertyInfo;
        }
        }

    public class FieldData : BinaryMemberData
    {
        private static MethodInfo SetterMethodInfo = typeof(FieldInfo).GetMethod("SetValue");

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


            Getter = MakeGetter(fieldInfo);



            Setter = MakeSetter(fieldInfo);

            Name = fieldInfo.Name;

            Type = fieldInfo.FieldType;

            DeclaringType = fieldInfo.DeclaringType;
        }

        public FieldData(FieldData fieldInfo, string scheme, TypeStorage storage) : base(fieldInfo, scheme, storage)
        {
            FieldInfo = fieldInfo.FieldInfo;
        }

        static MethodInfo MakeSetter(FieldInfo field)
        {
            DynamicMethod m = new DynamicMethod(
                "setter", typeof(void), new Type[] { field.DeclaringType, field.FieldType }, typeof(FieldData));
            ILGenerator cg = m.GetILGenerator();

            // arg0.<field> = arg1
            cg.Emit(OpCodes.Ldarg_0);
            cg.Emit(OpCodes.Ldarg_1);
            cg.Emit(OpCodes.Stfld, field);
            cg.Emit(OpCodes.Ret);

            var a = typeof(Action<,>);

            var tmp = a.MakeGenericType(field.DeclaringType, field.FieldType);

            return m.CreateDelegate(tmp).GetMethodInfo();
        }

        static MethodInfo MakeGetter(FieldInfo field)
        {
            DynamicMethod m = new DynamicMethod(
                "setter", field.FieldType, new Type[] { field.DeclaringType }, typeof(FieldData));
            ILGenerator cg = m.GetILGenerator();

            // arg0.<field> = arg1
            cg.Emit(OpCodes.Ldarg_0);
            cg.Emit(OpCodes.Ldloc, field);
            cg.Emit(OpCodes.Ret);

            var a = typeof(Func<,>);

            var tmp = a.MakeGenericType(field.DeclaringType, field.FieldType);

            return m.CreateDelegate(tmp).GetMethodInfo();
        }
    }
}
