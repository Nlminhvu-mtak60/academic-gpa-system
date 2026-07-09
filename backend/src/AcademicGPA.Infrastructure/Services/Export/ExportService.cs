using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AcademicGPA.Application.Common.Interfaces;
using AcademicGPA.Domain.Interfaces;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AcademicGPA.Infrastructure.Services.Export;

public class ExportService : IExportService
{
    private readonly IUnitOfWork _unitOfWork;

    public ExportService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<byte[]> ExportAcademicReportPdfAsync(Guid studentProfileId, CancellationToken ct = default)
    {
        var profile = await _unitOfWork.Students
            .GetQueryable()
            .FirstOrDefaultAsync(p => p.Id == studentProfileId, ct);

        if (profile == null) throw new Exception("Profile not found");

        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header().Element(ComposeHeader);
                page.Content().Element(x => ComposeContent(x, "Student", profile.StudentCode));
                page.Footer().AlignCenter().Text(x =>
                {
                    x.CurrentPageNumber();
                    x.Span(" / ");
                    x.TotalPages();
                });
            });
        });

        using var ms = new MemoryStream();
        doc.GeneratePdf(ms);
        return ms.ToArray();
    }

    private void ComposeHeader(IContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(column =>
            {
                column.Item().Text("ACADEMIC GPA REPORT").FontSize(20).SemiBold().FontColor(Colors.Blue.Darken2);
                column.Item().Text($"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm}").FontSize(10).FontColor(Colors.Grey.Medium);
            });
        });
    }

    private void ComposeContent(IContainer container, string fullName, string studentId)
    {
        container.PaddingVertical(1, Unit.Centimetre).Column(column =>
        {
            column.Spacing(5);

            column.Item().Row(row =>
            {
                row.RelativeItem().Text($"Student Name: {fullName}").SemiBold();
                row.RelativeItem().Text($"Student ID: {studentId}").SemiBold();
            });

            column.Item().PaddingTop(25).Text("This is a placeholder for the full PDF report.").FontSize(14);
        });
    }

    public async Task<byte[]> ExportAcademicReportExcelAsync(Guid studentProfileId, CancellationToken ct = default)
    {
        var profile = await _unitOfWork.Students
            .GetQueryable()
            .FirstOrDefaultAsync(p => p.Id == studentProfileId, ct);

        if (profile == null) throw new Exception("Profile not found");

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Academic Report");

        worksheet.Cell(1, 1).Value = "Academic GPA Report";
        worksheet.Cell(1, 1).Style.Font.Bold = true;
        worksheet.Cell(1, 1).Style.Font.FontSize = 16;

        worksheet.Cell(3, 1).Value = "Student Name:";
        worksheet.Cell(3, 2).Value = "Student";

        worksheet.Cell(4, 1).Value = "Student ID:";
        worksheet.Cell(4, 2).Value = profile.StudentCode;

        worksheet.Cell(6, 1).Value = "Course Code";
        worksheet.Cell(6, 2).Value = "Course Name";
        worksheet.Cell(6, 3).Value = "Credits";
        worksheet.Cell(6, 4).Value = "Final Score";
        
        var headerRange = worksheet.Range("A6:D6");
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

        worksheet.Cell(7, 1).Value = "Placeholder";
        worksheet.Cell(7, 2).Value = "Data will be populated here";
        worksheet.Cell(7, 3).Value = 3;
        worksheet.Cell(7, 4).Value = 9.5;

        worksheet.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }
}
