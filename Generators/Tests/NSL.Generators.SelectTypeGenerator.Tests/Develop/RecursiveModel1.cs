using NSL.Generators.SelectTypeGenerator.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSL.Generators.SelectTypeGenerator.Tests.Develop
{

    [SelectGenerate("model1")]
    internal partial class RecursiveModel1
    {
        [SelectGenerateInclude("model1")]
        [SelectGenerateProxy("model2")]
        public RecursiveModel2[] recursiveModel2s { get; set; }
    }

    [SelectGenerate("model2")]
    internal partial class RecursiveModel2
    {
        [SelectGenerateInclude("model2")]
        [SelectGenerateProxy("model3")]
        public RecursiveModel3 recursiveModel3s { get; set; }
    }

    [SelectGenerate("model3")]
    internal partial class RecursiveModel3
    {
        [SelectGenerateInclude("model3")]
        //[SelectGenerateProxy("model1")]
        public RecursiveModel1 recursiveModel1s { get; set; }
    }
}
