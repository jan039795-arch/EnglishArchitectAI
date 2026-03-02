using System.Security.Claims;
using EA.Application.Features.LessonComments.AddLessonComment;
using EA.Application.Features.LessonComments.GetLessonComments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EA.API.Controllers;

[Authorize]
public class LessonCommentsController : BaseApiController
{
    [HttpGet("{lessonId:guid}")]
    public async Task<IActionResult> GetLessonComments(Guid lessonId)
    {
        var result = await Mediator.Send(new GetLessonCommentsQuery(lessonId));
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> AddLessonComment([FromBody] AddCommentRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await Mediator.Send(new AddLessonCommentCommand(request.LessonId, userId, request.Body));
        return Ok(result);
    }
}

public record AddCommentRequest(Guid LessonId, string Body);
