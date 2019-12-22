using System.Reflection;

namespace BinarySerializer.Builder
{
    public class BuilderPropertyData : PropertyData
    {
        public StructBuilder CurrentBuilder;

        public BuilderPropertyData(PropertyInfo propertyInfo, TypeStorage storage) : base(propertyInfo, storage,true)
        {
        }

        public BuilderPropertyData(PropertyData propertyData, string scheme, TypeStorage storage) : base(propertyData, scheme, storage)
        {
        }
    }
}
