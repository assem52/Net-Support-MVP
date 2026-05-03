using System.IO;
using System.Linq;
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
                if (_isArabic)
                {
                    page.ContentFromRightToLeft();
                }
                page.DefaultTextStyle(x => x.FontSize(11).FontFamily(Fonts.Arial));

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
                column.Item().Text("NetSupport School MVP").FontSize(22).ExtraBold().FontColor(Colors.Blue.Darken3);
                column.Item().Text(NetSupport.Shared.Services.TranslationService.Translate("Final Exam Analytics Report", _isArabic)).FontSize(16).SemiBold().FontColor(Colors.Grey.Darken3);
                column.Item().Text($"{NetSupport.Shared.Services.TranslationService.Translate("Generated on:", _isArabic)} {DateTime.Now:f}").FontSize(10).FontColor(Colors.Grey.Medium);
            });
        });
    }

    private void ComposeContent(IContainer container, IEnumerable<StudentHelloPayload> students)
    {
        var studentList = students.ToList();
        
        container.PaddingVertical(1, Unit.Centimetre).Column(column =>
        {
            // 1. Summary Dashboard
            column.Item().Element(c => ComposeSummary(c, studentList));
            
            column.Item().PageBreak();

            // 2. Question Difficulty Analysis
            column.Item().Element(c => ComposeQuestionAnalysis(c, studentList));
            
            column.Item().PageBreak();

            // 3. Individual Student Details
            column.Item().Text(NetSupport.Shared.Services.TranslationService.Translate("Individual Student Results", _isArabic)).FontSize(18).SemiBold().FontColor(Colors.Blue.Darken2);
            column.Item().PaddingBottom(10).LineHorizontal(1).LineColor(Colors.Blue.Lighten3);

            foreach (var student in studentList)
            {
                column.Item().PaddingTop(15).Element(c => ComposeStudentDetail(c, student));
            }
        });
    }

    private void ComposeSummary(IContainer container, List<StudentHelloPayload> students)
    {
        var scores = students.Select(s => ParseScore(s.Score)).ToList();
        var avg = scores.Any() ? scores.Average() : 0;
        var max = scores.Any() ? scores.Max() : 0;
        var min = scores.Any() ? scores.Min() : 0;
        var passed = scores.Count(s => s >= 50);
        var failed = scores.Count(s => s < 50);

        container.Column(column =>
        {
            column.Item().Text(NetSupport.Shared.Services.TranslationService.Translate("Class Summary Dashboard", _isArabic)).FontSize(18).SemiBold().FontColor(Colors.Blue.Darken2);
            column.Item().PaddingBottom(10).LineHorizontal(1).LineColor(Colors.Blue.Lighten3);

            column.Item().PaddingVertical(10).Row(row =>
            {
                row.RelativeItem().Element(c => SummaryBox(c, NetSupport.Shared.Services.TranslationService.Translate("Total Students", _isArabic), students.Count.ToString(), Colors.Blue.Lighten5));
                row.ConstantItem(10);
                row.RelativeItem().Element(c => SummaryBox(c, NetSupport.Shared.Services.TranslationService.Translate("Class Average", _isArabic), $"{avg:F1}%", Colors.Green.Lighten5));
                row.ConstantItem(10);
                row.RelativeItem().Element(c => SummaryBox(c, NetSupport.Shared.Services.TranslationService.Translate("Top Score", _isArabic), $"{max:F1}%", Colors.Amber.Lighten5));
            });

            column.Item().Row(row =>
            {
                row.RelativeItem().Element(c => SummaryBox(c, NetSupport.Shared.Services.TranslationService.Translate("Passed", _isArabic), passed.ToString(), Colors.Green.Lighten4));
                row.ConstantItem(10);
                row.RelativeItem().Element(c => SummaryBox(c, NetSupport.Shared.Services.TranslationService.Translate("Failed", _isArabic), failed.ToString(), Colors.Red.Lighten5));
                row.ConstantItem(10);
                row.RelativeItem().Element(c => SummaryBox(c, NetSupport.Shared.Services.TranslationService.Translate("Lowest Score", _isArabic), $"{min:F1}%", Colors.Grey.Lighten4));
            });
        });

        void SummaryBox(IContainer c, string title, string value, string bgColor)
        {
            c.Background(bgColor).Padding(10).Column(col =>
            {
                col.Item().Text(title).FontSize(10).SemiBold().FontColor(Colors.Grey.Darken2);
                col.Item().Text(value).FontSize(20).ExtraBold().FontColor(Colors.Black);
            });
        }
    }

    private void ComposeQuestionAnalysis(IContainer container, List<StudentHelloPayload> students)
    {
        var allAnswers = students.Where(s => s.DetailedResults != null).SelectMany(s => s.DetailedResults).ToList();
        if (!allAnswers.Any()) return;

        var questionStats = allAnswers.GroupBy(a => a.QuestionText)
            .Select(g => new {
                Text = g.Key,
                Correct = g.Count(a => a.IsCorrect),
                Total = g.Count(),
                Rate = (float)g.Count(a => a.IsCorrect) / g.Count() * 100
            })
            .OrderBy(q => q.Rate)
            .ToList();

        var hardest = questionStats.First();

        container.Column(column =>
        {
            column.Item().Text(NetSupport.Shared.Services.TranslationService.Translate("Question Difficulty Analysis", _isArabic)).FontSize(18).SemiBold().FontColor(Colors.Blue.Darken2);
            column.Item().PaddingBottom(10).LineHorizontal(1).LineColor(Colors.Blue.Lighten3);

            // Hardest Question Alert
            column.Item().PaddingVertical(10).Background(Colors.Red.Lighten5).Padding(10).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text(NetSupport.Shared.Services.TranslationService.Translate("Hardest Question:", _isArabic)).FontSize(10).SemiBold().FontColor(Colors.Red.Darken2);
                    col.Item().Text(hardest.Text).FontSize(12).Italic();
                    col.Item().Text($"{NetSupport.Shared.Services.TranslationService.Translate("Success Rate:", _isArabic)} {hardest.Rate:F1}%").FontSize(11).Bold().FontColor(Colors.Red.Medium);
                });
            });

            column.Item().PaddingTop(10).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn();
                    columns.ConstantColumn(80);
                    columns.ConstantColumn(80);
                });

                table.Header(header =>
                {
                    header.Cell().Element(CellStyle).Text(NetSupport.Shared.Services.TranslationService.Translate("Question Text", _isArabic));
                    header.Cell().Element(CellStyle).AlignCenter().Text(NetSupport.Shared.Services.TranslationService.Translate("Correct", _isArabic));
                    header.Cell().Element(CellStyle).AlignCenter().Text(NetSupport.Shared.Services.TranslationService.Translate("Success %", _isArabic));
                    
                    static IContainer CellStyle(IContainer c) => c.DefaultTextStyle(x => x.SemiBold()).BorderBottom(1).PaddingBottom(5);
                });

                foreach (var q in questionStats)
                {
                    table.Cell().Element(CellStyle).Text(q.Text);
                    table.Cell().Element(CellStyle).AlignCenter().Text($"{q.Correct} / {q.Total}");
                    
                    var color = q.Rate < 50 ? Colors.Red.Medium : (q.Rate < 80 ? Colors.Orange.Medium : Colors.Green.Medium);
                    table.Cell().Element(CellStyle).AlignCenter().Text($"{q.Rate:F1}%").FontColor(color).Bold();

                    static IContainer CellStyle(IContainer c) => c.BorderBottom(1).BorderColor(Colors.Grey.Lighten3).PaddingVertical(5);
                }
            });
        });
    }

    private void ComposeStudentDetail(IContainer container, StudentHelloPayload student)
    {
        var scorePercent = ParseScore(student.Score);
        var isPassed = scorePercent >= 50;
        var statusColor = isPassed ? Colors.Green.Medium : Colors.Red.Medium;
        var statusText = isPassed ? NetSupport.Shared.Services.TranslationService.Translate("PASSED", _isArabic) : NetSupport.Shared.Services.TranslationService.Translate("FAILED", _isArabic);

        container.Column(column =>
        {
            // Header Row
            column.Item().Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text(student.Name).FontSize(14).Bold();
                    col.Item().Text(student.Ip).FontSize(10).FontColor(Colors.Grey.Medium);
                });
                row.RelativeItem().AlignRight().Column(col =>
                {
                    col.Item().Text($"{NetSupport.Shared.Services.TranslationService.Translate("Score:", _isArabic)} {student.Score}").FontSize(12).Bold();
                    col.Item().Text(statusText).FontSize(10).Bold().FontColor(statusColor);
                });
            });

            column.Item().PaddingTop(5).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(25);
                    columns.RelativeColumn(5);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(2);
                    columns.ConstantColumn(60);
                });

                table.Header(header =>
                {
                    header.Cell().Element(CellStyle).Text("#");
                    header.Cell().Element(CellStyle).Text(NetSupport.Shared.Services.TranslationService.Translate("Question", _isArabic));
                    header.Cell().Element(CellStyle).Text(NetSupport.Shared.Services.TranslationService.Translate("Answer", _isArabic));
                    header.Cell().Element(CellStyle).Text(NetSupport.Shared.Services.TranslationService.Translate("Correct", _isArabic));
                    header.Cell().Element(CellStyle).AlignCenter().Text(NetSupport.Shared.Services.TranslationService.Translate("Status", _isArabic));
                    
                    static IContainer CellStyle(IContainer c) => c.DefaultTextStyle(x => x.SemiBold().FontSize(9)).BorderBottom(1).PaddingBottom(3);
                });

                if (student.DetailedResults != null)
                {
                    int i = 0;
                    foreach (var ans in student.DetailedResults)
                    {
                        var rowBg = (i++ % 2 == 0) ? Colors.White : Colors.Grey.Lighten5;
                        
                        table.Cell().Element(c => CellStyle(c, rowBg)).Text(ans.QuestionIndex.ToString());
                        table.Cell().Element(c => CellStyle(c, rowBg)).Text(ans.QuestionText);
                        table.Cell().Element(c => CellStyle(c, rowBg)).Text(ans.SelectedOption);
                        table.Cell().Element(c => CellStyle(c, rowBg)).Text(ans.CorrectOption);
                        
                        var cColor = ans.IsCorrect ? Colors.Green.Medium : Colors.Red.Medium;
                        var cText = ans.IsCorrect ? "✔" : "✘";
                        table.Cell().Element(c => CellStyle(c, rowBg)).AlignCenter().Text(cText).FontColor(cColor).Bold().FontSize(12);

                        static IContainer CellStyle(IContainer c, string bg) => c.Background(bg).PaddingVertical(3).DefaultTextStyle(x => x.FontSize(9));
                    }
                }
            });

            column.Item().PaddingVertical(10).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten3);
        });
    }

    private float ParseScore(string score)
    {
        if (string.IsNullOrEmpty(score)) return 0;
        // Remove "FINAL: " if present
        var cleanScore = score.Replace("FINAL: ", "").Trim();
        var parts = cleanScore.Split('/');
        if (parts.Length == 2 && float.TryParse(parts[0], out float correct) && float.TryParse(parts[1], out float total) && total > 0)
        {
            return (correct / total) * 100;
        }
        return 0;
    }
}
