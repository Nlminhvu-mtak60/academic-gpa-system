using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AcademicGPA.Application.Features.TranscriptImport.DTOs;
using AcademicGPA.Application.Features.TranscriptImport.Interfaces;

namespace AcademicGPA.Infrastructure.Services.TranscriptImport.Parsers;

public class BachKhoaParser : IUniversityParser
{
    public string SupportedUniversity => "BachKhoa";

    public List<ImportedCourseDto> ParseLines(IEnumerable<string> lines, out List<string> warnings)
    {
        var courses = new List<ImportedCourseDto>();
        warnings = new List<string>();

        // Bach Khoa often uses tab or space separation:
        // Mã HP | Tên HP | TC | KT | Thi | Tổng kết
        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            if (line.Contains("Mã HP", StringComparison.OrdinalIgnoreCase)) continue;

            var parts = line.Split('\t', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 5)
            {
                // We have some tabs
                string courseName = parts[1].Trim();
                if (int.TryParse(parts[2], out int credits))
                {
                    decimal? finalScore = null;
                    if (parts.Length > 5 && decimal.TryParse(parts[5].Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal s))
                    {
                        finalScore = s;
                    }

                    var components = new Dictionary<string, decimal>();
                    if (decimal.TryParse(parts[3].Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal ktScore))
                        components["KT"] = ktScore;
                    if (decimal.TryParse(parts[4].Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal thiScore))
                        components["Thi"] = thiScore;

                    courses.Add(new ImportedCourseDto(courseName, credits, finalScore, components, 0.9));
                }
            }
            else
            {
                warnings.Add($"Could not parse BachKhoa line: {line}");
            }
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

            // Find columns by keys
            foreach (var kvp in row)
            {
                var key = kvp.Key.ToLowerInvariant();
                var val = kvp.Value.Replace(",", ".");

                if (key.Contains("tên hp") || key.Contains("môn học"))
                    courseName = kvp.Value;
                else if (key == "tc" || key.Contains("tín chỉ"))
                {
                    if (int.TryParse(val, out int c)) credits = c;
                }
                else if (key == "tk" || key.Contains("tổng kết"))
                {
                    if (decimal.TryParse(val, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal s)) finalScore = s;
                }
                else if (key == "kt" || key == "quá trình")
                {
                    if (decimal.TryParse(val, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal cs)) components["KT"] = cs;
                }
                else if (key == "thi" || key == "cuối kỳ")
                {
                    if (decimal.TryParse(val, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal cs)) components["Thi"] = cs;
                }
            }

            if (!string.IsNullOrWhiteSpace(courseName) && credits > 0)
            {
                courses.Add(new ImportedCourseDto(courseName, credits, finalScore, components, 1.0));
            }
            else
            {
                warnings.Add("Row skipped: missing course name or credits.");
            }
        }

        return courses;
    }
}
