using System.Text.Json.Serialization;

namespace NetSupport.Shared.Models;

/// <summary>
/// Payload sent by the Student to the Tutor every time an answer is selected.
/// Used to provide real-time updates on the student's progress during an exam.
/// </summary>
public class AnswerUpdatePayload
{
    /// <summary>
    /// The IP address of the student sending the update.
    /// </summary>
    [JsonPropertyName("ip")]
    public string Ip { get; set; } = string.Empty;

    /// <summary>
    /// The live score representation, typically formatted as "Correct/Total" (e.g., "2/5").
    /// </summary>
    [JsonPropertyName("score_string")]
    public string ScoreString { get; set; } = string.Empty;
}
