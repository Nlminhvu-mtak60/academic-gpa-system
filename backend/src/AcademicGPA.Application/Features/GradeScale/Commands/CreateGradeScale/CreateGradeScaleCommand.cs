using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AcademicGPA.Application.Common.Interfaces;
using AcademicGPA.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicGPA.Application.Features.GradeScale.Commands.CreateGradeScale;

public record CreateGradeScaleEntryCommand(
    string LetterGrade,
    decimal MinScore,
    decimal MaxScore,
    decimal Gpa4Value,
    string Classification,
    bool IsPass,
    int SortOrder
);

public record CreateGradeScaleCommand(
    string Name,
    bool IsDefault,
    List<CreateGradeScaleEntryCommand> Entries
) : IRequest<System.Guid>;

public class CreateGradeScaleCommandHandler : IRequestHandler<CreateGradeScaleCommand, System.Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateGradeScaleCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<System.Guid> Handle(CreateGradeScaleCommand request, CancellationToken cancellationToken)
    {
        if (request.IsDefault)
        {
            var defaults = await _context.GradeScales.Where(g => g.IsDefault).ToListAsync(cancellationToken);
            foreach (var d in defaults)
            {
                d.IsDefault = false;
            }
        }

        var scale = new AcademicGPA.Domain.Entities.GradeScale
        {
            Name = request.Name,
            IsDefault = request.IsDefault
        };

        foreach (var entry in request.Entries)
        {
            scale.GradeScaleEntries.Add(new GradeScaleEntry
            {
                LetterGrade = entry.LetterGrade,
                MinScore = entry.MinScore,
                MaxScore = entry.MaxScore,
                Gpa4Value = entry.Gpa4Value,
                Classification = entry.Classification,
                IsPass = entry.IsPass,
                SortOrder = entry.SortOrder
            });
        }

        await _context.GradeScales.AddAsync(scale, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return scale.Id;
    }
}
