using System.Security.Claims;
using EA.Application.Features.SpacedRepetition.GetDueCards;
using EA.Application.Features.SpacedRepetition.ReviewCard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EA.API.Controllers;

[Authorize]
public class SpacedRepetitionController : BaseApiController
{
    [HttpGet("due")]
    public async Task<IActionResult> GetDueCards([FromQuery] int maxCards = 20)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await Mediator.Send(new GetDueCardsQuery(userId, maxCards));
        return Ok(result);
    }

    [HttpPost("review")]
    public async Task<IActionResult> ReviewCard([FromBody] ReviewCardRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await Mediator.Send(new ReviewCardCommand(userId, request.CardId, request.Quality));
        return NoContent();
    }
}

public record ReviewCardRequest(Guid CardId, int Quality);
