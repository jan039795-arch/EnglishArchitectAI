using EA.Application.Common;
using EA.Application.Contracts;
using EA.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EA.Application.Features.Certificates.IssueCertificate;

public class IssueCertificateCommandHandler : IRequestHandler<IssueCertificateCommand, Result<CertificateDto>>
{
    private readonly IApplicationDbContext _context;

    public IssueCertificateCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<CertificateDto>> Handle(IssueCertificateCommand request, CancellationToken cancellationToken)
    {
        // Load the level
        var level = await _context.Levels
            .Include(l => l.Modules)
            .ThenInclude(m => m.Lessons)
            .FirstOrDefaultAsync(l => l.Code == request.LevelCode, cancellationToken);

        if (level is null)
            return Result<CertificateDto>.Failure($"Level with code {request.LevelCode} not found.");

        // Get all lesson IDs for this level
        var requiredLessonIds = level.Modules
            .SelectMany(m => m.Lessons)
            .Select(l => l.Id)
            .ToList();

        if (requiredLessonIds.Count == 0)
            return Result<CertificateDto>.Failure("No lessons found for this level.");

        // Check how many lessons the user has completed
        var completedCount = await _context.UserProgresses
            .Where(p => p.UserId == request.UserId && requiredLessonIds.Contains(p.LessonId))
            .CountAsync(cancellationToken);

        if (completedCount < requiredLessonIds.Count)
            return Result<CertificateDto>.Failure($"You must complete all lessons to receive a certificate. Progress: {completedCount}/{requiredLessonIds.Count}");

        // Check if certificate already exists (idempotency)
        var existingCert = await _context.Certificates
            .FirstOrDefaultAsync(c => c.UserId == request.UserId && c.LevelCode == request.LevelCode, cancellationToken);

        if (existingCert is not null)
            return Result<CertificateDto>.Success(new CertificateDto(existingCert.Id, existingCert.LevelCode, existingCert.IssuedAt, existingCert.PDFBlobUrl, existingCert.VerificationCode));

        // Create new certificate
        var certificate = new Certificate
        {
            UserId = request.UserId,
            LevelCode = request.LevelCode,
            IssuedAt = DateTime.UtcNow,
            VerificationCode = Guid.NewGuid()
        };

        _context.Certificates.Add(certificate);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<CertificateDto>.Success(new CertificateDto(certificate.Id, certificate.LevelCode, certificate.IssuedAt, certificate.PDFBlobUrl, certificate.VerificationCode));
    }
}
