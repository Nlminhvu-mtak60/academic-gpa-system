using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using AcademicGPA.Application.Features.TranscriptImport.DTOs;
using AcademicGPA.Application.Features.TranscriptImport.Interfaces;

namespace AcademicGPA.Infrastructure.Services.TranscriptImport.Parsers;

public class GenericParser : IUniversityParser
{
    public string SupportedUniversity => "Generic";

    public List<ImportedCourseDto> ParseLines(IEnumerable<string> lines, out List<string> warnings)
    {
        var courses = new List<ImportedCourseDto>();
        warnings = new List<string>();

        // Very basic generic parser: expects "Course Name [Tab] Credits [Tab] Final Score"
        // Or space separated with some heuristics.
        
        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            // Try split by tabs first
            var parts = line.Split('\t', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 3)
            {
                // Fallback to spaces if not enough tabs
                parts = Regex.Split(line.Trim(), @"\s{2,}"); // split by 2 or more spaces
                if (parts.Length < 3)
                {
                    warnings.Add($"Could not parse line: {line}");
                    continue;
                }
            }

            // Assuming: Course Name | Credits | Final Score
            // We need to find which part is credits (integer) and which is score (decimal).
            string courseName = parts[0];
            int credits = 0;
            decimal? finalScore = null;
            double confidence = 1.0;

            for (int i = 1; i < parts.Length; i++)
            {
                var part = parts[i].Replace(",", ".");
                if (int.TryParse(part, out int c) && c > 0 && c <= 10 && credits == 0)
                {
                    credits = c;
                }
                else if (decimal.TryParse(part, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal s) && s >= 0 && s <= 10)
                {
                    finalScore = s;
                }
                else
                {
                    // Append to course name if it's text
                    if (credits == 0 && finalScore == null)
                    {
                        courseName += " " + parts[i];
                    }
                }
            }

            if (credits == 0 || finalScore == null)
            {
                warnings.Add($"Could not determine credits or score for line: {line}");
                confidence = 0.5; // low confidence
                // Still add it so user can fix it manually
            }

            courses.Add(new ImportedCourseDto(
                CourseName: courseName.Trim(),
                Credits: credits,
                FinalScore: finalScore,
                ComponentScores: new Dictionary<string, decimal>(),
                Confidence: confidence
            ));
        }

        return courses;
    }

    public List<ImportedCourseDto> ParseRows(IEnumerable<Dictionary<string, string>> rows, out List<string> warnings)
    {
        var courses = new List<ImportedCourseDto>();
        warnings = new List<string>();

        foreach (var row in rows)
        {
            string courseName = string.Empty;
            int credits = 0;
            decimal? finalScore = null;
            var components = new Dictionary<string, decimal>();

            foreach (var kvp in row)
            {
                var key = kvp.Key.ToLowerInvariant();
                var val = kvp.Value.Replace(",", ".");

                if (key.Contains("tên môn") || key.Contains("course") || key.Contains("môn học"))
                {
                    courseName = kvp.Value;
                }
                else if (key.Contains("tín chỉ") || key.Contains("tc") || key.Contains("credit"))
                {
                    if (int.TryParse(val, out int c)) credits = c;
                }
                else if (key.Contains("điểm tổng kết") || key.Contains("điểm học phần") || key.Contains("tổng kết") || key.Contains("final") || key.Contains("tbhp"))
                {
                    if (decimal.TryParse(val, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal s)) finalScore = s;
                }
                else if (decimal.TryParse(val, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal cScore) && cScore >= 0 && cScore <= 10)
                {
                    // Might be a component score
                    components[kvp.Key] = cScore;
                }
            }

            if (string.IsNullOrWhiteSpace(courseName))
            {
                warnings.Add("Row skipped: Missing course name.");
                continue;
            }

            courses.Add(new ImportedCourseDto(
                CourseName: courseName,
                Credits: credits,
                FinalScore: finalScore,
                ComponentScores: components,
                Confidence: credits > 0 ? 1.0 : 0.7
            ));
        }

        return courses;
    }
}
