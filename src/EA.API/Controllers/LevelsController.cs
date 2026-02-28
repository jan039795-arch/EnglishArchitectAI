using EA.Application.Features.Levels.GetLevelById;
using EA.Application.Features.Levels.GetLevels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EA.API.Controllers;

[Authorize]
public class LevelsController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> GetLevels()
    {
        var result = await Mediator.Send(new GetLevelsQuery());
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetLevelById(Guid id)
    {
        var result = await Mediator.Send(new GetLevelByIdQuery(id));
        if (result is null) return NotFound();
        return Ok(result);
    }
}
