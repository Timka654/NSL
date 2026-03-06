#if !DEVELOP
using NSL.Generators.SelectTypeGenerator.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSL.Generators.SelectTypeGenerator.Tests
{
    public partial class ProxyModel2
    {

        [SelectGenerateInclude("Get")]
        public virtual ProxyModel3 P3 { get; set; }
    }
}
#endif