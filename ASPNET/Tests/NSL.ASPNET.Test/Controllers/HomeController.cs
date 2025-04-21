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

        [HttpPostAction]
        public async Task<DataResponse<TestResponseModel>?> Error400([FromBody] TestRequestModel query)
            => await this.ProcessRequestAsync(async () =>
            {
                return DataResponse.Ok(new TestResponseModel() { A = ""});
            });


    }
}


public class TestResponseModel
{
    public string A { get; set; }
    public string B { get; set; }
}

public class TestRequestModel
{
    public string A { get; set; }
    public string B { get; set; }
}