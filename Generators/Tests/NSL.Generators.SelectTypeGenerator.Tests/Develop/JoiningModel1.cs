using NSL.Generators.SelectTypeGenerator.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSL.Generators.SelectTypeGenerator.Tests.Develop
{
    [SelectGenerate("Get2")]
    [SelectGenerate("Get2")]
    [SelectGenerate("Get2")]
    [SelectGenerate("Get1")]
    [SelectGenerate("Get3")]
    [SelectGenerateModelJoin("Get2", "Get1", "Get3")]
    public partial class JoiningModel1
    {
        [SelectGenerateInclude("Get1")][SelectGenerateInclude("Get2")][SelectGenerateInclude("Get3")] public bool v1 { get; set; }

        [SelectGenerateInclude("Get2")] public bool v2 { get; set; }

        [SelectGenerateInclude("Get3")] public bool v3 { get; set; }

        //[SelectGenerateInclude("Get1"), SelectGenerateProxy("Get1", "Get")]
        //[SelectGenerateInclude("Get3"), SelectGenerateProxy("Get3", "Get")]
        //[SelectGenerateInclude("Get2"), SelectGenerateProxy("Get2","test")]
        public JoiningModel2 c1 { get; set; }

        public JoiningModel2[] a1 { get; set; }

        [SelectGenerateInclude("Get1")]
        [SelectGenerateInclude("Get3")]
        [SelectGenerateInclude("Get2"), SelectGenerateProxy("Get2", "test")]
        [SelectGenerateProxy("Get")]
        public List<JoiningModel2> l1 { get; set; }
    }

    public partial class JoiningModel2
    {
        [SelectGenerateInclude("Get")] public string b1 { get; set; }

        [SelectGenerateInclude("test")]public string b2 { get; set; }

        [SelectGenerateInclude("Get")] public string b3 { get; set; }
    }
}
