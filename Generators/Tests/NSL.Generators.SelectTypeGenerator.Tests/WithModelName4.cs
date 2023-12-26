#if !DEVELOP
using NSL.Generators.SelectTypeGenerator.Attributes;

namespace NSL.Generators.FillTypeGenerator.Tests
{
    [SelectGenerate("model1", "model2", "model3")]
    public partial class WithModelName4
    {
        [SelectGenerateInclude("model1", "model2", "model3")]
        [SelectGenerateProxy("model1", "model4")]
        [SelectGenerateProxy("model2", "model5")]
        [SelectGenerateProxy("model6")]
        public WithModelName5 Abc2 { get; set; }
    }
}
#endif