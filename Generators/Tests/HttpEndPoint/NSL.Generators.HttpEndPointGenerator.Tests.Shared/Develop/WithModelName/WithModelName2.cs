using Microsoft.AspNetCore.Mvc;
using NSL.ASPNET.ModelBinders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSL.Generators.FillTypeGenerator.Tests.Develop.WithModelName
{
    public partial class WithModelName2
    {
        public IFormFile file { get; set; }

        [ModelBinder<FormDataJsonBinder>]
        public WithModelName3 Abc3 { get; set; }
    }
    public partial class WithModelName4
    {
        public IFormFileCollection file { get; set; }

        [ModelBinder<FormDataJsonBinder>]
        public WithModelName3 Abc3 { get; set; }
    }

    public partial class WithModelName3
    {
        public int Abc1 { get; set; }

        public int Abc2 { get; set; }

        public WithModelName3 dev { get; set; }
    }
}
