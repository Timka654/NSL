using System.Reflection;

namespace BinarySerializer.Builder
{
    public class BuilderMemberData : BinaryMemberData
    {
        public StructBuilder CurrentBuilder;

        public BuilderMemberData(BinaryMemberData binaryMemberData, string scheme, TypeStorage storage) : base(binaryMemberData, scheme, storage)
        {
        }
    }

    public class BuilderPropertyData : BuilderMemberData
    {
        public BuilderPropertyData(PropertyInfo propertyInfo, TypeStorage storage) : base(new PropertyData(propertyInfo,storage, true), "", storage)
        {
        }
    }

    public class BuilderFieldData : BuilderMemberData
    {
        public BuilderFieldData(FieldInfo propertyInfo, TypeStorage storage) : base(new FieldData(propertyInfo, storage, true), "", storage)
        {
        }
    }
}
