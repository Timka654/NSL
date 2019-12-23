using System.Reflection;

namespace BinarySerializer.Builder
{
    public interface IBuilderMemberData
    {
        StructBuilder CurrentBuilder { get; set; }
        BinaryMemberData Member { get; }
    }

    public class BuilderPropertyData : PropertyData, IBuilderMemberData
    {
        public BuilderPropertyData(PropertyInfo propertyInfo, TypeStorage storage) : base(propertyInfo,storage, true)
        {
        }

        public StructBuilder CurrentBuilder { get; set; }
        public BinaryMemberData Member { get => this; }
    }

    public class BuilderFieldData : FieldData, IBuilderMemberData
    {
        public BuilderFieldData(FieldInfo propertyInfo, TypeStorage storage) : base(propertyInfo, storage, true)
        {
        }

        public StructBuilder CurrentBuilder { get; set; }
        public BinaryMemberData Member { get => this; }
    }
}
