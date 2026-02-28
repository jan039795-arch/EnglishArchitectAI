namespace EA.Application.Features.Certificates;

public record CertificateDto(Guid Id, string LevelCode, DateTime IssuedAt, string? PDFBlobUrl, Guid VerificationCode);

public record CertificateVerifyDto(Guid Id, string LevelCode, DateTime IssuedAt, string Username);
