#if SERVER

using NSL.Generators.SelectTypeGenerator.Attributes;

namespace DevProject1
{

    [SelectGenerate("Get")]
    public partial class Model1
    {
        [SelectGenerateInclude("Get")]
        public int val2 { get; set; }
    }
}

#endif