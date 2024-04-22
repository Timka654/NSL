using Microsoft.AspNetCore.Mvc;

namespace NSL.ASPNET.Mvc.Route.Attributes
{
    public class HttpPostActionAttribute : HttpPostAttribute
    {
        public HttpPostActionAttribute() : base("[action]")
        {
        }
    }
}
