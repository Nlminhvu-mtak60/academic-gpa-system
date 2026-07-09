using System;
using System.Threading;
using System.Threading.Tasks;

namespace AcademicGPA.Application.Common.Interfaces;

public interface IExportService
{
    Task<byte[]> ExportAcademicReportPdfAsync(Guid studentProfileId, CancellationToken ct = default);
    Task<byte[]> ExportAcademicReportExcelAsync(Guid studentProfileId, CancellationToken ct = default);
}
