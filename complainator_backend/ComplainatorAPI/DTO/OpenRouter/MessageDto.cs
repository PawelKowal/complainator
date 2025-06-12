using System.Text.Json.Serialization;

namespace ComplainatorAPI.DTO.OpenRouter;

public class MessageDto
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;
}