using System.Text.Json.Serialization;

namespace NetSupport.Shared.Models;

/// <summary>
/// Payload sent from Student to Tutor when they log in to the exam.
/// </summary>
public class StudentReadyPayload
{
    [JsonPropertyName("student_name")]
    public string StudentName { get; set; } = string.Empty;

    [JsonPropertyName("ip")]
    public string Ip { get; set; } = string.Empty;
}
