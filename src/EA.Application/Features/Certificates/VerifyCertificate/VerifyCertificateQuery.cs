using MediatR;

namespace EA.Application.Features.Certificates.VerifyCertificate;

public record VerifyCertificateQuery(Guid VerificationCode) : IRequest<CertificateVerifyDto?>;
