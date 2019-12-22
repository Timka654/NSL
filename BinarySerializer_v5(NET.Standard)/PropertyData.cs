using BinarySerializer.DefaultTypes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
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

        private static MethodInfo GetterMethodInfo = typeof(FieldInfo).GetMethod("GetValue");

        public FieldInfo FieldInfo { get; set; }

        public FieldData(FieldInfo fieldInfo, TypeStorage storage, bool builder = false)
        {
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

            Getter = GetterMethodInfo;

            Setter = SetterMethodInfo;

            Name = fieldInfo.Name;

            Type = fieldInfo.FieldType;

            DeclaringType = fieldInfo.DeclaringType;
        }

        public FieldData(FieldData fieldInfo, string scheme, TypeStorage storage) : base(fieldInfo, scheme, storage)
        {
            FieldInfo = fieldInfo.FieldInfo;
        }
    }
}
