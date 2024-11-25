using Microsoft.AspNetCore.Mvc;
using NSL.Generators.FillTypeGenerator.Tests.Develop.WithModelName;
using NSL.Generators.HttpEndPointGenerator.Shared.Attributes;
using NSL.HttpClient.Models;

namespace NSL.Generators.HttpEndPointGenerator.Tests.Shared
{
    [HttpEndPointContainerGenerate("api/[controller]")]
    public interface ITestController
    {
        [HttpEndPointGenerate(typeof(IdResponse<Guid>))] public Task<IActionResult> TestPost([FromForm, HttpEndPointParameter(GenHttpParameterEnum.Particle)] WithModelName2 query);

        [HttpEndPointGenerate(typeof(IdResponse<Guid>))] public Task<IActionResult> TestPost2([FromForm] WithModelName3 query);

        [HttpEndPointGenerate(typeof(IdResponse<Guid>))] public Task<IActionResult> TestPost3([FromBody] WithModelName3 query);

        [HttpEndPointGenerate(typeof(IdResponse<Guid>))] public Task<IActionResult> TestPost4([FromForm] WithModelName3 query, [FromForm] IFormFile file);


        [HttpEndPointGenerate(typeof(IdResponse<Guid>))] public Task<IActionResult> TestPost5([FromForm, HttpEndPointParameter(GenHttpParameterEnum.Particle)] WithModelName4 query);

        [HttpEndPointGenerate(typeof(IdResponse<Guid>))] public Task<IActionResult> TestPost6([FromForm] WithModelName3 query, [FromForm] IFormFileCollection file);

        [HttpEndPointGenerate(typeof(IdResponse<Guid>))] public Task<IActionResult> TestPost7([FromForm] WithModelName3 query, [FromForm] IFormFileCollection file, [FromHeader(Name = "abc"), HttpEndPointParameter] string h1, [FromHeader, HttpEndPointParameter] string abc2);
    }
}
