using EA.Application.Contracts;
using EA.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EA.Application.Features.LessonComments.AddLessonComment;

public class AddLessonCommentCommandHandler : IRequestHandler<AddLessonCommentCommand, LessonCommentDto>
{
    private readonly IApplicationDbContext _context;

    public AddLessonCommentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<LessonCommentDto> Handle(AddLessonCommentCommand request, CancellationToken cancellationToken)
    {
        // Create the comment
        var comment = new LessonComment
        {
            LessonId = request.LessonId,
            UserId = request.UserId,
            Body = request.Body
        };

        _context.LessonComments.Add(comment);
        await _context.SaveChangesAsync(cancellationToken);

        // Fetch the user to get the username
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        return new LessonCommentDto(
            comment.Id,
            comment.Body,
            user?.UserName ?? "Anonymous",
            comment.CreatedAt);
    }
}
