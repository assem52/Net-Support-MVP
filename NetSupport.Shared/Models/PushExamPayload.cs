using System.Text.Json.Serialization;

namespace NetSupport.Shared.Models;

/// <summary>
/// Payload sent from Tutor to Student to push an exam.
/// </summary>
public class PushExamPayload
{
    [JsonPropertyName("exam")]
    public List<ExamQuestion> Questions { get; set; } = new();

    [JsonPropertyName("duration_minutes")]
    public int DurationMinutes { get; set; } = 30;
}
