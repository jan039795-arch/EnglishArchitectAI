namespace EA.Application.Features.Weaknesses;

public record UserWeaknessDto(Guid Id, string Tag, double WeaknessScore, int TotalAttempts, int FailCount, DateTime LastUpdated);
