using EA.Application.Contracts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EA.Application.Features.Subscriptions.GetUserSubscription;

public class GetUserSubscriptionQueryHandler : IRequestHandler<GetUserSubscriptionQuery, SubscriptionDto?>
{
    private readonly IApplicationDbContext _context;

    public GetUserSubscriptionQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SubscriptionDto?> Handle(GetUserSubscriptionQuery request, CancellationToken cancellationToken)
    {
        return await _context.Subscriptions
            .Where(s => s.UserId == request.UserId)
            .OrderByDescending(s => s.StartDate)
            .Select(s => new SubscriptionDto(s.Id, s.Plan.ToString(), s.StartDate, s.EndDate, s.Status.ToString()))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
