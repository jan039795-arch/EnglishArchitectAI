using EA.Application.Common;
using MediatR;

namespace EA.Application.Features.Auth.Commands.Login;

public record LoginUserCommand(string Email, string Password) : IRequest<Result<string>>;
