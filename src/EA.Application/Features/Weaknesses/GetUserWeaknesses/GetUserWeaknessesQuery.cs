using MediatR;

namespace EA.Application.Features.Weaknesses.GetUserWeaknesses;

public record GetUserWeaknessesQuery(string UserId) : IRequest<List<UserWeaknessDto>>;
