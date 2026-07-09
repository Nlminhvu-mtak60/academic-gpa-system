using AcademicGPA.Application.Common.Exceptions;
using AcademicGPA.Application.Common.Interfaces;
using AcademicGPA.Application.Features.Students.DTOs;
using AcademicGPA.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicGPA.Application.Features.Students.Queries.GetProfile;

public record GetStudentProfileQuery : IRequest<StudentProfileDetailsDto>;

public class GetStudentProfileQueryHandler : IRequestHandler<GetStudentProfileQuery, StudentProfileDetailsDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetStudentProfileQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<StudentProfileDetailsDto> Handle(GetStudentProfileQuery request, CancellationToken cancellationToken)
    {
        // 1. Get Current User ID
        var userIdStr = _currentUserService.UserId;
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        // 2. Fetch User including StudentProfile relation
        var user = await _context.Users
            .Include(u => u.StudentProfile)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
        {
            throw new NotFoundException("User", userId);
        }

        // 3. Map to DTOs
        StudentProfileSubDto? profileDto = null;
        if (user.StudentProfile != null)
        {
            profileDto = new StudentProfileSubDto(
                user.StudentProfile.StudentCode,
                user.StudentProfile.UniversityName,
                user.StudentProfile.MajorName,
                user.StudentProfile.EnrollmentYear,
                user.StudentProfile.TotalRequiredCredits
            );
        }

        return new StudentProfileDetailsDto(
            user.Email,
            user.FirstName,
            user.LastName,
            user.AvatarUrl,
            user.PreferredLanguage,
            user.PreferredTheme,
            profileDto
        );
    }
}
