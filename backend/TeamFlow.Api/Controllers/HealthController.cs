using Microsoft.AspNetCore.Mvc;

namespace TeamFlow.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new
            {
                status = "ok",
                time = DateTime.UtcNow
            });
        }
    }
