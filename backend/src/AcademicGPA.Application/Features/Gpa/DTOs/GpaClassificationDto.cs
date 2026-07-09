using System;

namespace AcademicGPA.Application.Features.Gpa.DTOs;

public record GpaClassificationDto(
    decimal? CumulativeGpa10,
    string ClassificationEn,
    string ClassificationVn,
    decimal MinimumThresholdGpa10
);
