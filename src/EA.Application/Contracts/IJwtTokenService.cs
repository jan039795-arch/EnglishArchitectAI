using EA.Domain.Entities;

namespace EA.Application.Contracts;

public interface IJwtTokenService
{
    string GenerateToken(ApplicationUser user, IList<string> roles);
}
