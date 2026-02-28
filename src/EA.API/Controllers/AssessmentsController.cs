using System.Security.Claims;
using EA.Application.Features.Assessments.GetAssessmentsByScope;
using EA.Application.Features.Assessments.SubmitAssessmentResult;
using EA.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EA.API.Controllers;

[Authorize]
public class AssessmentsController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> GetByScope([FromQuery] AssessmentScopeType scopeType, [FromQuery] Guid scopeId)
    {
        var result = await Mediator.Send(new GetAssessmentsByScopeQuery(scopeType, scopeId));
        return Ok(result);
    }

    [HttpPost("submit")]
    public async Task<IActionResult> SubmitResult([FromBody] SubmitAssessmentResultRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var command = new SubmitAssessmentResultCommand(userId, request.AssessmentId, request.Score);
        var id = await Mediator.Send(command);
        return Ok(new { id });
    }
}

public record SubmitAssessmentResultRequest(Guid AssessmentId, int Score);
