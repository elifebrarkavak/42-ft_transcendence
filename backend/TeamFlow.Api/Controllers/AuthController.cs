
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TeamFlow.Api.Domain;
using TeamFlow.Api.Dtos;

namespace TeamFlow.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;

    public AuthController(UserManager<ApplicationUser> userManager)
    {
         _userManager = userManager;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request)
    {
        if(string.IsNullOrWhiteSpace(request.Name) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new
            {
                code = "VALIDATION_ERROR",
                message = "Name, email and password is must."
            });
        }
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            UserName = request.Email.Trim(),
            Email = request.Email.Trim()
        };

        var result = await _userManager.CreateAsync(
            user,
            request.Password);
        
        if(!result.Succeeded)
        {
            return BadRequest(new
            {
                code = "REGISTRATION_FAILED",
                errors = result.Errors.Select(error => new
                {
                    code = error.Code
                })
            });
        }
        return StatusCode(StatusCodes.Status201Created, new
        {
            id = user.Id,
            name = user.Name,
            email = user.Email
        });
    }
}
