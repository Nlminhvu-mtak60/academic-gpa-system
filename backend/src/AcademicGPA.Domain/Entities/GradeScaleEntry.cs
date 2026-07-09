using System;

namespace AcademicGPA.Domain.Entities;

/// <summary>
/// Represents a single grade mapping entry within a GradeScale.
/// </summary>
public class GradeScaleEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public Guid GradeScaleId { get; set; }
    public GradeScale GradeScale { get; set; } = null!;
    
    /// <summary>
    /// e.g., "A+", "B"
    /// </summary>
    public string LetterGrade { get; set; } = string.Empty;
    
    /// <summary>
    /// Minimum course score required for this grade.
    /// </summary>
    public decimal MinScore { get; set; }
    
    /// <summary>
    /// Maximum course score for this grade.
    /// </summary>
    public decimal MaxScore { get; set; }
    
    /// <summary>
    /// GPA 4 scale equivalent (e.g., 4.0, 3.0).
    /// </summary>
    public decimal Gpa4Value { get; set; }
    
    /// <summary>
    /// Academic Classification (e.g., "Excellent", "Good").
    /// </summary>
    public string Classification { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether this grade represents a passing grade.
    /// </summary>
    public bool IsPass { get; set; }
    
    /// <summary>
    /// Ordering for display.
    /// </summary>
    public int SortOrder { get; set; }
}
