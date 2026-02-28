using System.Security.Claims;
using EA.Application.Features.Weaknesses.GetUserWeaknesses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EA.API.Controllers;

[Authorize]
public class WeaknessesController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> GetUserWeaknesses()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await Mediator.Send(new GetUserWeaknessesQuery(userId));
        return Ok(result);
    }
}
