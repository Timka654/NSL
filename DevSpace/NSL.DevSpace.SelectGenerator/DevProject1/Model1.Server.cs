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

#if CLIENT

using NSL.Generators.SelectTypeGenerator.Attributes;

namespace DevProject1
{

    [SelectGenerate("Get22")]
    public partial class Model1
    {
        [SelectGenerateInclude("Get22")]
        public int val4 { get; set; }
    }
}

#endif