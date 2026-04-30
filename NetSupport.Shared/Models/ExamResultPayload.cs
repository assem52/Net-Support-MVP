using System.Text.Json.Serialization;

namespace NetSupport.Shared.Models;

/// <summary>
/// Payload sent by the Student to the Tutor when the exam is finished.
/// This happens either when the timer runs out or the student manually submits.
/// </summary>
public class ExamResultPayload
{
    /// <summary>
    /// The IP address of the student submitting the exam.
    /// </summary>
    [JsonPropertyName("ip")]
    public string Ip { get; set; } = string.Empty;

    /// <summary>
    /// The final score representation, typically formatted as "Correct/Total" (e.g., "5/5").
    /// </summary>
    [JsonPropertyName("final_score")]
    public string FinalScore { get; set; } = string.Empty;
}
