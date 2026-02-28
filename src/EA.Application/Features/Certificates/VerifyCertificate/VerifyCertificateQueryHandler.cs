using EA.Application.Contracts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EA.Application.Features.Certificates.VerifyCertificate;

public class VerifyCertificateQueryHandler : IRequestHandler<VerifyCertificateQuery, CertificateVerifyDto?>
{
    private readonly IApplicationDbContext _context;

    public VerifyCertificateQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CertificateVerifyDto?> Handle(VerifyCertificateQuery request, CancellationToken cancellationToken)
    {
        return await _context.Certificates
            .Where(c => c.VerificationCode == request.VerificationCode)
            .Include(c => c.User)
            .Select(c => new CertificateVerifyDto(c.Id, c.LevelCode, c.IssuedAt, c.User.UserName!))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
