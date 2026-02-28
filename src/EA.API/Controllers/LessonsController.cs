using EA.Application.Features.Lessons.GetLessonById;
using EA.Application.Features.Lessons.GetLessonsByModule;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EA.API.Controllers;

[Authorize]
public class LessonsController : BaseApiController
{
    [HttpGet("by-module/{moduleId:guid}")]
    public async Task<IActionResult> GetLessonsByModule(Guid moduleId)
    {
        var result = await Mediator.Send(new GetLessonsByModuleQuery(moduleId));
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetLessonById(Guid id)
    {
        var result = await Mediator.Send(new GetLessonByIdQuery(id));
        if (result is null) return NotFound();
        return Ok(result);
    }
}
