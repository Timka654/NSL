//using BinarySerializer;
//using BinarySerializer.Attributes;
//using NSL.SocketCore.Extensions.BinarySerializer.DefaultTypes;
//using NSL.SocketCore.Extensions.BinarySerializer.IEnumerableTypes;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Numerics;
//using System.Reflection;
//using System.Text;
//using System.Threading.Tasks;

//#if (Unity)
//using UnityEngine;
//#endif

//namespace NSL.SocketCore.Extensions.BinarySerializer
//{
//    public class BinaryNetworkStorage : BinaryStorage
//    {
//        private class tempProp {
//            public PropertyInfo property;
//            public BinaryAttribute attribute;
//            public IEnumerable<BinarySchemeAttribute> schemes;
//        }

//        public BinaryNetworkStorage()
//        {
//            Store.Add(typeof(List<>).Name, new BinaryList());
//            Store.Add(typeof(Array).Name, new BinaryArray());
//            Store.Add(typeof(Dictionary<,>).Name, new BinaryDictionary());
//            Store.Add(typeof(Nullable<>).Name, new BinaryNullable());
//            Store.Add(typeof(byte).Name, new BinaryByte());
//            Store.Add(typeof(sbyte).Name, new BinarySByte());
//            Store.Add(typeof(bool).Name, new BinaryBool());
//            Store.Add(typeof(short).Name, new BinaryInt16());
//            Store.Add(typeof(ushort).Name, new BinaryUInt16());
//            Store.Add(typeof(int).Name, new BinaryInt32());
//            Store.Add(typeof(uint).Name, new BinaryUInt32());
//            Store.Add(typeof(long).Name, new BinaryInt64());
//            Store.Add(typeof(ulong).Name, new BinaryUInt64());
//            Store.Add(typeof(float).Name, new BinaryFloat32());
//            Store.Add(typeof(double).Name, new BinaryFloat64());
//            Store.Add(typeof(string).Name, new BinaryString());
//            Store.Add(typeof(DateTime).Name, new BinaryDateTime());
//            Store.Add(typeof(TimeSpan).Name, new BinaryTimeSpan());
//            Store.Add(typeof(Vector2).Name, new BinaryVector2());
//            Store.Add(typeof(Vector3).Name, new BinaryVector3());


//            foreach (var item in Store)
//            {
//                item.Value.Storage = this;
//            }
//        }

//        public void BuildTypes()
//        {
//            BuildTypes(Assembly.GetCallingAssembly());
//        }

//        public void BuildTypes(Assembly assembly)
//        {
//            var types = assembly.GetTypes().Select(x => new { attr = x.GetCustomAttributes<BinaryNetworkTypeAttribute>().FirstOrDefault(), type = x }).Where(x => x.attr != null).ToArray();

//            foreach (var item in types)
//            {
//                BuildType(item.type);
//            }
//        }

//        private IEnumerable<tempProp> getProps(Type type)
//        {
//            return type.GetProperties(BindingFlags.NonPublic | BindingFlags.Public
//           | BindingFlags.Instance).Select(x => new tempProp { property = x, attribute = x.GetCustomAttribute<BinaryAttribute>(), schemes = x.GetCustomAttributes<BinarySchemeAttribute>() });
//        }

//        public void BuildType(Type type, string schemeS = "")
//        {
//            if (this.ContainsType(type))
//            {
//                if(!string.IsNullOrWhiteSpace(schemeS) && !this.ContainsType(type,schemeS))
//                    BuildType(type, schemeS, null);
//                return;
//            }
//            var props = getProps(type).Where(x => x.attribute != null).ToArray();

//            var schemes = props.SelectMany(x => x.schemes).Select(x => x.Scheme).Distinct();

//            BinarySchemeStorage bss = new BinarySchemeStorage();
//            bss.Type = type;

//            RegisterType(type, bss);
//            foreach (var scheme in schemes)
//            {
//                if (bss.ExistsScheme(scheme))
//                    continue;
//                BuildType(type, scheme,bss);
//            }
//        }

//        public void BuildType(Type type, string scheme, BinarySchemeStorage bss = null)
//        {
//            var br = type.Name;
//            if (bss == null)
//            {
//                bss = GetBinarySchemeStorage(type);

//                if (bss == null)
//                {
//                    bss = new BinarySchemeStorage();
//                    bss.Type = type;

