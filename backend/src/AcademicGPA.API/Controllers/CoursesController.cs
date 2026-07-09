using System;
using System.Threading.Tasks;
using AcademicGPA.Application.Features.Courses.Commands.CreateCourse;
using AcademicGPA.Application.Features.Courses.Commands.DeleteCourse;
using AcademicGPA.Application.Features.Courses.Commands.UpdateCourse;
using AcademicGPA.Application.Features.Courses.Queries.GetCourses;
using AcademicGPA.Application.Features.Courses.Queries.GetEligibleOriginalCourses;
using AcademicGPA.Application.Features.Scores.Commands.UpdateScores;
using AcademicGPA.Application.Features.Scores.Queries.GetScoreAuditLogs;
using AcademicGPA.Application.Features.Scores.Queries.GetScores;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AcademicGPA.API.Controllers;

[ApiController]
[Route("api/v1")]
[Authorize]
public class CoursesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CoursesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("semesters/{semesterId:guid}/courses")]
    public async Task<IActionResult> GetCourses(Guid semesterId)
    {
        var query = new GetCoursesQuery(semesterId);
        var result = await _mediator.Send(query);
        return Ok(new ApiResponse(true, result));
    }

    [HttpPost("semesters/{semesterId:guid}/courses")]
    public async Task<IActionResult> Create(Guid semesterId, [FromBody] CreateCourseRequest request)
    {
        var command = new CreateCourseCommand(
            semesterId,
            request.CourseCode,
            request.CourseName,
            request.Credits,
            request.IsRetake,
            request.OriginalCourseId
        );
        var result = await _mediator.Send(command);

        var responseData = new
        {
            id = result.Id,
            courseCode = result.CourseCode,
            courseName = result.CourseName,
            credits = result.Credits
        };

        return StatusCode(201, new ApiResponse(true, responseData, null, "Course added successfully."));
    }

    [HttpPut("courses/{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCourseRequest request)
    {
        var command = new UpdateCourseCommand(
            id,
            request.CourseCode,
            request.CourseName,
            request.Credits,
            request.IsRetake,
            request.OriginalCourseId
        );
        await _mediator.Send(command);
        return Ok(new ApiResponse(true, null, null, "Course updated successfully."));
    }

    [HttpDelete("courses/{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var command = new DeleteCourseCommand(id);
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpGet("courses/eligible-originals")]
    public async Task<IActionResult> GetEligibleOriginalCourses([FromQuery] string courseCode)
    {
        var query = new GetEligibleOriginalCoursesQuery(courseCode);
        var result = await _mediator.Send(query);
        return Ok(new ApiResponse(true, result));
    }

    [HttpPut("courses/{courseId:guid}/scores")]
    public async Task<IActionResult> UpdateScores(Guid courseId, [FromBody] UpdateScoresRequest request)
    {
        var command = new UpdateScoresCommand(
            courseId,
            request.AttendanceScore,
            request.ContinuousScore,
            request.FinalExamScore
        );
        var result = await _mediator.Send(command);

        var responseData = new
        {
            courseId = courseId,
            attendanceScore = result.AttendanceScore,
            continuousScore = result.ContinuousScore,
            finalExamScore = result.FinalExamScore,
            courseScore = result.CourseScore,
            letterGrade = result.LetterGrade,
            gpa4Value = result.Gpa4Value
        };

        return Ok(new ApiResponse(true, responseData, null, "Scores updated and grades recalculated successfully."));
    }

    [HttpGet("courses/{courseId:guid}/scores")]
    public async Task<IActionResult> GetScores(Guid courseId)
    {
        var query = new GetScoresQuery(courseId);
        var result = await _mediator.Send(query);

        var responseData = new
        {
            attendanceScore = result.AttendanceScore,
            continuousScore = result.ContinuousScore,
            finalExamScore = result.FinalExamScore,
            courseScore = result.CourseScore,
            letterGrade = result.LetterGrade,
            gpa4Value = result.Gpa4Value,
            calculatedAt = result.CalculatedAt
        };

        return Ok(new ApiResponse(true, responseData));
    }

    [HttpGet("courses/{courseId:guid}/scores/audit")]
    public async Task<IActionResult> GetScoreAudit(Guid courseId)
    {
        var query = new GetScoreAuditLogsQuery(courseId);
        var result = await _mediator.Send(query);
        return Ok(new ApiResponse(true, result));
    }

    private record ApiResponse(bool Success, object? Data, object? Errors = null, string? Message = null)
    {
        public DateTime Timestamp { get; } = DateTime.UtcNow;
    }

    public record CreateCourseRequest(
        string CourseCode,
        string CourseName,
        int Credits,
        bool IsRetake,
        Guid? OriginalCourseId
    );

    public record UpdateCourseRequest(
        string CourseCode,
        string CourseName,
        int Credits,
        bool IsRetake,
        Guid? OriginalCourseId
    );

    public record UpdateScoresRequest(
        decimal? AttendanceScore,
        decimal? ContinuousScore,
        decimal? FinalExamScore
    );
}
