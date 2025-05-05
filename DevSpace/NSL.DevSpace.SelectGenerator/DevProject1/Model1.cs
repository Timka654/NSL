using NSL.Generators.SelectTypeGenerator.Attributes;

namespace DevProject1
{
    [SelectGenerate("Get", Typed = true, Dto = true)]
    [SelectGenerateModelJoin("Get", "query")]
    public partial class Model1
    {
        [SelectGenerateInclude("Get")]
        public int val1 { get; set; }

        [SelectGenerateInclude("query")]
        public int val3 { get; set; }
    }
}
