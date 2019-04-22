using BinarySerializer.DefaultTypes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace BinarySerializer
{
    public class PropertyData
    {
        public PropertyInfo Property { get; set; }

        public BinaryAttribute Attrib { get; set; }

        public BasicType Type { get; set; }

        public Func<object> CreateInstance { get; set; }

        public Func<object, int> GetSize { get; set; }

        public Func<object, int> GetArraySize { get; set; }

        public Action<object, object> Setter { get; set; }

        public Func<object, object> Getter { get; set; }

        public Action<BinarySerializer> Serialize;

        public Action<BinarySerializer> Deserialize;

        internal PropertyTypeEnum PropertyType;

        public PropertyData(PropertyInfo property, Dictionary<Type, BasicType> TypeInstanceMap, BinaryAttribute attrib)
        {
            Property = property;
            Attrib = attrib;

            BasicType type;

            if (TypeInstanceMap.TryGetValue(attrib.Type, out type))
            {
                Type = type;
            }
            else
            {
                CreateInstance = Expression.Lambda<Func<object>>(Expression.New(attrib.Type)).Compile();
            }
        }

        public void FillSize(List<PropertyData> PropertyList)
        {
            if (Type != null && Type.FixedSize == true)
                GetSize = new Func<object, int>((o) => { return Type.Size; });
            else
            {
                if (string.IsNullOrEmpty(Attrib.TypeSizeName))
                {
                    GetSize = new Func<object, int>((o) => { return Attrib.TypeSize; });
                }
                else
                {
                    var DynamicTypeSizeProperty = PropertyList.Find(x => x.Property.Name == Attrib.TypeSizeName);
                    GetSize = new Func<object, int>((o) => { return Convert.ToInt32(DynamicTypeSizeProperty.Getter(o)); });
                }
            }

            if (!typeof(IList).IsAssignableFrom(Property.PropertyType))
            {
                GetArraySize = new Func<object, int>((o) => { return 0; });

                PropertyType = Type == null ?
                    PropertyTypeEnum.ClassType :
                    (
                        Type.FixedSize ?
                        PropertyTypeEnum.FixedBasicType :
                        PropertyTypeEnum.DynamicBasicType
                    );
            }
            else
            {
                if (string.IsNullOrEmpty(Attrib.ArraySizeName))
                {
                    GetArraySize = new Func<object, int>((o) => { return Attrib.ArraySize; });
                }
                else
                {
                    var DynamicArraySizeProperty = PropertyList.Find(x => x.Property.Name == Attrib.ArraySizeName);
                    GetArraySize = new Func<object, int>((o) => { return (int)DynamicArraySizeProperty.Getter(o); });
                }

                PropertyType = Type == null ?
                    (Property.PropertyType.IsArray ?
                        PropertyTypeEnum.ClassArrayType :
                        PropertyTypeEnum.ClassListType
                    ) :
                    (Property.PropertyType.IsArray ?
                        (
                            Type.FixedSize ?
                            PropertyTypeEnum.FixedBasicArrayType :
                            PropertyTypeEnum.DynamicBasicArrayType
                        ) :
                        (
                            Type.FixedSize ?
                            PropertyTypeEnum.FixedBasicListType :
                            PropertyTypeEnum.DynamicBasicListType
                        )
                    );
            }

        }

        private void WriteArrayPrimitiveFixed(BinarySerializer bs)
        {
            int arr_len = GetArraySize(bs.ProcessObject);

            while (bs.Buffer.Length - bs.Offset < Type.Size * arr_len)
            {
                Array.Resize(ref bs.Buffer, bs.Buffer.Length * 2);
            }

            var f_array = (ICollection)Getter(bs.ProcessObject);

            int i = 0;

            foreach (var item in f_array)
            {
                if (arr_len < ++i)
                    break;
                Type.GetBytes(ref bs.Buffer, bs.Offset, item);
                bs.Offset += Type.Size;
            }

            if (arr_len > f_array.Count)
                bs.Offset += Type.Size * (arr_len - f_array.Count);
        }

        private void ReadArrayPrimitiveFixed(BinarySerializer bs)
        {
            int arr_len = GetArraySize(bs.ProcessObject);

            IList r = (IList)Activator.CreateInstance(Property.PropertyType, arr_len);

            for (int i = 0; i < arr_len; i++)
            {
                r[i] = Type.GetValue(ref bs.Buffer, bs.Offset);

                bs.Offset += Type.Size;
            }

            Setter(bs.ProcessObject, r);
        }

        private void ReadListPrimitiveFixed(BinarySerializer bs)
        {
            int arr_len = GetArraySize(bs.ProcessObject);

            IList r = (IList)Activator.CreateInstance(Property.PropertyType, arr_len);

            for (int i = 0; i < arr_len; i++)
            {
                r.Add(Type.GetValue(ref bs.Buffer, bs.Offset));

                bs.Offset += Type.Size;
            }

            Setter(bs.ProcessObject, r);
        }

        private void WriteArrayPrimitive(BinarySerializer bs)
        {
            int arr_len = GetArraySize(bs.ProcessObject);
            BasicType tinst = bs.GetTypeInstance(Type.GetType());
            tinst.Size = GetSize(bs.ProcessObject);

            while (bs.Buffer.Length - bs.Offset < tinst.Size * arr_len)
            {
                Array.Resize(ref bs.Buffer, bs.Buffer.Length * 2);
            }

            var f_array = (ICollection)Getter(bs.ProcessObject);

            int i = 0;

            foreach (var item in f_array)
            {
                if (arr_len < ++i)
                    break;
                tinst.GetBytes(ref bs.Buffer, bs.Offset, item);
                bs.Offset += tinst.Size;
            }

            if (arr_len > f_array.Count)
                bs.Offset += tinst.Size * (arr_len - f_array.Count);
        }

        private void ReadArrayPrimitive(BinarySerializer bs)
        {
            int arr_len = GetArraySize(bs.ProcessObject);
            BasicType tinst = bs.GetTypeInstance(Type.GetType());
            tinst.Size = GetSize(bs.ProcessObject);

            IList r = (IList)Activator.CreateInstance(Property.PropertyType, arr_len);

            for (int i = 0; i < arr_len; i++)
            {
                r[i] = tinst.GetValue(ref bs.Buffer, bs.Offset);

                bs.Offset += tinst.Size;
            }

            Setter(bs.ProcessObject, r);
        }

        private void ReadListPrimitive(BinarySerializer bs)
        {
            int arr_len = GetArraySize(bs.ProcessObject);
            BasicType tinst = bs.GetTypeInstance(Type.GetType());
            tinst.Size = GetSize(bs.ProcessObject);

            IList r = (IList)Activator.CreateInstance(Property.PropertyType, arr_len);

            for (int i = 0; i < arr_len; i++)
            {
                r.Add(tinst.GetValue(ref bs.Buffer, bs.Offset));

                bs.Offset += tinst.Size;
            }

            Setter(bs.ProcessObject, r);
        }

        private void WritePrimitive(BinarySerializer bs)
        {
            BasicType tinst = bs.GetTypeInstance(Type.GetType());
            tinst.Size = GetSize(bs.ProcessObject);
            while (bs.Buffer.Length - bs.Offset < tinst.Size)
            {
                Array.Resize(ref bs.Buffer, bs.Buffer.Length * 2);
            }

            tinst.GetBytes(ref bs.Buffer, bs.Offset, Getter(bs.ProcessObject));
            bs.Offset += tinst.Size;
        }

        private void ReadPrimitive(BinarySerializer bs)
        {
            BasicType tinst = bs.GetTypeInstance(Type.GetType());
            tinst.Size = GetSize(bs.ProcessObject);
            Setter(bs.ProcessObject, tinst.GetValue(ref bs.Buffer, bs.Offset));
            bs.Offset += tinst.Size;
        }

        private void ReadEnumPrimitive(BinarySerializer bs)
        {
            BasicType tinst = bs.GetTypeInstance(Type.GetType());
            tinst.Size = GetSize(bs.ProcessObject);
            Setter(bs.ProcessObject, Enum.ToObject(Property.PropertyType, tinst.GetValue(ref bs.Buffer, bs.Offset)));
            bs.Offset += tinst.Size;
        }

        private void WritePrimitiveFixed(BinarySerializer bs)
        {
            while (bs.Buffer.Length - bs.Offset < Type.Size)
            {
                Array.Resize(ref bs.Buffer, bs.Buffer.Length * 2);
            }

            Type.GetBytes(ref bs.Buffer, bs.Offset, Getter(bs.ProcessObject));
            bs.Offset += Type.Size;
        }

        private void ReadPrimitiveFixed(BinarySerializer bs)
        {
            Setter(bs.ProcessObject, Type.GetValue(ref bs.Buffer, bs.Offset));
            bs.Offset += Type.Size;
        }

        private void ReadEnumPrimitiveFixed(BinarySerializer bs)
        {
            Setter(bs.ProcessObject, Enum.ToObject(Property.PropertyType, Type.GetValue(ref bs.Buffer, bs.Offset)));
            bs.Offset += Type.Size;
        }

        private void WriteArrayType(BinarySerializer bs)
        {
            int arr_len = GetArraySize(bs.ProcessObject);

            var f_array = (ICollection)Getter(bs.ProcessObject);

            int q = 0;

            var oldProcessObject = bs.ProcessObject;
            foreach (var item in f_array)
            {
                if (arr_len < ++q)
                    break;
                bs.Serialize(bs.SchemeName, item);
            }

            for (; q < arr_len; q++)
            {
                bs.Serialize(bs.SchemeName, CreateInstance());
            }

            bs.ProcessObject = oldProcessObject;
        }

        private void ReadArrayType(BinarySerializer bs)
        {
            int arr_len = GetArraySize(bs.ProcessObject);

            IList r = (IList)Activator.CreateInstance(Property.PropertyType, arr_len);
            var oldProcessObject = bs.ProcessObject;
            for (int i = 0; i < arr_len; i++)
            {
                r[i] = (bs.Deserialize(bs.SchemeName, Attrib.Type, bs.TypeStorage.GetTypeInfo(bs.SchemeName, Attrib.Type, bs.TypeInstanceMap)));
            }
            bs.ProcessObject = oldProcessObject;

            Setter(bs.ProcessObject, r);
        }

        private void ReadListType(BinarySerializer bs)
        {
            int arr_len = GetArraySize(bs.ProcessObject);

            IList r = (IList)Activator.CreateInstance(Property.PropertyType, arr_len);
            var oldProcessObject = bs.ProcessObject;
            for (int i = 0; i < arr_len; i++)
            {
                r.Add(bs.Deserialize(bs.SchemeName, Attrib.Type, bs.TypeStorage.GetTypeInfo(bs.SchemeName, Attrib.Type, bs.TypeInstanceMap)));
            }
            bs.ProcessObject = oldProcessObject;

            Setter(bs.ProcessObject, r);
        }

        private void WriteType(BinarySerializer bs)
        {
            var oldProcessObject = bs.ProcessObject;
            bs.Serialize(bs.SchemeName, Getter(bs.ProcessObject));
            bs.ProcessObject = oldProcessObject;
        }

        private void ReadType(BinarySerializer bs)
        {
            var oldProcessObject = bs.ProcessObject;
            Setter(bs.ProcessObject, bs.Deserialize(bs.SchemeName, Attrib.Type, bs.TypeStorage.GetTypeInfo(bs.SchemeName, Attrib.Type, bs.TypeInstanceMap)));
            bs.ProcessObject = oldProcessObject;
        }

        public void SetMethods(Action<object, object> setter, Func<object, object> getter)
        {
            Getter = getter;
            Setter = setter;

            switch (PropertyType)
            {
                case PropertyTypeEnum.FixedBasicType:
                    Serialize = new Action<BinarySerializer>(WritePrimitiveFixed);
                    Deserialize = Property.PropertyType.IsEnum ? new Action<BinarySerializer>(ReadEnumPrimitiveFixed) : new Action<BinarySerializer>(ReadPrimitiveFixed);
                    break;
                case PropertyTypeEnum.DynamicBasicType:
                    Serialize = new Action<BinarySerializer>(WritePrimitive);
                    Deserialize = Property.PropertyType.IsEnum ? new Action<BinarySerializer>(ReadEnumPrimitive) : new Action<BinarySerializer>(ReadPrimitive);
                    break;
                case PropertyTypeEnum.FixedBasicArrayType:
                    Serialize = new Action<BinarySerializer>(WriteArrayPrimitiveFixed);
                    Deserialize = new Action<BinarySerializer>(ReadArrayPrimitiveFixed);
                    break;
                case PropertyTypeEnum.DynamicBasicArrayType:
                    Serialize = new Action<BinarySerializer>(WriteArrayPrimitive);
                    Deserialize = new Action<BinarySerializer>(ReadArrayPrimitive);
                    break;
                case PropertyTypeEnum.ClassType:
                    Serialize = new Action<BinarySerializer>(WriteType);
                    Deserialize = new Action<BinarySerializer>(ReadType);
                    break;
                case PropertyTypeEnum.ClassArrayType:
                    Serialize = new Action<BinarySerializer>(WriteArrayType);
                    Deserialize = new Action<BinarySerializer>(ReadArrayType);
                    break;
                case PropertyTypeEnum.FixedBasicListType:
                    Serialize = new Action<BinarySerializer>(WriteArrayPrimitiveFixed);
                    Deserialize = new Action<BinarySerializer>(ReadListPrimitiveFixed);
                    break;
                case PropertyTypeEnum.DynamicBasicListType:
                    Serialize = new Action<BinarySerializer>(WriteArrayPrimitive);
                    Deserialize = new Action<BinarySerializer>(ReadListPrimitive);
                    break;
                case PropertyTypeEnum.ClassListType:
                    Serialize = new Action<BinarySerializer>(WriteArrayType);
                    Deserialize = new Action<BinarySerializer>(ReadListType);
                    break;
                default:
                    break;
            }
        }

        public override string ToString()
        {
            return $"{Property.Name}";
        }
    }
}
