using MediatR;

namespace EA.Application.Features.Progress.GetUserProgress;

public record GetUserProgressQuery(string UserId) : IRequest<List<UserProgressDto>>;
