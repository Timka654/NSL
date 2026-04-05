#if !DEVELOP
using NSL.Generators.FillTypeGenerator.Shared;

namespace NSL.Generators.FillTypeGenerator.Tests.DuplicateName
{
    [FillTypeFromGenerate(typeof(IDuplicateNameModel))]
    public partial interface IDuplicateNameModel
    {
        string Id { get; set; }
    }

}
#endif
