using System.IO;
using NetSupport.Shared.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace NetSupport.Tutor.Services;

/// <summary>
/// Generates a PDF report containing the final exam scores of all students.
/// Uses QuestPDF library for rendering.
/// </summary>
public class PdfReportGenerator
{
    public string GenerateReport(IEnumerable<StudentHelloPayload> students)
    {
        // Must configure QuestPDF license type (Community is free for MVP/Open Source)
        QuestPDF.Settings.License = LicenseType.Community;

        // Ensure the reports directory exists
        string reportsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Reports");
        if (!Directory.Exists(reportsDir))
        {
            Directory.CreateDirectory(reportsDir);
        }

        string fileName = $"ExamReport_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
        string filePath = Path.Combine(reportsDir, fileName);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(12).FontFamily(Fonts.Arial));

                page.Header().Element(ComposeHeader);
                page.Content().Element(x => ComposeContent(x, students));
                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Page ");
                    x.CurrentPageNumber();
                    x.Span(" of ");
                    x.TotalPages();
                });
            });
        });

        document.GeneratePdf(filePath);
        return filePath;
    }

    private void ComposeHeader(IContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(column =>
            {
                column.Item().Text("NetSupport School MVP").FontSize(20).SemiBold().FontColor(Colors.Blue.Darken2);
                column.Item().Text($"Final Exam Report").FontSize(14).SemiBold();
                column.Item().Text($"Generated on: {DateTime.Now:f}");
            });
        });
    }

    private void ComposeContent(IContainer container, IEnumerable<StudentHelloPayload> students)
    {
        container.PaddingVertical(1, Unit.Centimetre).Column(column =>
        {
            column.Spacing(5);

            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3); // Name
                    columns.RelativeColumn(2); // IP
                    columns.RelativeColumn(2); // Status
                    columns.RelativeColumn(2); // Score
                });

                table.Header(header =>
                {
                    header.Cell().Element(CellStyle).Text("Student Name").SemiBold();
                    header.Cell().Element(CellStyle).Text("IP Address").SemiBold();
                    header.Cell().Element(CellStyle).Text("Status").SemiBold();
                    header.Cell().Element(CellStyle).Text("Score").SemiBold();

                    static IContainer CellStyle(IContainer container)
                    {
                        return container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                    }
                });

                foreach (var student in students)
                {
                    table.Cell().Element(CellStyle).Text(student.Name);
                    table.Cell().Element(CellStyle).Text(student.Ip);
                    table.Cell().Element(CellStyle).Text(student.IsReady ? "In Exam" : "Idle");
                    table.Cell().Element(CellStyle).Text(string.IsNullOrWhiteSpace(student.Score) ? "N/A" : student.Score);

                    static IContainer CellStyle(IContainer container)
                    {
                        return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
                    }
                }
            });
        });
    }
}
