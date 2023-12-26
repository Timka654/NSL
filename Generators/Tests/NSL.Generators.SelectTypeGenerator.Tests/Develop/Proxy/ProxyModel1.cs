using NSL.Generators.SelectTypeGenerator.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSL.Generators.SelectTypeGenerator.Tests.Develop.Proxy
{
    [SelectGenerate("TestGet")]
    public partial class ProxyModel1
    {
        [SelectGenerateInclude("TestGet")]
        public int Id { get; set; }

        [SelectGenerateInclude("TestGet")]
        [SelectGenerateProxy("Get")]
        public virtual List<ProxyModel2> List { get; set; }
    }
}
