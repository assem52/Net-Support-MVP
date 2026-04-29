using System.Text.Json;
using System.Text.Json.Serialization;

namespace NetSupport.Shared.Models;

/// <summary>
/// The standard envelope for all messages sent over the network.
/// </summary>
public class NetworkMessage
{
    /// <summary>
    /// Identifies what kind of message this is (e.g., "STUDENT_HELLO", "LOCK").
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// The actual data being sent. Since it can be different objects, we use JsonElement.
    /// </summary>
    [JsonPropertyName("payload")]
    public JsonElement Payload { get; set; }

    /// <summary>
    /// When the message was created.
    /// </summary>
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
