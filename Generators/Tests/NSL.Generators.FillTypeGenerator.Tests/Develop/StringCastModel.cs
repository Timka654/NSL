using NSL.Generators.FillTypeGenerator.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSL.Generators.FillTypeGenerator.Tests.Develop
{
    [FillTypeFromGenerate(typeof(StringCast2Model))]
    partial class StringCast1Model
    {
        public string Property1 { get; set; }
    }

    partial class StringCast2Model
    {
        public IStringCastInterface Property1 { get; set; }
    }

    partial interface IStringCastInterface
    {

    }
}
