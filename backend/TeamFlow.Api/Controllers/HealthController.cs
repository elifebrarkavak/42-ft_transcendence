using Microsoft.AspNetCore.Mvc;

namespace TeamFlow.Api.Controllers;

[ApiController]
[Route("api/health")]
public class HealthController : ControllerBase
{
    [HttpGet]                    // GET api/health
    public IActionResult Get()
    {
        return Ok(new { status = "ok", time = DateTime.UtcNow });
    }

    [HttpGet("ping")]            // GET api/health + /ping
    public IActionResult Ping()
    {
        return Ok(new { message = "pong" });
    }
}