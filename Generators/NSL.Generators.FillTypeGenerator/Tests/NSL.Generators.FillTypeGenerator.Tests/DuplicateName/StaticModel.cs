#if !DEVELOP
using NSL.Generators.FillTypeGenerator.Shared;

namespace NSL.Generators.FillTypeGenerator.Tests.DuplicateName
{
    [FillTypeGenerate(typeof(StaticModel))]
    partial class StaticModel
    {
        public static object a { get; set; } = new object();

        public int b { get; set; }  
    }

}
#endif