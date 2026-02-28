using System.Security.Claims;
using EA.Application.Features.Certificates.GetUserCertificates;
using EA.Application.Features.Certificates.VerifyCertificate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EA.API.Controllers;

[Authorize]
public class CertificatesController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> GetUserCertificates()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await Mediator.Send(new GetUserCertificatesQuery(userId));
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpGet("verify/{verificationCode:guid}")]
    public async Task<IActionResult> VerifyCertificate(Guid verificationCode)
    {
        var result = await Mediator.Send(new VerifyCertificateQuery(verificationCode));
        if (result is null) return NotFound();
        return Ok(result);
    }
}
