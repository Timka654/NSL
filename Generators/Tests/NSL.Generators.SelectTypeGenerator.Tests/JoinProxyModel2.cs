#if !DEVELOP
using NSL.Generators.SelectTypeGenerator.Attributes;

namespace NSL.Generators.SelectTypeGenerator.Tests
{
    [SelectGenerateModelJoin("Get", "Get1", "Get2")]
    [SelectGenerateModelJoin("GetA", "Get1")]
    public partial class JoinProxyModel2
    {
        [SelectGenerateInclude("Get1")]
        public string v1 { get; set; }

        [SelectGenerateInclude("Get2")]
        public string v2 { get; set; }

        [SelectGenerateInclude("Get")]
        public string v3 { get; set; }

        [SelectGenerateInclude("Get_n")]
        public string v4 { get; set; }

        [SelectGenerateInclude("GetA")]
        public string v5 { get; set; }
    }
}
#endif