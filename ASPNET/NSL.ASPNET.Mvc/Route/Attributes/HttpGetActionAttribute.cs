using Microsoft.AspNetCore.Mvc;

namespace NSL.ASPNET.Mvc.Route.Attributes
{
    public class HttpGetActionAttribute : HttpGetAttribute
    {
        public HttpGetActionAttribute() : base("[action]")
        {
        }
    }
}
