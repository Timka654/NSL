using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSL.ASPNET.ModelBinders;
using NSL.ASPNET.Mvc;
using NSL.ASPNET.Mvc.Route.Attributes;
using NSL.Generators.FillTypeGenerator.Tests.Develop.WithModelName;
using NSL.Generators.HttpEndPointGenerator.Shared.Attributes;
using NSL.Generators.HttpEndPointGenerator.Tests.Shared;
using System;
using System.Threading.Tasks;

namespace NSL.Generators.HttpEndPointGenerator.Tests.Server.Controllers
{
    [Route("api/[controller]")]
    public class TestController : Controller, ITestController
    {
        [HttpPostAction]
        public async Task<IActionResult> TestPost([FromForm] WithModelName2 query)
        {
            return Ok();
        }

        [HttpPostAction]
        public async Task<IActionResult> TestPost2([FromForm,ModelBinder<FormDataJsonBinder>] WithModelName3 query)
        {
            return Ok();
        }

        [HttpPostAction]
        public async Task<IActionResult> TestPost3([FromBody] WithModelName3 query)
        {
            return Ok();
        }

        [HttpPostAction]
        public async Task<IActionResult> TestPost4([FromForm, ModelBinder<FormDataJsonBinder>] WithModelName3 query, [FromForm] IFormFile file)
        {
            return Ok();
        }

        [HttpPostAction]
        public async Task<IActionResult> TestPost5([FromForm] WithModelName4 query)
        {
            return Ok();
        }

        [HttpPostAction]
        public async Task<IActionResult> TestPost6([FromForm, ModelBinder<FormDataJsonBinder>] WithModelName3 query, [FromForm] IFormFileCollection file)
        {
            return Ok();
        }

        [HttpPostAction]
        public async Task<IActionResult> TestPost7([FromForm, ModelBinder<FormDataJsonBinder>] WithModelName3 query, [FromForm] IFormFileCollection file, [FromHeader(Name = "abc")] string h1, [FromHeader] string abc2)
        {
            return Ok();
        }


        [HttpPostAction]
        public async Task<DataResponse<TestBaseModel1>> NewPost1([FromForm] WithModelName2 query)
        {
            return DataResponse.Ok(new TestBaseModel1() { Abc1 = 1, Abc2 = 2 });
        }

        [HttpPostAction]
        public Task<IActionResult> TestPost8([FromForm, HttpEndPointParameter(GenHttpParameterEnum.Particle)] WithModelName4 query)
        {
            throw new NotImplementedException();
        }

        [HttpPostAction]
        public Task<IActionResult> TestPost9([FromForm] WithModelName3 query, [FromForm] IFormFileCollection file, [FromHeader(Name = "abc"), HttpEndPointParameter(GenHttpParameterEnum.Normal)] string h1, [FromHeader, HttpEndPointParameter(GenHttpParameterEnum.Normal)] string abc2)
        {
            throw new NotImplementedException();
        }

        [HttpPostAction]
        public Task<IActionResult> TestPost10([FromForm, HttpEndPointParameter(GenHttpParameterEnum.Particle)] WithModelName4 query)
        {
            throw new NotImplementedException();
        }

        [HttpPostAction]
        public Task<IActionResult> TestPost11([FromForm] WithModelName3 query, [FromForm] IFormFileCollection file, [FromHeader(Name = "abc"), HttpEndPointParameter(GenHttpParameterEnum.Normal)] string h1, [FromHeader, HttpEndPointParameter(GenHttpParameterEnum.Normal)] string abc2)
        {
            throw new NotImplementedException();
        }
    }
}
