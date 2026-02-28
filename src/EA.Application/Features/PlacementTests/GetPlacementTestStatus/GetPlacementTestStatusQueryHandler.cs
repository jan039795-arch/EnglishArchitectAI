using EA.Application.Contracts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EA.Application.Features.PlacementTests.GetPlacementTestStatus;

public class GetPlacementTestStatusQueryHandler : IRequestHandler<GetPlacementTestStatusQuery, PlacementTestStatusDto>
{
    private readonly IApplicationDbContext _context;

    public GetPlacementTestStatusQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PlacementTestStatusDto> Handle(GetPlacementTestStatusQuery request, CancellationToken cancellationToken)
    {
        var test = await _context.PlacementTests
            .Where(t => t.UserId == request.UserId)
            .OrderByDescending(t => t.CompletedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (test is null)
            return new PlacementTestStatusDto(false, null, null);

        return new PlacementTestStatusDto(true, test.FinalLevel.ToString(), test.CompletedAt);
    }
}
