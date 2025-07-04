#if !DEVELOP
using NSL.Generators.FillTypeGenerator.Attributes;

namespace NSL.Generators.FillTypeGenerator.Tests.Develop
{
    [FillTypeGenerate(typeof(StaticModel))]
    partial class StaticModel
    {
        public static object a { get; set; } = new object();

        public int b { get; set; }  
    }

}
#endif