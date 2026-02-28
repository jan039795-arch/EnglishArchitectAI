using EA.Application.Contracts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EA.Application.Features.Levels.GetLevels;

public class GetLevelsQueryHandler : IRequestHandler<GetLevelsQuery, List<LevelDto>>
{
    private readonly IApplicationDbContext _context;

    public GetLevelsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<LevelDto>> Handle(GetLevelsQuery request, CancellationToken cancellationToken)
    {
        return await _context.Levels
            .OrderBy(l => l.Order)
            .Select(l => new LevelDto(l.Id, l.Code, l.Name, l.Order, l.UnlockRequirement))
            .ToListAsync(cancellationToken);
    }
}
