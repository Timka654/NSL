using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using GrEmit;

namespace BinarySerializer.DefaultTypes
{
    public partial class BinaryConcurrentDictionary16<TKey,TValue> : IBasicType
    {
#if !NOT_UNITY
        Func<object> keyReader;
        Func<object> valueReader;

        public object GetReadILCode(BinaryMemberData prop, BinaryStruct currentStruct, IL2CPPMemoryStream buffer, object currentObject)
        {
            //var len = buffer.ReadInt16();

            //var result = new ConcurrentDictionary<TKey, TValue>();

            //if(keyReader == null || valueReader == null)
            //{
            //    var keyType = prop.BinaryAttr.Type.GetGenericArguments()[0];
            //if (typeof(IBasicType).IsAssignableFrom(keyType))
            //    {
            //        IBasicType t = (IBasicType)Activator.CreateInstance<TKey>();
            //        keyReader = new Func<object>(() => { return t.GetReadILCode(prop, currentStruct, buffer, currentObject); });
            //}
            //else
            //{


            //    var constr = BinaryStruct.GetConstructor(keyType, null);
            //    if (constr == null)
            //        throw new Exception($"Type {keyType} not have constructor with not parameters");

            //        keyReader = () =>
            //            BinaryStruct.CompileReader(currentStruct,));

            //}

            //    for (int i = 0; i < len; i++)
            //    {

            //    }
            return null;
        }

        public void GetWriteILCode(BinaryMemberData prop, BinaryStruct currentStruct, IL2CPPMemoryStream buffer, object currentObject)
        {
           
        }
#endif
    }
}
