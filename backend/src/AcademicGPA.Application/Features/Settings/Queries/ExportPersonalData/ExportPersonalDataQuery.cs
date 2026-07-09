using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AcademicGPA.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicGPA.Application.Features.Settings.Queries.ExportPersonalData;

public record ExportPersonalDataQuery(Guid StudentProfileId) : IRequest<string>;

public class ExportPersonalDataQueryHandler : IRequestHandler<ExportPersonalDataQuery, string>
{
    private readonly IUnitOfWork _unitOfWork;

    public ExportPersonalDataQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<string> Handle(ExportPersonalDataQuery request, CancellationToken cancellationToken)
    {
        var profile = await _unitOfWork.Students
            .GetQueryable()
            .Include(p => p.AcademicYears)
                .ThenInclude(ay => ay.Semesters)
                    .ThenInclude(s => s.Courses)
                        .ThenInclude(c => c.Score)
            .FirstOrDefaultAsync(p => p.Id == request.StudentProfileId, cancellationToken);

        if (profile == null) throw new Exception("Profile not found.");

        var data = new
        {
            Profile = new { profile.StudentCode, profile.UniversityName, profile.MajorName },
            AcademicYears = profile.AcademicYears.Select(ay => new
            {
                ay.YearName,
                Semesters = ay.Semesters.Select(s => new
                {
                    s.SemesterName,
                    Courses = s.Courses.Select(c => new
                    {
                        c.CourseCode,
                        c.CourseName,
                        c.Credits,
                        Score = c.Score != null ? new
                        {
                            c.Score.AttendanceScore,
                            c.Score.ContinuousScore,
                            c.Score.FinalExamScore,
                            c.Score.CourseScore,
                            c.Score.LetterGrade,
                            c.Score.Gpa4Value
                        } : null
                    })
                })
            })
        };

        return System.Text.Json.JsonSerializer.Serialize(data, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
    }
}
