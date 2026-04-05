#if !DEVELOP
using NSL.Generators.FillTypeGenerator.Shared;

namespace NSL.Generators.FillTypeGenerator.Tests.Proxy
{
    [FillTypeGenerate(typeof(ProxyModel1), "InstanceUpdate")]
    internal partial class ProxyModel1
    {
        [FillTypeGenerateInclude("InstanceUpdate")]
        [FillTypeGenerateProxy(default)]
        public List<ProxyModel2> M2List { get; set; }
    }
}
#endif