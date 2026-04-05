#if !DEVELOP
using NSL.Generators.SelectTypeGenerator.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSL.Generators.SelectTypeGenerator.Tests.Proxy
{
    [SelectGenerate("Get")]
    public partial class ProxyModel3
    {
        [SelectGenerateInclude("Get")] public Guid Id { get; set; }
    }
}
#endif