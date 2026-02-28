using MediatR;

namespace EA.Application.Features.PlacementTests.GetPlacementTestStatus;

public record GetPlacementTestStatusQuery(string UserId) : IRequest<PlacementTestStatusDto>;
