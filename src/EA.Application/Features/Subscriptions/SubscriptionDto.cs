namespace EA.Application.Features.Subscriptions;

public record SubscriptionDto(Guid Id, string Plan, DateTime StartDate, DateTime EndDate, string Status);
