#if NSL_SERVER
using Microsoft.AspNetCore.Mvc;
#else
using NSL.Generators.HttpEndPointGenerator.Shared.Fake.Attributes;
using NSL.Generators.HttpEndPointGenerator.Shared.Fake.Interfaces;
#endif

using NSL.Generators.HttpEndPointGenerator.Shared.Attributes;
using NSL.BlazorTemplate.Shared.Models;
using NSL.BlazorTemplate.Shared.Models.RequestModels;
using NSL.HttpClient.Models;

namespace NSL.BlazorTemplate.Shared.Controllers
{
    [HttpEndPointContainerGenerate("api/[controller]")]
    public interface IUserController
    {
        [HttpEndPointGenerate(typeof(BaseResponse))] Task<IActionResult> ChangeEmail([FromBody] IdentityEmailRequestModel query, CancellationToken cancellationToken);

        [HttpEndPointGenerate(typeof(BaseResponse))] Task<IActionResult> ChangePassword([FromBody] IdentityChangePasswordRequestModel query, CancellationToken cancellationToken);

        [HttpEndPointGenerate(typeof(DataResponse<UserModel>))] Task<IActionResult> Details(CancellationToken cancellationToken);

        [HttpEndPointGenerate(typeof(BaseResponse))] Task<IActionResult> Edit([FromBody] IdentityIndexRequestModel query, CancellationToken cancellationToken);
    }
}
