using BinarySerializer.DefaultTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace BinarySerializer
{
    public partial class BinarySerializer
    {
        public Encoding TextCoding { get; private set; }

        internal Dictionary<Type, BasicType> TypeInstanceMap { get; set; }

        internal BasicType BoolTypeInstance;

        internal TypeStorage TypeStorage { get;set; }

        internal int Offset;

        public int Length => Offset;

        public Type CurrentSerializedType => ProcessObject.GetType();

        public PropertyData CurrentProperty { get; private set; }

        public string Stack { get; private set; } = "";

        private BinarySerializer(BinarySerializer bs)
        {
            this.Buffer = bs.Buffer;
            this.Offset = bs.Offset;
            this.TextCoding = bs.TextCoding;
        }

        public BinarySerializer(TypeStorage storage)
        {
            TextCoding = Encoding.ASCII;

            TypeInstanceMap = new Dictionary<Type, BasicType>()
            {
                { typeof(BinaryByte), new BinaryByte() },
                { typeof(BinaryBool), new BinaryBool() },
                { typeof(BinaryInt16), new BinaryInt16() },
                { typeof(BinaryUInt16), new BinaryUInt16() },
                { typeof(BinaryInt32), new BinaryInt32() },
                { typeof(BinaryUInt32), new BinaryUInt32() },
                { typeof(BinaryInt64), new BinaryInt64() },
                { typeof(BinaryUInt64), new BinaryUInt64() },
                { typeof(BinaryFloat32), new BinaryFloat32() },
                { typeof(BinaryFloat64), new BinaryFloat64() },
                { typeof(BinaryString), new BinaryString() },
                { typeof(BinaryString16), new BinaryString16() },
                { typeof(BinaryString32), new BinaryString32() },
                { typeof(BinaryDateTime), new BinaryDateTime() },
                { typeof(BinaryTimeSpan), new BinaryTimeSpan() },
                { typeof(BinaryVector2), new BinaryVector2() },
                { typeof(BinaryVector3), new BinaryVector3() },
            };
            
            foreach (var item in TypeInstanceMap)
            {
                item.Value.SetSerializer(this);
            }

            BoolTypeInstance = TypeInstanceMap[typeof(BinaryBool)];

            TypeStorage = storage;
        }


        public BinarySerializer() : this(new TypeStorage())
        {
            
        }
        
        public BinarySerializer(Encoding coding) : this()
        {
            TextCoding = coding;
        }

        public BinarySerializer(Encoding coding, TypeStorage storage) : this(storage)
        {
            TextCoding = coding;
        }

        private void RegisterBasicType(Type type)
        {
            if (type.BaseType != typeof(BasicType))
                throw new Exception("Тип должен наследоваться от BasicType");

            if (TypeInstanceMap.ContainsKey(type))
                return;

            TypeInstanceMap.Add(type, (BasicType)Activator.CreateInstance(type));
        }
        internal BasicType GetTypeInstance(Type type)
        {
            return TypeInstanceMap[type];
        }
    }
}
