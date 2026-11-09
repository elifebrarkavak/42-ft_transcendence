using Microsoft.AspNetCore.Mvc;

namespace TeamFlow.Api.Controllers;

[ApiController]
[Route("api/health")]
public class HealthController : ControllerBase
{
    [HttpGet]                    // GET api/health
    public IActionResult Get()
    {
        return Ok(new
        {
            status = "ok",
            application = "TeamFlow",
            time = DateTime.UtcNow
        });
    }

    [HttpGet("ping")]            // GET api/health + /ping
    public IActionResult Ping()
    {
        return Ok(new { message = "pong" });
    }

    [HttpGet("greet")]
    public IActionResult Greet([FromQuery] string? name)
    {
        if(string.IsNullOrWhiteSpace(name))
        {
            return BadRequest(new
            {
                code = "NAME_REQUIRED",
                message = "Name must."
            });
        }

        return Ok(new
        {
           message = $"Merhaba, {name}!" 
        });
    }
}
