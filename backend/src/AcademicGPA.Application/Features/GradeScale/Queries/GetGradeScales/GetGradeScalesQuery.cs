using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AcademicGPA.Application.Common.Interfaces;
using AcademicGPA.Application.Features.GradeScale.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicGPA.Application.Features.GradeScale.Queries.GetGradeScales;

public record GetGradeScalesQuery() : IRequest<List<GradeScaleDto>>;

public class GetGradeScalesQueryHandler : IRequestHandler<GetGradeScalesQuery, List<GradeScaleDto>>
{
    private readonly IApplicationDbContext _context;

    public GetGradeScalesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<GradeScaleDto>> Handle(GetGradeScalesQuery request, CancellationToken cancellationToken)
    {
        var scales = await _context.GradeScales
            .Include(gs => gs.GradeScaleEntries)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return scales.Select(gs => new GradeScaleDto(
            gs.Id,
            gs.Name,
            gs.IsDefault,
            gs.GradeScaleEntries
                .OrderBy(e => e.SortOrder)
                .Select(e => new GradeScaleEntryDto(
                    e.Id,
                    e.LetterGrade,
                    e.MinScore,
                    e.MaxScore,
                    e.Gpa4Value,
                    e.Classification,
                    e.IsPass,
                    e.SortOrder
                )).ToList()
        )).ToList();
    }
}
