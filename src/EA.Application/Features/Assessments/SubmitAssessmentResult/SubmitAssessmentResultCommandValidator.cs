using FluentValidation;

namespace EA.Application.Features.Assessments.SubmitAssessmentResult;

public class SubmitAssessmentResultCommandValidator : AbstractValidator<SubmitAssessmentResultCommand>
{
    public SubmitAssessmentResultCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.AssessmentId).NotEmpty();
        RuleFor(x => x.Score).GreaterThanOrEqualTo(0);
    }
}
