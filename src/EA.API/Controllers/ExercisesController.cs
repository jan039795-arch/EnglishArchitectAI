using System.Security.Claims;
using EA.Application.Features.Exercises.GetExercisesByLesson;
using EA.Application.Features.Exercises.SubmitResponse;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EA.API.Controllers;

[Authorize]
public class ExercisesController : BaseApiController
{
    [HttpGet("by-lesson/{lessonId:guid}")]
    public async Task<IActionResult> GetExercisesByLesson(Guid lessonId)
    {
        var result = await Mediator.Send(new GetExercisesByLessonQuery(lessonId));
        return Ok(result);
    }

    [HttpPost("submit")]
    public async Task<IActionResult> SubmitResponse([FromBody] SubmitResponseRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var command = new SubmitResponseCommand(userId, request.ExerciseId, request.UserAnswer, request.TimeTakenSeconds);
        var result = await Mediator.Send(command);
        return Ok(result);
    }
}

public record SubmitResponseRequest(Guid ExerciseId, string UserAnswer, int TimeTakenSeconds);
