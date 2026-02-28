using System.Security.Claims;
using EA.Application.Features.PlacementTests.GetPlacementTestStatus;
using EA.Application.Features.PlacementTests.SubmitPlacementTest;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EA.API.Controllers;

[Authorize]
public class PlacementTestController : BaseApiController
{
    [HttpGet("status")]
    public async Task<IActionResult> GetStatus()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await Mediator.Send(new GetPlacementTestStatusQuery(userId));
        return Ok(result);
    }

    [HttpPost("submit")]
    public async Task<IActionResult> Submit([FromBody] SubmitPlacementTestRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var command = new SubmitPlacementTestCommand(userId, request.Answers);
        var assignedLevel = await Mediator.Send(command);
        return Ok(new { assignedLevel });
    }
}

public record SubmitPlacementTestRequest(List<PlacementAnswer> Answers);
