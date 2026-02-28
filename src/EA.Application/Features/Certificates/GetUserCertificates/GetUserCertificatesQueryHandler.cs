using EA.Application.Contracts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EA.Application.Features.Certificates.GetUserCertificates;

public class GetUserCertificatesQueryHandler : IRequestHandler<GetUserCertificatesQuery, List<CertificateDto>>
{
    private readonly IApplicationDbContext _context;

    public GetUserCertificatesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<CertificateDto>> Handle(GetUserCertificatesQuery request, CancellationToken cancellationToken)
    {
        return await _context.Certificates
            .Where(c => c.UserId == request.UserId)
            .Select(c => new CertificateDto(c.Id, c.LevelCode, c.IssuedAt, c.PDFBlobUrl, c.VerificationCode))
            .ToListAsync(cancellationToken);
    }
}
