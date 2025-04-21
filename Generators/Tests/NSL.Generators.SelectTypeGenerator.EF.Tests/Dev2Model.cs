using NSL.Generators.SelectTypeGenerator.Attributes;

namespace NSL.Generators.SelectTypeGenerator.EF.Tests
{
    [SelectGenerate("Get")]
    public class Dev2Model
    {
        [SelectGenerateInclude($"Get")]
        public int Id { get; set; }

        [SelectGenerateInclude($"Get")]
        public string Data { get; set; }

        public virtual Dev1Model? Parent { get; set; }
    }
}
