using GrEmit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;

namespace BinarySerializer
{
    public class BinaryStruct
    {
        public Type Type { get; set; }

        public string Scheme { get; set; }

        public List<PropertyData> PropertyList { get; set; }

        public Func<object, byte[]> WriteMethod { get; set; }

        public Func<byte[], object> ReadMethod { get; set; }

        public int InitLen { get; set; } = 32;

        public BinaryStruct(Type type, string scheme, List<PropertyData> propertyList)
        {
            Type = type;
            Scheme = scheme;
            if (string.IsNullOrEmpty(scheme))
                PropertyList = propertyList;
            else
                PropertyList = propertyList.Where(x => x.BinarySchemeAttrList.FirstOrDefault(y => y.SchemeName == scheme) != null).ToList();

            Compile();
        }

        private void Compile()
        {
            CompileWriter();
            CompileReader();
        }

        public BinaryStruct GetSchemeData(string schemeName)
        {
            return new BinaryStruct(Type, schemeName, PropertyList);
        }

        private void CompileWriter()
        {
            DynamicMethod dm = new DynamicMethod(Guid.NewGuid().ToString(), typeof(byte[]), new[] { Type });

            using (var il = new GroboIL(dm))
            {
                var arr = il.DeclareLocal(typeof(byte[]));

                il.ld(typeof(byte));
                il.Ldc_I4(InitLen);
                il.Call(typeof(Array).GetMethod("CreateInstance"));
                il.Stloc(arr);


                il.Ldloc(arr);
                il.Ret();
                var r = dm.Invoke(null, new object[] { new BinarySerializer() });
                if (PropertyList.Count > 0)
                {
                    foreach (var item in PropertyList)
                    {
                        item.BinaryType.GetWriteILCode(item, Type, il);
                    }
                    il.Ret();
                }
                else
                {
                    il.Stloc(il.DeclareLocal(typeof(byte[])));
                    il.Ret();
                }
            }
            WriteMethod = (obj) => (byte[])dm.Invoke(null, new object[] { obj });
        }

        private void CompileReader()
        {
            DynamicMethod dm = new DynamicMethod(Guid.NewGuid().ToString(), this.Type, new[] { typeof(byte[]) });

            using (var il = new GroboIL(dm))
            {
                if (PropertyList.Count > 0)
                {
                    foreach (var item in PropertyList)
                    {
                        item.BinaryType.GetReadILCode(item, Type, il);
                    }
                    il.Ret();
                }
                else
                {
                    il.Stloc(il.DeclareLocal(this.Type));
                    il.Ret();
                }
            }
            ReadMethod = (obj) => dm.Invoke(null, new object[] { obj });
        }

    }
}
