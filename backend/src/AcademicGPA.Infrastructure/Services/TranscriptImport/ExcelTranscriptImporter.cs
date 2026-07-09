using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AcademicGPA.Application.Features.TranscriptImport.DTOs;
using AcademicGPA.Application.Features.TranscriptImport.Interfaces;
using ClosedXML.Excel;
using CsvHelper;
using CsvHelper.Configuration;

namespace AcademicGPA.Infrastructure.Services.TranscriptImport;

public class ExcelTranscriptImporter : ITranscriptImporter
{
    private readonly IEnumerable<IUniversityParser> _parsers;
    private readonly IUniversityDetector _detector;

    public ExcelTranscriptImporter(IEnumerable<IUniversityParser> parsers, IUniversityDetector detector)
    {
        _parsers = parsers;
        _detector = detector;
    }

    public async Task<TranscriptImportResult> ImportAsync(Stream input, string? fileName = null, CancellationToken cancellationToken = default)
    {
        List<Dictionary<string, string>> rows;
        
        if (fileName != null && fileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
        {
            rows = await ParseCsvAsync(input);
        }
        else
        {
            rows = ParseXlsx(input);
        }

        if (rows.Count == 0)
        {
            return new TranscriptImportResult(new List<ImportedCourseDto>(), "Unknown", "Excel", new List<string> { "No data found in file." });
        }

        // Detect university from headers
        var headers = rows.First().Keys;
        var university = _detector.DetectUniversityFromHeaders(headers);

        var parser = _parsers.FirstOrDefault(p => p.SupportedUniversity == university) 
            ?? _parsers.First(p => p.SupportedUniversity == "Generic");

        var courses = parser.ParseRows(rows, out var warnings);

        return new TranscriptImportResult(courses, university, "Excel", warnings);
    }

    private async Task<List<Dictionary<string, string>>> ParseCsvAsync(Stream input)
    {
        var result = new List<Dictionary<string, string>>();
        using var reader = new StreamReader(input, leaveOpen: true);
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            MissingFieldFound = null,
            HeaderValidated = null
        };
        
        using var csv = new CsvReader(reader, config);
        await csv.ReadAsync();
        csv.ReadHeader();
        var headers = csv.HeaderRecord;

        if (headers == null) return result;

        while (await csv.ReadAsync())
        {
            var row = new Dictionary<string, string>();
            foreach (var header in headers)
            {
                row[header] = csv.GetField(header) ?? string.Empty;
            }
            result.Add(row);
        }

        return result;
    }

    private List<Dictionary<string, string>> ParseXlsx(Stream input)
    {
        var result = new List<Dictionary<string, string>>();
        using var workbook = new XLWorkbook(input);
        var worksheet = workbook.Worksheets.FirstOrDefault();
        if (worksheet == null) return result;

        // Find header row. Assuming it's the first row with at least 3 non-empty cells
        var rows = worksheet.RowsUsed();
        IXLRow? headerRow = null;
        
        foreach (var row in rows)
        {
            if (row.CellsUsed().Count() >= 3)
            {
                headerRow = row;
                break;
            }
        }

        if (headerRow == null) return result;

        var headers = new Dictionary<int, string>();
        foreach (var cell in headerRow.CellsUsed())
        {
            headers[cell.Address.ColumnNumber] = cell.GetString();
        }

        var dataRows = rows.Where(r => r.RowNumber() > headerRow.RowNumber());
        foreach (var row in dataRows)
        {
            var rowDict = new Dictionary<string, string>();
            bool hasData = false;
            foreach (var kvp in headers)
            {
                var cellValue = row.Cell(kvp.Key).GetString();
                rowDict[kvp.Value] = cellValue;
                if (!string.IsNullOrWhiteSpace(cellValue)) hasData = true;
            }
            if (hasData)
            {
                result.Add(rowDict);
            }
        }

        return result;
    }
}
