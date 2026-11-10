
using System.ComponentModel.DataAnnotations;
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
        // if(string.IsNullOrWhiteSpace(request.Name) ||
        //     string.IsNullOrWhiteSpace(request.Email) ||
        //     string.IsNullOrWhiteSpace(request.Password))
        // {
        //     return BadRequest(new
        //     {
        //         code = "VALIDATION_ERROR",
        //         message = "Name, email and password is must."
        //     });
        //}
        var errors = new Dictionary<string, string[]>();
        if(string.IsNullOrWhiteSpace(request.Name))
        {
            errors["name"] = new[] { "REQUIRED" };
        }
        else if (request.Name.Trim().Length > 100)
        {
            errors["name"] = new[] { "MAX_LENGTH_100" };
        }
        if (string.IsNullOrWhiteSpace(request.Email))
            errors["email"] = new[] { "REQUIRED" };
        else if (!new EmailAddressAttribute().IsValid(request.Email.Trim()))
            errors["email"] = new[] { "INVALID_EMAIL" };

        if(string.IsNullOrWhiteSpace(request.Password))
        {
            errors["password"] = new[] { "REQUIRED" };
        }

        if(errors.Count > 0)
        {
            return BadRequest(new
            {
                status = 400,
                code = "VALIDATION_ERROR",
                errors
            });
        }
        var name = request.Name?.Trim() ?? string.Empty;
        var email = request.Email?.Trim() ?? string.Empty;
        var password = request.Password ?? string.Empty;
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Name = name,
            UserName = email,
            Email = email
        };

        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            var identityErrors = new Dictionary<string, List<string>>();

            foreach (var error in result.Errors)
            {
                var (field, code) = error.Code switch
                {
                    "DuplicateEmail" or "DuplicateUserName"
                        => ("email", "EMAIL_ALREADY_USED"),

                    "InvalidEmail"
                        => ("email", "INVALID_EMAIL"),

                    "PasswordTooShort"
                        => ("password", "PASSWORD_TOO_SHORT"),

                    "PasswordRequiresDigit"
                        => ("password", "PASSWORD_REQUIRES_DIGIT"),

                    "PasswordRequiresLower"
                        => ("password", "PASSWORD_REQUIRES_LOWERCASE"),

                    "PasswordRequiresUpper"
                        => ("password", "PASSWORD_REQUIRES_UPPERCASE"),

                    "PasswordRequiresNonAlphanumeric"
                        => ("password", "PASSWORD_REQUIRES_SPECIAL_CHARACTER"),

                    _ => ("general", "REGISTRATION_FAILED")
                };

                if (!identityErrors.ContainsKey(field))
                {
                    identityErrors[field] = new List<string>();
                }

                if (!identityErrors[field].Contains(code))
                {
                    identityErrors[field].Add(code);
                }
            }

            return BadRequest(new
            {
                status = 400,
                code = "VALIDATION_ERROR",
                errors = identityErrors
            });
        }
        return StatusCode(StatusCodes.Status201Created, new
        {
            id = user.Id,
            name = user.Name,
            email = user.Email
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request)
    {
         var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.Email))
            errors["email"] = new[] { "REQUIRED" };
        if(string.IsNullOrWhiteSpace(request.Password))
            errors["password"] = new[] { "REQUIRED"};
        if(errors.Count > 0)
        {
            return BadRequest(new
            {
                status = 400,
                code = "VALIDATION_ERROR",
                errors
            });
        }
        var email = request.Email?.Trim() ?? string.Empty;
        var password = request.Password ?? string.Empty;

        var user = await _userManager.FindByEmailAsync(email);

        if(user is null)
        {
            return Unauthorized(new
            {
                status = 401,
                code = "INVALID_CREDENTIALS"
            });
        }
        var passwordIsValid = await _userManager.CheckPasswordAsync(
            user,
            password);
        if(!passwordIsValid)
        {
            return Unauthorized(new
            {
                status = 401,
                code = "INVALID_CREDENTIALS"
            });
        }

        //ı will create JWT and cookie
        return Ok(new
        {
            id = user.Id,
            name = user.Name,
            email = user.Email
        });
    }
}
