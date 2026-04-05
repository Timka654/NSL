#if !DEVELOP

using NSL.Generators.SelectTypeGenerator.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSL.Generators.SelectTypeGenerator.Tests
{
    internal class RegexProxyModel2
    {
        [SelectGenerateInclude("v")]
        [SelectGenerateInclude("vall")]
        public int v { get; set; }

        [SelectGenerateInclude("v")]
        public int v1 { get; set; }

        [SelectGenerateInclude("v")]
        public int v2 { get; set; }

        [SelectGenerateInclude("v")]
        public int v3 { get; set; }
    }
}

#endif