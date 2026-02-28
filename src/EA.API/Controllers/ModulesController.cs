using EA.Application.Features.Modules.GetModulesByLevel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EA.API.Controllers;

[Authorize]
public class ModulesController : BaseApiController
{
    [HttpGet("by-level/{levelId:guid}")]
    public async Task<IActionResult> GetModulesByLevel(Guid levelId)
    {
        var result = await Mediator.Send(new GetModulesByLevelQuery(levelId));
        return Ok(result);
    }
}
