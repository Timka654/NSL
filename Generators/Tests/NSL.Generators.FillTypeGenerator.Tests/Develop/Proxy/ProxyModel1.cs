using NSL.Generators.FillTypeGenerator.Attributes;

namespace NSL.Generators.FillTypeGenerator.Tests.Develop.Proxy
{
    [FillTypeGenerate(typeof(ProxyModel1), "InstanceUpdate")]
    internal partial class ProxyModel1
    {
        [FillTypeGenerateInclude("InstanceUpdate")]
        [FillTypeGenerateProxy(default)]
        public List<ProxyModel2> M2List { get; set; }
    }
}
