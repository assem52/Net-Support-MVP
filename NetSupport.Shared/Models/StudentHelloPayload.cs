using System.Text.Json.Serialization;

namespace NetSupport.Shared.Models;

/// <summary>
/// The payload sent by the student app to announce itself to the tutor.
/// </summary>
public class StudentHelloPayload
{
    /// <summary>
    /// The name of the student's PC (e.g., "PC-01").
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The IP address of the student's PC.
    /// </summary>
    [JsonPropertyName("ip")]
    public string Ip { get; set; } = string.Empty;

    /// <summary>
    /// The room the student is in, used by the tutor to filter.
    /// </summary>
    [JsonPropertyName("room")]
    public string Room { get; set; } = string.Empty;
}
