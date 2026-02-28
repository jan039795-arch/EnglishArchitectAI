using System.Security.Claims;
using EA.Application.Features.Speaking.SubmitSpeakingAttempt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EA.API.Controllers;

[Authorize]
public class SpeakingController : BaseApiController
{
    [HttpPost("submit")]
    public async Task<IActionResult> SubmitAttempt([FromBody] SubmitSpeakingAttemptRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var command = new SubmitSpeakingAttemptCommand(
            userId, request.ExerciseId, request.AudioBlobUrl, request.TranscriptText,
            request.AccuracyScore, request.ProsodyScore, request.CompletenessScore,
            request.OverallScore, request.FeedbackJson);
        var id = await Mediator.Send(command);
        return Ok(new { id });
    }
}

public record SubmitSpeakingAttemptRequest(
    Guid ExerciseId,
    string? AudioBlobUrl,
    string? TranscriptText,
    double AccuracyScore,
    double ProsodyScore,
    double CompletenessScore,
    double OverallScore,
    string? FeedbackJson);
