#if !DEVELOP

using NSL.Generators.FillTypeGenerator.Attributes;

namespace NSL.Generators.FillTypeGenerator.Tests
{
    [FillTypeGenerate(typeof(Test4Model))]
    [FillTypeGenerate(typeof(Test5Model))]
    [FillTypeGenerate(typeof(Test4Model), "abc1", "abc2")]
    public partial class Test3Model
    {
        [FillTypeGenerateInclude("abc1")] public int Abc1 { get; set; }

        [FillTypeGenerateInclude("abc1", "abc2")] public int Abc2 { get; set; }

        [FillTypeGenerateInclude("abc2")] public int Abc3 { get; set; }

        [FillTypeGenerateInclude("abc2"), FillTypeGenerateIgnore(typeof(Test4Model))] public int Abc4 { get; set; }
    }
}

#endif