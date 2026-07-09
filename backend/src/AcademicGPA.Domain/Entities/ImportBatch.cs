using System;
using System.Collections.Generic;

namespace AcademicGPA.Domain.Entities;

/// <summary>
/// Represents a batch of courses imported via OCR, PDF, Excel, or Text.
/// </summary>
public class ImportBatch
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public Guid StudentProfileId { get; set; }
    public Guid SemesterId { get; set; }
    
    public string SourceType { get; set; } = string.Empty; // e.g., Excel, PDF, ImageOcr, Text
    
    public DateTime ImportedAt { get; set; } = DateTime.UtcNow;
    
    public int CourseCount { get; set; }
    
    public string? UniversityDetected { get; set; }
    public double? ConfidenceAverage { get; set; }
    
    public bool IsUndone { get; set; }
    
    public StudentProfile StudentProfile { get; set; } = null!;
    public Semester Semester { get; set; } = null!;
    
    public ICollection<ImportBatchCourse> ImportBatchCourses { get; set; } = new List<ImportBatchCourse>();
}
