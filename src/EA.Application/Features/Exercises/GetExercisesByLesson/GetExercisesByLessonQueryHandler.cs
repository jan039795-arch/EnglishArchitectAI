using EA.Application.Contracts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EA.Application.Features.Exercises.GetExercisesByLesson;

public class GetExercisesByLessonQueryHandler : IRequestHandler<GetExercisesByLessonQuery, List<ExerciseDto>>
{
    private readonly IApplicationDbContext _context;

    public GetExercisesByLessonQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ExerciseDto>> Handle(GetExercisesByLessonQuery request, CancellationToken cancellationToken)
    {
        return await _context.Exercises
            .Where(e => e.LessonId == request.LessonId)
            .Include(e => e.Options)
            .Select(e => new ExerciseDto(
                e.Id,
                e.LessonId,
                e.Type.ToString(),
                e.Prompt,
                e.Difficulty,
                e.Tags,
                e.Source.ToString(),
                e.Options.Select(o => new ExerciseOptionDto(o.Id, o.Text, o.Explanation)).ToList()))
            .ToListAsync(cancellationToken);
    }
}
