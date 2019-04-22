using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BinarySerializer.DefaultTypes;

namespace BinarySerializer
{
    public class TypeHelper
    {
        public static void FillDynamicProperties(object obj)
        {
            var type = obj.GetType();

            var fields = type.GetProperties().ToList().Where(x =>
                x.GetCustomAttributes(true).FirstOrDefault(y => y.GetType() == typeof(BinaryAttribute)) != null).ToDictionary(x=>x.Name,x=>x);

            foreach (var fieldInfo in fields.Values)
            {
                var attr = (BinaryAttribute)fieldInfo.GetCustomAttributes(true).First(y => y.GetType() == typeof(BinaryAttribute));
                if (attr.Type.BaseType != typeof(BasicType))
                {
                    FillDynamicProperties(fieldInfo.GetValue(obj));
                    continue;
                }
                

                if (attr.ArraySize > 0)
                {
                    if (typeof(ICollection).IsAssignableFrom(fieldInfo.DeclaringType))
                        throw new Exception($"{fieldInfo.Name} must release ICollection interface");
                }

                if (attr.ArraySizeName != null)
                {
                    if (typeof(ICollection).IsAssignableFrom(fieldInfo.DeclaringType))
                        throw new Exception($"{fieldInfo.Name} must release ICollection interface");
                    var len = ((ICollection)fieldInfo.GetValue(obj)).Count;

                    fields[attr.ArraySizeName].SetValue(obj, len);

                }

                if (attr.TypeSizeName != null)
                {
                    //var len =(fieldInfo.GetValue(obj)).ToString() .Length;
                    int len = 0;
                    object v = fieldInfo.GetValue(obj);
                    if (attr.ArraySize > 0 || attr.ArraySizeName != null)
                    {
                        for (int i = 0; i < ((ICollection) v).Count; i++)
                        {
                            int r = (int)attr.Type.GetMethod("GetSize", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public)
                                .Invoke(null, new[] {((ICollection<object>)v).ElementAt(i)});

                            if(r > len)
                                len = r;
                        }
                    }
                    else
                    {
                        attr.Type.GetMethod("GetSize",
                                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public)
                            .Invoke(null, new[] {fieldInfo.GetValue(obj)});
                    }

                fields[attr.TypeSizeName].SetValue(obj, Convert.ChangeType(len,fields[attr.TypeSizeName].PropertyType));
                }
            }
        }
    }
}
