using MediatR;

namespace EA.Application.Features.Subscriptions.GetUserSubscription;

public record GetUserSubscriptionQuery(string UserId) : IRequest<SubscriptionDto?>;
