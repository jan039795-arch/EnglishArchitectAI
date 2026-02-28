using MediatR;

namespace EA.Application.Features.SpacedRepetition.ReviewCard;

public record ReviewCardCommand(string UserId, Guid CardId, int Quality) : IRequest;
