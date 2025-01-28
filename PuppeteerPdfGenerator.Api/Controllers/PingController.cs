using Microsoft.AspNetCore.Mvc;

namespace PuppeteerPdfGenerator.Api.Controllers
{
    [ApiController]
    [Route("api")]
    public class PingController : ControllerBase
    {
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok("pong");
        }

        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new
            {
                Status = "Healthy",
                Timestamp = DateTime.UtcNow
            });
        }
    }
}
