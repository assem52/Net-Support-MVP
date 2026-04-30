using System.IO;
using System.Text;
using NetSupport.Shared.Models;

namespace NetSupport.Designer.Services;

public class CsvExporter
{
    public void ExportToCsv(string filePath, IEnumerable<ExamQuestion> questions)
    {
        // Use UTF8 encoding with BOM so that Excel and Windows natively recognize Arabic characters
        var encoding = new UTF8Encoding(true);
        
        using var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
        using var writer = new StreamWriter(stream, encoding);
        
        // Write the header as required by the spec
        writer.WriteLine("QuestionText,OptionA,OptionB,OptionC,OptionD,CorrectOption");
        
        foreach (var q in questions)
        {
            // Escape commas in the text just in case the user typed a comma in their question
            var qt = EscapeForCsv(q.QuestionText);
            var oa = EscapeForCsv(q.OptionA);
            var ob = EscapeForCsv(q.OptionB);
            var oc = EscapeForCsv(q.OptionC);
            var od = EscapeForCsv(q.OptionD);
            var correct = EscapeForCsv(q.CorrectOption);
            
            writer.WriteLine($"{qt},{oa},{ob},{oc},{od},{correct}");
        }
    }

    private string EscapeForCsv(string field)
    {
        if (string.IsNullOrEmpty(field)) return "";
        // If the field contains a comma or quote, wrap it in quotes and double the internal quotes
        if (field.Contains(',') || field.Contains('"') || field.Contains('\n'))
        {
            return $"\"{field.Replace("\"", "\"\"")}\"";
        }
        return field;
    }
}
