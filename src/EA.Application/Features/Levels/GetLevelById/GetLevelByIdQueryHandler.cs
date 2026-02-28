using EA.Application.Contracts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EA.Application.Features.Levels.GetLevelById;

public class GetLevelByIdQueryHandler : IRequestHandler<GetLevelByIdQuery, LevelDto?>
{
    private readonly IApplicationDbContext _context;

    public GetLevelByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<LevelDto?> Handle(GetLevelByIdQuery request, CancellationToken cancellationToken)
    {
        return await _context.Levels
            .Where(l => l.Id == request.Id)
            .Select(l => new LevelDto(l.Id, l.Code, l.Name, l.Order, l.UnlockRequirement))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
