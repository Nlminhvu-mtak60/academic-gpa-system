using System;
using System.Collections.Generic;
using System.Linq;
using AcademicGPA.Application.Common.Interfaces;
using AcademicGPA.Domain.Entities;
using AcademicGPA.Domain.ValueObjects;

namespace AcademicGPA.Infrastructure.Services;

/// <summary>
/// Core implementation of course grade conversions, rounding rules, and GPA aggregation logic.
/// </summary>
public class GpaCalculator : IGpaCalculator
{
    public decimal? CalculateCourseScore(decimal? attendance, decimal? continuous, decimal? final)
    {
        if (attendance == null || continuous == null || final == null)
        {
            return null;
        }

        var roundedAttendance = RoundToNearestHalf(attendance.Value);
        var roundedContinuous = RoundToNearestHalf(continuous.Value);
        var roundedFinal = RoundToNearestHalf(final.Value);

        var courseScore = roundedAttendance * 0.1m + roundedContinuous * 0.3m + roundedFinal * 0.6m;
        
        return Math.Round(courseScore, 1, MidpointRounding.AwayFromZero);
    }

    public GradeResult MapToGradeResult(decimal courseScore, GradeScale? gradeScale = null)
    {
        if (gradeScale != null && gradeScale.GradeScaleEntries.Any())
        {
            var entry = gradeScale.GradeScaleEntries
                .OrderByDescending(e => e.MinScore)
                .FirstOrDefault(e => courseScore >= e.MinScore && courseScore <= e.MaxScore);
                
            if (entry != null)
            {
                return new GradeResult(entry.LetterGrade, entry.Gpa4Value, entry.Classification, entry.IsPass);
            }
        }

        // Fallback to default logic
        if (courseScore >= 9.0m) return new GradeResult("A+", 4.0m, "Outstanding", true);
        if (courseScore >= 8.5m) return new GradeResult("A", 3.7m, "Excellent", true);
        if (courseScore >= 8.0m) return new GradeResult("B+", 3.5m, "Very Good", true);
        if (courseScore >= 7.0m) return new GradeResult("B", 3.0m, "Good", true);
        if (courseScore >= 6.5m) return new GradeResult("C+", 2.5m, "Average Good", true);
        if (courseScore >= 5.5m) return new GradeResult("C", 2.0m, "Average", true);
        if (courseScore >= 5.0m) return new GradeResult("D+", 1.5m, "Average", true);
        if (courseScore >= 4.0m) return new GradeResult("D", 1.0m, "Weak", true);

        return new GradeResult("F", 0.0m, "Poor", false);
    }

    public decimal? CalculateGpa10(IEnumerable<Course> courses)
    {
        var gradedCourses = courses
            .Where(c => c.Score != null && c.Score.CourseScore.HasValue)
            .ToList();

        if (gradedCourses.Count == 0)
        {
            return null;
        }

        decimal weightedSum = 0;
        int totalCredits = 0;

        foreach (var course in gradedCourses)
        {
            weightedSum += course.Score!.CourseScore!.Value * course.Credits;
            totalCredits += course.Credits;
        }

        if (totalCredits == 0)
        {
            return null;
        }

        return Math.Round(weightedSum / totalCredits, 2, MidpointRounding.AwayFromZero);
    }

    public decimal? CalculateGpa4(IEnumerable<Course> courses)
    {
        var gradedCourses = courses
            .Where(c => c.Score != null && c.Score.Gpa4Value.HasValue)
            .ToList();

        if (gradedCourses.Count == 0)
        {
            return null;
        }

        decimal weightedSum = 0;
        int totalCredits = 0;

        foreach (var course in gradedCourses)
        {
            weightedSum += course.Score!.Gpa4Value!.Value * course.Credits;
            totalCredits += course.Credits;
        }

        if (totalCredits == 0)
        {
            return null;
        }

        return Math.Round(weightedSum / totalCredits, 2, MidpointRounding.AwayFromZero);
    }

    public IEnumerable<Course> FilterBestAttempts(IEnumerable<Course> courses)
    {
        var result = new List<Course>();
        var grouped = courses.GroupBy(c => c.CourseCode.ToUpperInvariant());

        foreach (var group in grouped)
        {
            var best = group
                .OrderByDescending(c => c.Score?.CourseScore ?? -1m)
                .ThenByDescending(c => c.CreatedAt)
                .First();

            result.Add(best);
        }

        return result;
    }

    private decimal RoundToNearestHalf(decimal value)
    {
        return Math.Round(value * 2, MidpointRounding.AwayFromZero) / 2;
    }
}
