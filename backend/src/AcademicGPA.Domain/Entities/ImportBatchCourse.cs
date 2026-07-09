using System;

namespace AcademicGPA.Domain.Entities;

/// <summary>
/// Join entity between ImportBatch and Course.
/// </summary>
public class ImportBatchCourse
{
    public Guid ImportBatchId { get; set; }
    public ImportBatch ImportBatch { get; set; } = null!;
    
    public Guid CourseId { get; set; }
    public Course Course { get; set; } = null!;
}
