using EA.Application.Common;
using EA.Domain.Enums;

namespace EA.Application.Contracts;

public interface IIdentityService
{
    Task<Result<string>> RegisterAsync(string email, string username, string password);
    Task<Result<string>> LoginAsync(string email, string password);
    Task<Result> CompletePlacementAsync(string userId, CEFRLevel assignedLevel);
}
