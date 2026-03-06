using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NSL.ASPNET.Identity.Host;
using NSL.ASPNET.Mvc;
using NSL.ASPNET.Mvc.Route.Attributes;
using NSL.BlazorTemplate.Shared.Models.RequestModels;
using NSL.BlazorTemplate.Shared.Server.Data;
using NSL.BlazorTemplate.Shared.Server.Manages;
using NSL.BlazorTemplate.Shared;
using NSL.BlazorTemplate.Shared.Controllers;

namespace NSL.BlazorTemplate.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class UserController(ApplicationDbContext dbContext
        , IConfiguration configuration
        , AppUserManager appUserManager
        ) : Controller, IUserController
    {
        [HttpPostAction]
        public async Task<IActionResult> Details(CancellationToken cancellationToken)
        {
            var uid = User.GetId();

            var user = await dbContext.Users.FindAsync([uid], cancellationToken: cancellationToken);

            return this.DataResponse((object)user.ToDetails());
        }

        [HttpPostAction]
        public async Task<IActionResult> Edit([FromBody] IdentityIndexRequestModel query, CancellationToken cancellationToken)
        {
            var uid = User.GetId();

            var user = await appUserManager.FindByIdAsync(uid);

            if (user == null) return this.NotFoundResponse();

            user.PhoneNumber = query.PhoneNumber;

            await appUserManager.UpdateAsync(user);

            return Ok();
        }

        [HttpPostAction]
        public async Task<IActionResult> ChangeEmail([FromBody] IdentityEmailRequestModel query, CancellationToken cancellationToken)
        {
            var uid = User.GetId();

            var user = await appUserManager.FindByIdAsync(uid);

            if (user == null) return this.NotFoundResponse();

            user.Email = query.NewEmail;

            await appUserManager.UpdateAsync(user);

            return Ok();
        }

        [HttpPostAction]
        public async Task<IActionResult> ChangePassword([FromBody] IdentityChangePasswordRequestModel query, CancellationToken cancellationToken)
        {
            var uid = User.GetId();

            var user = await appUserManager.FindByIdAsync(uid);

            if (user == null) return this.NotFoundResponse();

            var result = await appUserManager.ChangePasswordAsync(user, query.OldPassword, query.NewPassword);

            if (!result.Succeeded) return this.ModelStateResponse(result.JoinErrors());

            return Ok();
        }
    }
}
