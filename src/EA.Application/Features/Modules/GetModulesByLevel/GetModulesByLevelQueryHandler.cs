using EA.Application.Contracts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EA.Application.Features.Modules.GetModulesByLevel;

public class GetModulesByLevelQueryHandler : IRequestHandler<GetModulesByLevelQuery, List<ModuleDto>>
{
    private readonly IApplicationDbContext _context;

    public GetModulesByLevelQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ModuleDto>> Handle(GetModulesByLevelQuery request, CancellationToken cancellationToken)
    {
        return await _context.Modules
            .Where(m => m.LevelId == request.LevelId)
            .OrderBy(m => m.Order)
            .Select(m => new ModuleDto(m.Id, m.LevelId, m.Title, m.Description, m.Order, m.YoutubePlaylistId, m.EstimatedHours))
            .ToListAsync(cancellationToken);
    }
}
