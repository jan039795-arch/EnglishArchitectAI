using EA.Application.Features.Auth.Commands.Login;
using EA.Application.Features.Auth.Commands.Register;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EA.API.Controllers;

[AllowAnonymous]
public class AuthController : BaseApiController
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserCommand command)
    {
        var result = await Mediator.Send(command);
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors });
        return Ok(new { token = result.Value });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginUserCommand command)
    {
        var result = await Mediator.Send(command);
        if (!result.Succeeded)
            return Unauthorized(new { errors = result.Errors });
        return Ok(new { token = result.Value });
    }
}
