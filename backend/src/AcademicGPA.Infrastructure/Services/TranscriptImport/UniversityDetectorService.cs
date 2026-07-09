using System;
using System.Collections.Generic;
using System.Linq;
using AcademicGPA.Application.Features.TranscriptImport.Interfaces;

namespace AcademicGPA.Infrastructure.Services.TranscriptImport;

public class UniversityDetectorService : IUniversityDetector
{
    private static readonly Dictionary<string, List<string>> _universityKeywords = new()
    {
        { "MilitaryTechnicalAcademy", new List<string> { "Học viện Kỹ thuật Quân sự", "MTA", "Kỹ thuật Quân sự" } },
        { "BachKhoa", new List<string> { "Đại học Bách khoa", "HUST", "Đại học Bách Khoa Hà Nội" } },
        { "PTIT", new List<string> { "Học viện Công nghệ Bưu chính Viễn thông", "PTIT" } },
        { "NEU", new List<string> { "Đại học Kinh tế Quốc dân", "NEU" } },
        { "VNU", new List<string> { "Đại học Quốc gia", "VNU" } }
    };

    public string DetectUniversity(string transcriptText)
    {
        if (string.IsNullOrWhiteSpace(transcriptText))
            return "Generic";

        var lowerText = transcriptText.ToLowerInvariant();

        foreach (var (uni, keywords) in _universityKeywords)
        {
            if (keywords.Any(k => lowerText.Contains(k.ToLowerInvariant())))
            {
                return uni;
            }
        }

        return "Generic";
    }

    public string DetectUniversityFromHeaders(IEnumerable<string> headers)
    {
        var headerList = headers.Select(h => h.Trim().ToLowerInvariant()).ToList();
        
        // Example: If headers have specific CC/TX/CK, we might guess.
        // But mostly, we'll return Generic unless we have very specific header combos.
        if (headerList.Contains("điểm qt") || headerList.Contains("điểm qttp"))
        {
            // Just examples. In reality, it might need more sophisticated mapping.
        }

        return "Generic"; // Default fallback
    }
}
