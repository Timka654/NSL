﻿using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace NSL.ASPNET.Mvc
{
    public static class ActionProcessing
    {
        public static async Task<IActionResult> ProcessRequestAsync(this ControllerBase controller, Func<Task> action, Func<IActionResult> result)
        {
            if (!controller.ModelState.IsValid)
                return controller.ModelStateResponse();

            await action();

            return result();
        }

        public static async Task<IActionResult> ProcessRequestAsync(this ControllerBase controller, Func<Task<IActionResult>> action)
        {
            if (!controller.ModelState.IsValid)
                return controller.ModelStateResponse();

            return await action();
        }

        public static async Task<TActionResult> ProcessRequestAsync<TActionResult>(this ControllerBase controller, Func<Task> action, Func<TActionResult> result)
            where TActionResult : IActionResult
        {
            if (!controller.ModelState.IsValid)
            {
                await controller.ModelStateResponse().ExecuteResultAsync(controller.ControllerContext);
                return default;
            }
            await action();

            return result();
        }

        public static async Task<TActionResult> ProcessRequestAsync<TActionResult>(this ControllerBase controller, Func<Task<TActionResult>> action)
            where TActionResult : IActionResult
        {
            if (!controller.ModelState.IsValid)
            {
                await controller.ModelStateResponse().ExecuteResultAsync(controller.ControllerContext);
                return default;
            }

            return await action();
        }


        public static IActionResult IdResponse(this ControllerBase controller, object id)
            => controller.Ok(new { id });

        public static IActionResult DataResponse(this ControllerBase controller, object data)
            => controller.Ok(new { Data = data });

        public static IActionResult NotFoundResponse(this ControllerBase controller, string errorMessage)
            => ControllerResults.NotFoundResponse(controller.ModelState, errorMessage);

        public static IActionResult NotFoundResponse(this ControllerBase controller)
            => controller.NotFoundResponse("{...no_found}");

        public static IActionResult ModelStateResponse(this ControllerBase controller, string errorMessage)
            => ControllerResults.ModelStateResponse(controller.ModelState, errorMessage);

        public static IActionResult ModelStateResponse(this ControllerBase controller, string[] errorMessages)
            => ControllerResults.ModelStateResponse(controller.ModelState, errorMessages);

        public static IActionResult ModelStateResponse(this ControllerBase controller, string errorKey, string errorMessage)
            => ControllerResults.ModelStateResponse(controller.ModelState, errorKey, errorMessage);

        public static IActionResult ModelStateResponse(this ControllerBase controller, params (string key, string message)[] errorMessages)
            => ControllerResults.ModelStateResponse(controller.ModelState, errorMessages);

        public static IActionResult ModelStateResponse(this ControllerBase controller)
            => ControllerResults.ModelStateResponse(controller.ModelState);
    }
}
