using System.Text.Json.Serialization;

namespace NetSupport.Shared.Models;

public class ExamResultPayload
{
    [JsonPropertyName("ip")]
    public string Ip { get; set; } = string.Empty;

    [JsonPropertyName("final_score")]
    public string FinalScore { get; set; } = string.Empty;
}
