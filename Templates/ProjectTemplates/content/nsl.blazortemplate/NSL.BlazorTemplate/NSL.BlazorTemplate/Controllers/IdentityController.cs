using Microsoft.AspNetCore.Mvc;
using NSL.ASPNET.Identity.Host;
using NSL.ASPNET.Mvc;
using NSL.ASPNET.Mvc.Route.Attributes;
using NSL.BlazorTemplate.Client.Pages.Account.Pages;
using NSL.BlazorTemplate.Shared.Controllers;
using NSL.BlazorTemplate.Shared.Models;
using NSL.BlazorTemplate.Shared.Models.RequestModels;
using NSL.BlazorTemplate.Shared.Server.Manages;

namespace NSL.BlazorTemplate.Controllers
{
    [Route("api/[controller]")]
    public class IdentityController(
        IConfiguration configuration,
        AppSignInManager signInManager) : Controller, IIdentityController
    {
        [HttpPostAction]
        public async Task<IActionResult> Login([FromBody] IdentityLoginRequestModel query, CancellationToken cancellationToken = default)
            => await this.ProcessRequestAsync(async () =>
            {
                var user = await signInManager.UserManager.FindByNameAsync(query.Email);

                if (user == null)
                    return this.ModelStateResponse("User not found");

                var checkPassword = await signInManager.CheckPasswordSignInAsync(user, query.Password, false);


                if (!checkPassword.Succeeded)
                    return this.ModelStateResponse("User not found");

                var token = user.GenerateClaims((u, c) =>
                {

                }).GenerateJWT(configuration);

                return this.DataResponse(token);
            });

        [HttpPostAction]
        public async Task<IActionResult> Register([FromBody] IdentityRegisterRequestModel query, CancellationToken cancellationToken)
            => await this.ProcessRequestAsync(async () =>
            {
                var u = new UserModel()
                {
                    UserName = query.Email,
                    Email = query.Email
                };

                var result = await signInManager.UserManager.CreateAsync(u, query.Password);
                if (result.Succeeded)
                {
                    var token = u.GenerateClaims((u,c) =>
                    {

                    }).GenerateJWT(configuration);

                    return this.DataResponse(token);
                }
                return Ok();
            });
    }
}
