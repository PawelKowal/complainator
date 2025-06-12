namespace ComplainatorAPI.Services;

public class OpenRouterSettings
{
    public string EndpointUrl { get; set; } = "https://openrouter.ai/api/v1/chat/completions";
    public string ApiKey { get; set; } = string.Empty;
    public string DefaultModel { get; set; } = "gpt-3.5-turbo";
    public IDictionary<string, object> DefaultParameters { get; set; } = new Dictionary<string, object>
    {
        { "temperature", 0.7 },
        { "max_tokens", 500 }
    };
}