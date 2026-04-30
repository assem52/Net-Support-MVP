using System.Text.Json.Serialization;

namespace NetSupport.Shared.Models;

public class AnswerUpdatePayload
{
    [JsonPropertyName("ip")]
    public string Ip { get; set; } = string.Empty;

    [JsonPropertyName("score_string")]
    public string ScoreString { get; set; } = string.Empty;
}
