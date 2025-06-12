using ComplainatorAPI.DTO.OpenRouter;

namespace ComplainatorAPI.Services;

public interface IOpenRouterService
{
    /// <summary>
    /// Sends a chat completion request to OpenRouter API.
    /// </summary>
    /// <param name="messages">Collection of system and user messages</param>
    /// <param name="model">Optional model override (defaults to settings)</param>
    /// <param name="parameters">Optional parameters override (defaults to settings)</param>
    /// <returns>Chat completion response</returns>
    Task<object> SendChatAsync(
        IEnumerable<MessageDto> messages,
        string? model = null,
        IDictionary<string, object>? parameters = null);
}