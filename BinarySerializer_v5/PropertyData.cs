using BinarySerializer.DefaultTypes;
using System;
using System.Collections.Generic;
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

        public PropertyData(PropertyInfo propertyInfo)
        {
            PropertyInfo = propertyInfo;

            BinaryAttr = propertyInfo.GetCustomAttribute<BinaryAttribute>();

            BinarySchemeAttrList = propertyInfo.GetCustomAttributes<BinarySchemeAttribute>().ToArray();

            IsBaseType = typeof(IBasicType).IsAssignableFrom(BinaryAttr.Type);

            if(IsBaseType)
                BinaryType = (IBasicType)Activator.CreateInstance(BinaryAttr.Type);

            Getter = propertyInfo.GetMethod;

            Setter = propertyInfo.SetMethod;

        }
    }
}
