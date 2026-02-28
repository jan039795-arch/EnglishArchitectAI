using EA.Application.Contracts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EA.Application.Features.Weaknesses.GetUserWeaknesses;

public class GetUserWeaknessesQueryHandler : IRequestHandler<GetUserWeaknessesQuery, List<UserWeaknessDto>>
{
    private readonly IApplicationDbContext _context;

    public GetUserWeaknessesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<UserWeaknessDto>> Handle(GetUserWeaknessesQuery request, CancellationToken cancellationToken)
    {
        return await _context.UserWeaknesses
            .Where(w => w.UserId == request.UserId)
            .OrderByDescending(w => w.WeaknessScore)
            .Select(w => new UserWeaknessDto(w.Id, w.Tag, w.WeaknessScore, w.TotalAttempts, w.FailCount, w.LastUpdated))
            .ToListAsync(cancellationToken);
    }
}
