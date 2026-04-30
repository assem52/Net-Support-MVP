using System.Text.Json.Serialization;

namespace NetSupport.Shared.Models;

/// <summary>
/// Represents a single multiple-choice question in an exam.
/// </summary>
public class ExamQuestion
{
    [JsonPropertyName("index")]
    public int Index { get; set; }

    [JsonPropertyName("question")]
    public string QuestionText { get; set; } = string.Empty;

    // We'll use individual properties for options to make CSV export easier
    [JsonPropertyName("optionA")]
    public string OptionA { get; set; } = string.Empty;

    [JsonPropertyName("optionB")]
    public string OptionB { get; set; } = string.Empty;

    [JsonPropertyName("optionC")]
    public string OptionC { get; set; } = string.Empty;

    [JsonPropertyName("optionD")]
    public string OptionD { get; set; } = string.Empty;

    /// <summary>
    /// The correct option letter: "A", "B", "C", or "D".
    /// </summary>
    [JsonPropertyName("correct")]
    public string CorrectOption { get; set; } = "A";
}
