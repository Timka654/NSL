using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NSL.ASPNET.Mvc;
using NSL.ASPNET.Mvc.Route.Attributes;

namespace NSL.ASPNET.Test.Controllers
{
    [Route("[controller]")]
    public class HomeController : Controller
    {
        [HttpPostAction]
        public BaseResponse Index()
        {
            return BaseResponse.Ok();
        }
    }
}
