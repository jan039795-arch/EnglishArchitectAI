using EA.Application.Common;
using MediatR;

namespace EA.Application.Features.Certificates.IssueCertificate;

public record IssueCertificateCommand(string UserId, string LevelCode) : IRequest<Result<CertificateDto>>;
