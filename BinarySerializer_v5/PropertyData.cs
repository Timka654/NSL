using BinarySerializer.DefaultTypes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace BinarySerializer
{
    public class PropertyData
    {
        public PropertyInfo PropertyInfo { get; set; }

        public MethodInfo Getter { get; set; }

        public MethodInfo Setter { get; set; }

        public IBasicType BinaryType { get; set; }

        public BinaryAttribute BinaryAttr { get; set; }

        public BinarySchemeAttribute[] BinarySchemeAttrList { get; set; }

        public bool IsBaseType { get; set; }

        public BinaryStruct BinaryStruct { get; internal set; }

        public PropertyData ArraySizeProperty { get; internal set; }

        public PropertyData TypeSizeProperty { get; internal set; }

        public int ArraySize => BinaryAttr.ArraySize;

        public int TypeSize => BinaryAttr.TypeSize;

        public PropertyData(PropertyInfo propertyInfo, TypeStorage storage)
        {
            PropertyInfo = propertyInfo;

            BinaryAttr = propertyInfo.GetCustomAttribute<BinaryAttribute>();

            BinarySchemeAttrList = propertyInfo.GetCustomAttributes<BinarySchemeAttribute>().ToArray();

            IsBaseType = typeof(IBasicType).IsAssignableFrom(BinaryAttr.Type);

            //if (PropertyInfo.Name == "CharacterPart")
            //    Debugger.Break();

            if (IsBaseType)
                BinaryType = (IBasicType)Activator.CreateInstance(BinaryAttr.Type);
            //else
            //    BinaryStruct = storage.GetTypeInfo(BinaryAttr.Type,"");


            Getter = propertyInfo.GetMethod;

            Setter = propertyInfo.SetMethod;

        }

        public PropertyData(PropertyData propertyData, string scheme, TypeStorage storage)
        {
            PropertyInfo = propertyData.PropertyInfo;

            BinaryAttr = propertyData.BinaryAttr;

            BinarySchemeAttrList = propertyData.BinarySchemeAttrList;

            IsBaseType = typeof(IBasicType).IsAssignableFrom(BinaryAttr.Type);

            if (IsBaseType)
                BinaryType = (IBasicType)Activator.CreateInstance(BinaryAttr.Type);
            else
                BinaryStruct = storage.GetTypeInfo(BinaryAttr.Type, scheme);


            Getter = propertyData.PropertyInfo.GetMethod;

            Setter = propertyData.PropertyInfo.SetMethod;

        }
    }
}
