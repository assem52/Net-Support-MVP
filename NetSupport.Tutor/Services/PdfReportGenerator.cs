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
    private bool _isArabic;

    public string GenerateReport(IEnumerable<StudentHelloPayload> students, bool isArabic = false)
    {
        _isArabic = isArabic;
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
                page.PageDirection(_isArabic ? TextDirection.RightToLeft : TextDirection.LeftToRight);
                page.DefaultTextStyle(x => x.FontSize(12).FontFamily(Fonts.Arial));

                page.Header().Element(ComposeHeader);
                page.Content().Element(x => ComposeContent(x, students));
                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span(_isArabic ? "صفحة " : "Page ");
                    x.CurrentPageNumber();
                    x.Span(_isArabic ? " من " : " of ");
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
                column.Item().Text(NetSupport.Shared.Services.TranslationService.Translate("Final Exam Report", _isArabic)).FontSize(14).SemiBold();
                column.Item().Text($"{NetSupport.Shared.Services.TranslationService.Translate("Generated on:", _isArabic)} {DateTime.Now:f}");
            });
        });
    }

    private void ComposeContent(IContainer container, IEnumerable<StudentHelloPayload> students)
    {
        container.PaddingVertical(1, Unit.Centimetre).Column(column =>
        {
            foreach (var student in students)
            {
                // Student Header
                column.Item().PaddingTop(20).Text($"{NetSupport.Shared.Services.TranslationService.Translate("Student:", _isArabic)} {student.Name} ({student.Ip})").FontSize(16).SemiBold();
                column.Item().Text($"{NetSupport.Shared.Services.TranslationService.Translate("Final Score:", _isArabic)} {student.Score}").FontSize(14).FontColor(Colors.Grey.Darken2);
                column.Item().PaddingBottom(10);

                // Detailed Answers Table
                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(30); // Q#
                        columns.RelativeColumn(4);  // Question
                        columns.RelativeColumn(2);  // Selected
                        columns.RelativeColumn(2);  // Correct
                        columns.RelativeColumn(2);  // Status
                    });

                    table.Header(header =>
                    {
                        header.Cell().Element(CellStyle).Text("#");
                        header.Cell().Element(CellStyle).Text(NetSupport.Shared.Services.TranslationService.Translate("Question", _isArabic));
                        header.Cell().Element(CellStyle).Text(NetSupport.Shared.Services.TranslationService.Translate("Answer Given", _isArabic));
                        header.Cell().Element(CellStyle).Text(NetSupport.Shared.Services.TranslationService.Translate("Correct Answer", _isArabic));
                        header.Cell().Element(CellStyle).Text(NetSupport.Shared.Services.TranslationService.Translate("Status", _isArabic));
                        
                        static IContainer CellStyle(IContainer c) => c.DefaultTextStyle(x => x.SemiBold()).BorderBottom(1).BorderColor(Colors.Black).PaddingBottom(5);
                    });

                    if (student.DetailedResults != null)
                    {
                        foreach (var ans in student.DetailedResults)
                        {
                            table.Cell().Element(CellStyle).Text(ans.QuestionIndex.ToString());
                            table.Cell().Element(CellStyle).Text(ans.QuestionText);
                            table.Cell().Element(CellStyle).Text(ans.SelectedOption);
                            table.Cell().Element(CellStyle).Text(ans.CorrectOption);
                            
                            var statusColor = ans.IsCorrect ? Colors.Green.Medium : Colors.Red.Medium;
                            var statusText = ans.IsCorrect ? NetSupport.Shared.Services.TranslationService.Translate("Correct", _isArabic) : NetSupport.Shared.Services.TranslationService.Translate("Incorrect", _isArabic);
                            table.Cell().Element(CellStyle).Text(statusText).FontColor(statusColor).SemiBold();

                            static IContainer CellStyle(IContainer c) => c.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
                        }
                    }
                });

                // Separator Line
                column.Item().PaddingVertical(20).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
            }
        });
    }
}
