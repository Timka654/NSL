#if !DEVELOP

using NSL.Generators.SelectTypeGenerator.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSL.Generators.SelectTypeGenerator.Tests
{
    [SelectGenerateModelJoin("GetLevel2", "GetLevel1")]
    [SelectGenerateModelJoin("GetLevel3", "GetLevel2", Recursive = false)]
    internal class MultiLevelJoinModel2
    {
        [SelectGenerateInclude("GetLevel1")]
        public int Id { get; set; }

        [SelectGenerateInclude("GetLevel2")]
        public string Name { get; set; }

        [SelectGenerateInclude("GetLevel3")]
        public string NameLevel3 { get; set; }
    }
}

#endif