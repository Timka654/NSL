using NSL.Generators.SelectTypeGenerator.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSL.Generators.SelectTypeGenerator.Tests.Develop
{
    [SelectGenerate("all", "v1", "v2", "v3")]
    [SelectGenerateModelJoin("v1", "all")]
    [SelectGenerateModelJoin("v2", "all")]
    [SelectGenerateModelJoin("v3", "all")]
    internal class RegexProxyModel1
    {
        [SelectGenerateInclude("all")]
        [SelectGenerateProxy("v(\\S*\\s*)", "v")]
        [SelectGenerateProxy("vall")]
        public RegexProxyModel2 p { get; set; }

    }
}
