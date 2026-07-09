using System;
using System.Collections.Generic;

namespace AcademicGPA.Domain.Entities;

/// <summary>
/// Represents a university-specific grading scale.
/// </summary>
public class GradeScale
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// Name of the grading scale (e.g., "BachKhoa Scale", "Default").
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether this is the system default grading scale.
    /// </summary>
    public bool IsDefault { get; set; }
    
    /// <summary>
    /// Collection of grade scale entries.
    /// </summary>
    public ICollection<GradeScaleEntry> GradeScaleEntries { get; set; } = new List<GradeScaleEntry>();
}
