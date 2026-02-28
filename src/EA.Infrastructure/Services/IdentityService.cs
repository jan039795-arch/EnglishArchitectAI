using EA.Application.Common;
using EA.Application.Contracts;
using EA.Domain.Entities;
using EA.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace EA.Infrastructure.Services;

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtTokenService _jwtTokenService;

    public IdentityService(UserManager<ApplicationUser> userManager, IJwtTokenService jwtTokenService)
    {
        _userManager = userManager;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<Result<string>> RegisterAsync(string email, string username, string password)
    {
        var user = new ApplicationUser
        {
            Email = email,
            UserName = username
        };

        var result = await _userManager.CreateAsync(user, password);

        if (!result.Succeeded)
            return Result<string>.Failure(result.Errors.Select(e => e.Description));

        var roles = await _userManager.GetRolesAsync(user);
        var token = _jwtTokenService.GenerateToken(user, roles);

        return Result<string>.Success(token);
    }

    public async Task<Result<string>> LoginAsync(string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user is null)
            return Result<string>.Failure("Invalid credentials.");

        var passwordValid = await _userManager.CheckPasswordAsync(user, password);

        if (!passwordValid)
            return Result<string>.Failure("Invalid credentials.");

        var roles = await _userManager.GetRolesAsync(user);
        var token = _jwtTokenService.GenerateToken(user, roles);

        return Result<string>.Success(token);
    }

    public async Task<Result> CompletePlacementAsync(string userId, CEFRLevel assignedLevel)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
            return Result.Failure($"User {userId} not found.");

        user.PlacementCompleted = true;
        user.CEFRLevel = assignedLevel;
        user.UpdatedAt = DateTime.UtcNow;

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
            return Result.Failure(result.Errors.Select(e => e.Description));

        return Result.Success();
    }
}
