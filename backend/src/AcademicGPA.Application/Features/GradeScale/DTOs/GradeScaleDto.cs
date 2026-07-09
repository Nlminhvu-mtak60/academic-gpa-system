using System;
using System.Collections.Generic;

namespace AcademicGPA.Application.Features.GradeScale.DTOs;

public record GradeScaleEntryDto(
    Guid Id,
    string LetterGrade,
    decimal MinScore,
    decimal MaxScore,
    decimal Gpa4Value,
    string Classification,
    bool IsPass,
    int SortOrder
);

public record GradeScaleDto(
    Guid Id,
    string Name,
    bool IsDefault,
    List<GradeScaleEntryDto> Entries
);
