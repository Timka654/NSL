using NSL.Generators.SelectTypeGenerator.Attributes;

namespace NSL.Generators.SelectTypeGenerator.EF.Tests
{
    [SelectGenerate("GetTest")]
    public class Dev1Model
    {
        [SelectGenerateInclude($"GetTest")]
        public int Id { get; set; }

        [SelectGenerateInclude($"GetTest")]
        public string Data { get; set; }

        [SelectGenerateInclude($"GetTest")]
        [SelectGenerateProxy("Get")]
        public virtual List<Dev2Model>? Childs { get; set; }
    }
}
