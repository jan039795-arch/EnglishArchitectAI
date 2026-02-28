using EA.Application.Common;
using MediatR;

namespace EA.Application.Features.Auth.Commands.Register;

public record RegisterUserCommand(
    string Email,
    string Username,
    string Password,
    string ConfirmPassword) : IRequest<Result<string>>;
