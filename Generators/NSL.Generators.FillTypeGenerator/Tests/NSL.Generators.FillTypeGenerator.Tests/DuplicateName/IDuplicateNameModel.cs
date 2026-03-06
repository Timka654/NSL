#if !DEVELOP
using NSL.Generators.FillTypeGenerator.Attributes;

namespace NSL.Generators.FillTypeGenerator.Tests.Develop
{
    [FillTypeFromGenerate(typeof(IDuplicateNameModel))]
    public partial interface IDuplicateNameModel
    {
        string Id { get; set; }
    }

}
#endif
