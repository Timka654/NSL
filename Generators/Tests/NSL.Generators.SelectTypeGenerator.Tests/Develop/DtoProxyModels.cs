using NSL.Generators.SelectTypeGenerator.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSL.Generators.SelectTypeGenerator.Tests.Develop
{
    [SelectGenerate("dto", Dto = true)]
    //[SelectGenerate("no_dto")]
    internal class DtoProxyModel1
    {
        [SelectGenerateInclude("dto")] public string Name1 { get; set; }
        [SelectGenerateInclude("no_dto")] public string Name2 { get; set; }
        [SelectGenerateInclude("dto")] public string Name3 { get; set; }

        //[SelectGenerateInclude("dto", "no_dto")]
        //[SelectGenerateProxy("dto", "no_dto")]
        //[SelectGenerateProxy("dto")]
        //public List<DtoProxyModel2> dtoProxyModel2s { get; set; } = new List<DtoProxyModel2>();

        [SelectGenerateInclude("dto", "no_dto")]
        [SelectGenerateProxy("dto", "dto")]
        //[SelectGenerateProxy("no_dto")]
        public List<DtoProxyModel2> dtoProxyModel3s { get; set; } = new List<DtoProxyModel2>();
    }

    [SelectGenerate("dto", Dto = true)]
    [SelectGenerate("no_dto")]
    public class DtoProxyModel2
    {
        [SelectGenerateInclude("dto")] public string Name1 { get; set; }
        [SelectGenerateInclude("no_dto")] public string Name2 { get; set; }
        [SelectGenerateInclude("dto")] public string Name3 { get; set; }
    }
}
