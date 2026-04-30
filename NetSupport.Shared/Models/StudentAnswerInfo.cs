using System.Text.Json.Serialization;

namespace NetSupport.Shared.Models;

/// <summary>
/// Represents the result of a single question answered by a student.
/// Used for generating detailed analytics in the final PDF report.
/// </summary>
public class StudentAnswerInfo
{
    [JsonPropertyName("question_index")]
    public int QuestionIndex { get; set; }

    [JsonPropertyName("question_text")]
    public string QuestionText { get; set; } = string.Empty;

    [JsonPropertyName("selected_option")]
    public string SelectedOption { get; set; } = string.Empty;

    [JsonPropertyName("correct_option")]
    public string CorrectOption { get; set; } = string.Empty;

    [JsonPropertyName("is_correct")]
    public bool IsCorrect { get; set; }
}
