using System.Security.Claims;
using EA.Application.Features.Progress.CompleteLesson;
using EA.Application.Features.Progress.GetUserProgress;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EA.API.Controllers;

[Authorize]
public class ProgressController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> GetUserProgress()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await Mediator.Send(new GetUserProgressQuery(userId));
        return Ok(result);
    }

    [HttpPost("complete-lesson")]
    public async Task<IActionResult> CompleteLesson([FromBody] CompleteLessonRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var command = new CompleteLessonCommand(userId, request.LessonId, request.Score, request.TimeSpentSeconds);
        var id = await Mediator.Send(command);
        return Ok(new { id });
    }
}

public record CompleteLessonRequest(Guid LessonId, int Score, int TimeSpentSeconds);
