using System.Text.Json.Serialization;

namespace NetSupport.Shared.Models;

/// <summary>
/// Payload sent by the Student to notify the Tutor that they need help.
/// </summary>
public class RaiseHandPayload
{
    [JsonPropertyName("ip")]
    public string Ip { get; set; } = string.Empty;

    [JsonPropertyName("is_raising")]
    public bool IsRaising { get; set; } = true;

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
}
