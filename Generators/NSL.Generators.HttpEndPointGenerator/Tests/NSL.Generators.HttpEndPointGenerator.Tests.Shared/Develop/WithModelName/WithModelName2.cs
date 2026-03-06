#if NSL_SERVER
using Microsoft.AspNetCore.Mvc;
#elif NSL_CLIENT
using NSL.Generators.HttpEndPointGenerator.Shared.Fake.Interfaces;
using NSL.Generators.HttpEndPointGenerator.Shared.Fake;
#endif
using NSL.ASPNET.ModelBinders;

namespace NSL.Generators.FillTypeGenerator.Tests.Develop.WithModelName
{
    public partial class WithModelName2
    {
        public IFormFile file { get; set; }

#if NSL_SERVER
        [ModelBinder<FormDataJsonBinder>]
#endif
        public WithModelName3 Abc3 { get; set; }
    }
    public partial class WithModelName4
    {
        public IFormFileCollection file { get; set; }

#if NSL_SERVER
        [ModelBinder<FormDataJsonBinder>]
#endif
        public WithModelName3 Abc3 { get; set; }
    }

    public partial class WithModelName3
    {
        public int Abc1 { get; set; }

        public int Abc2 { get; set; }

        public WithModelName3 dev { get; set; }
    }

    public partial class TestBaseModel1
    {
        public int Abc1 { get; set; }

        public int Abc2 { get; set; }
    }
}
