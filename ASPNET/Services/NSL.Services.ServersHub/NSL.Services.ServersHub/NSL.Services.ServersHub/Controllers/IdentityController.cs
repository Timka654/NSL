﻿using Microsoft.AspNetCore.Mvc;
using NSL.ASPNET.Identity.Host;
using NSL.ASPNET.Mvc;
using NSL.ASPNET.Mvc.Route.Attributes;
using NSL.Services.ServersHub.Client.Pages.Account.Pages;
using NSL.Services.ServersHub.Shared.Controllers;
using NSL.Services.ServersHub.Shared.Models;
using NSL.Services.ServersHub.Shared.Models.RequestModels;
using NSL.Services.ServersHub.Shared.Server.Manages;

namespace NSL.Services.ServersHub.Controllers
{
    [Route("api/[controller]")]
    public class IdentityController(
        IConfiguration configuration,
        AppSignInManager signInManager) : Controller, IIdentityController
    {
        [HttpPostAction]
        public async Task<IActionResult> Login([FromBody] IdentityLoginRequestModel query)
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
        public async Task<IActionResult> Register([FromBody] IdentityRegisterRequestModel query)
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
