using MediatR;

namespace EA.Application.Features.Certificates.GetUserCertificates;

public record GetUserCertificatesQuery(string UserId) : IRequest<List<CertificateDto>>;
