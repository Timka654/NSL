using NSL.Generators.SelectTypeGenerator.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSL.Generators.SelectTypeGenerator.Tests.Develop
{
    [SelectGenerate("base", "level1", "level2", "level3")]
    [SelectGenerateModelJoin("level1", "base")]
    [SelectGenerateModelJoin("level2", "level1")]
    [SelectGenerateModelJoin("level3", "level2")]
    internal class MultiLevelJoinModel1
    {
        [SelectGenerateInclude("base")]
        public int Id { get; set; }

        [SelectGenerateInclude("level1")]
        [SelectGenerateProxy("level1", "GetLevel1")]
        public MultiLevelJoinModel2 Level1 { get; set; }

        [SelectGenerateInclude("level2")]
        [SelectGenerateProxy("level2", "GetLevel2")]
        [SelectGenerateProxy("GetLevel1")]
        public MultiLevelJoinModel2 Level2 { get; set; }

        [SelectGenerateInclude("level3")]
        [SelectGenerateProxy("GetLevel3")]
        public MultiLevelJoinModel2 Level3 { get; set; }
    }
}
