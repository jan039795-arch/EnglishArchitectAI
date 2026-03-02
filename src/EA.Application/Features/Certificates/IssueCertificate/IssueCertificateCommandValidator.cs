using FluentValidation;

namespace EA.Application.Features.Certificates.IssueCertificate;

public class IssueCertificateCommandValidator : AbstractValidator<IssueCertificateCommand>
{
    public IssueCertificateCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.LevelCode).NotEmpty();
    }
}
