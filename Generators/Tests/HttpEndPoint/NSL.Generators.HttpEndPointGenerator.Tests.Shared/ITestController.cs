using DevExtensions.Blazor.Http.Models;
using NSL.Generators.FillTypeGenerator.Tests.Develop.WithModelName;
using NSL.Generators.HttpEndPointGenerator.Attributes;
using NSL.Generators.HttpEndPointGenerator.Tests.Shared.Develop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSL.Generators.HttpEndPointGenerator.Tests.Shared
{
    [HttpEndPointContainerGenerate("api/Test")]
    public interface ITestController
    {
        [HttpEndPointGenerate(typeof(IdResponse<Guid>))]
        public Task<IActionResult> TestPost([FromBody] WithModelName2 query);
    }
}
