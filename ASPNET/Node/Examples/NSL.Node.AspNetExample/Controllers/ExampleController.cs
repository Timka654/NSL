using Microsoft.AspNetCore.Mvc;

namespace NSL.Node.AspNetExample.Controllers
{
    [Route("[controller]")]
    public class ExampleController : Controller
    {
        private readonly ILogger<ExampleController> _logger;

        public ExampleController(ILogger<ExampleController> logger)
        {
            _logger = logger;
        }

        [HttpGet("[action]")]
        public IActionResult Get()
        {
            return Ok();
        }
    }
}