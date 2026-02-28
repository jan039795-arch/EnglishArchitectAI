using System.Security.Claims;
using EA.Application.Features.Subscriptions.GetUserSubscription;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EA.API.Controllers;

[Authorize]
public class SubscriptionsController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> GetUserSubscription()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await Mediator.Send(new GetUserSubscriptionQuery(userId));
        if (result is null) return NotFound();
        return Ok(result);
    }
}
