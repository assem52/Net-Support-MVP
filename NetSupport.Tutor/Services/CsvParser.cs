using System.IO;
using NetSupport.Shared.Models;

namespace NetSupport.Tutor.Services;

/// <summary>
/// Reads a CSV file created by the Designer app and turns it back into ExamQuestion objects.
/// </summary>
public class CsvParser
{
    public List<ExamQuestion> ParseCsv(string filePath)
    {
        var questions = new List<ExamQuestion>();
        
        // Read lines, skipping the header row
        var lines = File.ReadAllLines(filePath).Skip(1);
        
        int index = 1;
        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            
            var parts = ParseCsvLine(line);
            if (parts.Count >= 6)
            {
                questions.Add(new ExamQuestion
                {
                    Index = index++,
                    QuestionText = parts[0],
                    OptionA = parts[1],
                    OptionB = parts[2],
                    OptionC = parts[3],
                    OptionD = parts[4],
                    CorrectOption = parts[5]
                });
            }
        }
        return questions;
    }

    // A simple CSV parser that respects quotes to handle commas inside the question text
    private List<string> ParseCsvLine(string line)
    {
        var result = new List<string>();
        bool inQuotes = false;
        string currentField = "";
        
        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (c == '\"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(currentField);
                currentField = "";
            }
            else
            {
                currentField += c;
            }
        }
        result.Add(currentField);
        
        // Clean up quotes
        for (int i = 0; i < result.Count; i++)
        {
            if (result[i].StartsWith("\"") && result[i].EndsWith("\""))
            {
                result[i] = result[i].Substring(1, result[i].Length - 2).Replace("\"\"", "\"");
            }
        }
        
        return result;
    }
}
