#if !DEVELOP
using NSL.Generators.SelectTypeGenerator.Attributes;

namespace NSL.Generators.SelectTypeGenerator.Tests.Develop
{
    [SelectGenerate("dto", Dto = true)]
    [SelectGenerate("no_dto")]
    public class DtoProxyModel2
    {
        [SelectGenerateInclude("dto")] public string Name1 { get; set; }
        [SelectGenerateInclude("no_dto")] public string Name2 { get; set; }
        [SelectGenerateInclude("dto")] public string Name3 { get; set; }
    }
}

#endif