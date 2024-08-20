#if SERVER
using Microsoft.AspNetCore.Mvc;
#else
using NSL.Generators.HttpEndPointGenerator.Shared.Fake.Attributes;
using NSL.Generators.HttpEndPointGenerator.Shared.Fake.Interfaces;
#endif

using NSL.Generators.HttpEndPointGenerator.Shared.Attributes;
using NSL.BlazorTemplate.Shared.Models.RequestModels;
using NSL.HttpClient.Models;

namespace NSL.BlazorTemplate.Shared.Controllers
{
    [HttpEndPointContainerGenerate("api/[controller]")]
    public interface IIdentityController
    {
        [HttpEndPointGenerate(typeof(DataResponse<string>))] Task<IActionResult> Login([FromBody] IdentityLoginRequestModel query);

        [HttpEndPointGenerate(typeof(DataResponse<string>))] Task<IActionResult> Register([FromBody] IdentityRegisterRequestModel query);
    }
}
