using System.Collections.Generic;
using AcademicGPA.Domain.Entities;
using AcademicGPA.Domain.ValueObjects;

namespace AcademicGPA.Application.Common.Interfaces;

/// <summary>
/// Core service contract for course and overall GPA calculations.
/// </summary>
public interface IGpaCalculator
{
    /// <summary>
    /// Calculates the weighted course score based on Attendance (10%), Continuous (30%), and Final (60%) component scores.
    /// Applies nearest 0.5 rounding to components first, and standard 1 decimal place rounding to the result.
    /// Returns null if any component is missing.
    /// </summary>
    decimal? CalculateCourseScore(decimal? attendance, decimal? continuous, decimal? final);

    /// <summary>
    /// Maps a raw course score to a GradeResult (e.g. 8.5 -> A, 3.7).
    /// </summary>
    GradeResult MapToGradeResult(decimal courseScore, AcademicGPA.Domain.Entities.GradeScale? gradeScale = null);

    /// <summary>
    /// Calculates the weighted GPA on 10-scale for a collection of courses.
    /// </summary>
    decimal? CalculateGpa10(IEnumerable<Course> courses);

    /// <summary>
    /// Calculates the weighted GPA on 4-scale for a collection of courses.
    /// </summary>
    decimal? CalculateGpa4(IEnumerable<Course> courses);

    /// <summary>
    /// Filters out original attempts if retakes are present, keeping only the highest graded attempt.
    /// </summary>
    IEnumerable<Course> FilterBestAttempts(IEnumerable<Course> courses);
}