//                    RegisterType(type, bss);
//                }
//            }
//            var cprops = getProps(type).Where(x => x.attribute != null).Where(x => x.schemes.Any(y => y.Scheme == scheme)).OrderBy(x=>x.property.Name).ToList();

//            List<BinaryWriteAction> writeActions = new List<BinaryWriteAction>();
//            List<BinaryReadAction> readActions = new List<BinaryReadAction>();

//            Type currType = null;


//            foreach (var p in cprops)
//            {
//                currType = p.property.PropertyType;

//                if (currType.IsEnum)
//                    currType = currType.GetEnumUnderlyingType();
//                else if (currType.IsArray)
//                    currType = typeof(Array);
//                //p.property.GetGetMethod()

//                if (!base.ContainsType(currType, scheme))
//                    BuildType(currType, scheme);

//                BinaryReadAction ra = null;
//                BinaryReadAction ra2 = base.GetReadAction(currType, p.property, scheme);

//                BinaryWriteAction wa = null;
//                BinaryWriteAction wa2 = base.GetWriteAction(currType, p.property, scheme);

//                var setter = Extensions.CreateSetPropertyFuncDynamic(p.property);

//                if (Nullable.GetUnderlyingType(p.property.PropertyType) != null || (p.property.PropertyType.IsClass && !p.property.PropertyType.IsPrimitive))
//                {
//                    var getter = Extensions.CreateGetPropertyFuncDynamic(p.property);

//                    var instanceAction = Extensions.GetInstanceFuncDynamic(p.property);

//                    wa = (ms, s, obj) =>
//                    {
//                        var d = getter(obj);
//                        if (d == null)
//                        {
//                            ((SocketCore.Utils.Buffer.OutputPacketBuffer)ms).WriteBool(false);
//                            return;
//                        }
//                            ((SocketCore.Utils.Buffer.OutputPacketBuffer)ms).WriteBool(true);
//                        wa2(ms, s, d);
//                    };

//                    ra = (ms, s, obj) =>
//                    {
//                        if (((SocketCore.Utils.Buffer.InputPacketBuffer)ms).ReadBool())
//                        {
//                            setter(obj, ra2(ms, s, obj));
//                        }
//                        return null;
//                    };
//                }
//                else if (p.property.PropertyType.IsEnum)
//                        {
//                            wa = wa2;
//                            ra = (ms, s, obj) => { setter(obj, Enum.ToObject(p.property.PropertyType,ra2(ms, s, obj))); return null; };
//                        }
//                else
//                {
//                    wa = wa2;
//                    ra = (ms, s, obj) => { setter(obj, ra2(ms, s, obj)); return null; };
//                }
//                writeActions.Add(wa);

//                readActions.Add(ra);
//            }


//            var writeActionCombine = (BinaryWriteAction)BinaryWriteAction.Combine(writeActions.ToArray());
//            var readActionCombine = (BinaryReadAction)BinaryReadAction.Combine(readActions.ToArray());

//            BinaryWriteAction writeAction = (ms, s, obj) =>
//            {

//                if (obj == null)
//                {
//                    ((SocketCore.Utils.Buffer.OutputPacketBuffer)ms).WriteBool(false);
//                    return;
//                }
//                    ((SocketCore.Utils.Buffer.OutputPacketBuffer)ms).WriteBool(true);
//                writeActionCombine(ms, s, obj);
//            };

//            var typeActivator = Extensions.GetInstanceByType(type);

//            BinaryReadAction readAction = (ms, s, obj) =>
//            {
//                if (((SocketCore.Utils.Buffer.InputPacketBuffer)ms).ReadBool())
//                {
//                    obj = typeActivator();
//                    readActionCombine(ms, s, obj);
//                    return obj;
//                }
//                return null;
//            };


//            bss.RegisterScheme(scheme, new BinaryTypeStorage()
//            {
//                WriteAction = writeAction,
//                ReadAction = readAction
//            });
//        }


//        public override BinaryWriteAction GetWriteAction(Type type, PropertyInfo property, string scheme = "default")
//        {
//            if (type.IsEnum)
//                return base.GetWriteAction(type.GetEnumUnderlyingType(), property, scheme);
//            return base.GetWriteAction(type, property, scheme);
//        }

//    }
//}
