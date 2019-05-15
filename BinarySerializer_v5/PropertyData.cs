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

        public Func<object, byte[]> WriteMethod { get; set; }

        public BinaryAttribute BinaryAttr { get; set; }

        public BinarySchemeAttribute[] BinarySchemeAttrList { get; set; }

        public PropertyData(PropertyInfo propertyInfo, Dictionary<Type, IBasicType> instanceMap)
        {
            PropertyInfo = propertyInfo;

            BinaryAttr = propertyInfo.GetCustomAttribute<BinaryAttribute>();

            BinarySchemeAttrList = propertyInfo.GetCustomAttributes<BinarySchemeAttribute>().ToArray();

            BinaryType = instanceMap[BinaryAttr.Type];

            Getter = propertyInfo.GetMethod;

            Setter = propertyInfo.SetMethod;
        }
    }
}
