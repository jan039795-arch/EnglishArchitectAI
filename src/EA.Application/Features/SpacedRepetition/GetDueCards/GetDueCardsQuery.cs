using MediatR;

namespace EA.Application.Features.SpacedRepetition.GetDueCards;

public record GetDueCardsQuery(string UserId, int MaxCards = 20) : IRequest<List<SpacedRepetitionCardDto>>;
