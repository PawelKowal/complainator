using System.Net.Http.Json;
using System.Text.Json;
using ComplainatorAPI.DTO.OpenRouter;
using ComplainatorAPI.Services.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ComplainatorAPI.Services;

public class OpenRouterService : IOpenRouterService
{
    private readonly HttpClient _httpClient;
    private readonly OpenRouterSettings _settings;
    private readonly ILogger<OpenRouterService> _logger;

    public OpenRouterService(
        HttpClient httpClient,
        IOptions<OpenRouterSettings> options,
        ILogger<OpenRouterService> logger)
    {
        _httpClient = httpClient;
        _settings = options.Value;
        _logger = logger;

        // Configure HttpClient
        _httpClient.BaseAddress = new Uri(_settings.EndpointUrl);
        _httpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _settings.ApiKey);
    }

    public async Task<object> SendChatAsync(
        IEnumerable<MessageDto> messages,
        string? model = null,
        IDictionary<string, object>? parameters = null)
    {
        try
        {
            var request = BuildRequestPayload(messages, model, parameters);
            _logger.LogInformation("Sending request to OpenRouter API. Model: {Model}, Messages: {MessageCount}", 
                request.Model, request.Messages.Count);
            
            var requestJson = JsonSerializer.Serialize(request, new JsonSerializerOptions { WriteIndented = true });
            _logger.LogInformation("Request payload: {RequestJson}", requestJson);
            
            var response = await _httpClient.PostAsJsonAsync("", request);
            
            _logger.LogInformation("Received response from OpenRouter API. Status: {StatusCode}", response.StatusCode);
            var rawResponse = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Raw response: {RawResponse}", rawResponse);
            
            // Handle HTTP errors first
            await HandleHttpErrors(response);

            // Try to parse as JSON
            JsonDocument? jsonResponse = null;
            try
            {
                jsonResponse = JsonDocument.Parse(rawResponse);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse response as JSON: {RawResponse}", rawResponse);
                throw new OpenRouterException("Invalid JSON response from OpenRouter API", ex);
            }

            if (jsonResponse == null)
            {
                throw new OpenRouterException("Received empty response from OpenRouter API");
            }

            // Check for API-level errors in JSON response
            if (jsonResponse.RootElement.TryGetProperty("error", out var errorElement))
            {
                var errorMessage = errorElement.GetProperty("message").GetString();
                _logger.LogError("OpenRouter API returned error in response: {ErrorMessage}", errorMessage);
                throw new OpenRouterException($"OpenRouter API error: {errorMessage}");
            }

            return jsonResponse.RootElement.Clone();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error while communicating with OpenRouter API");
            throw new OpenRouterException("Failed to communicate with OpenRouter API", ex);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Request to OpenRouter API timed out");
            throw new OpenRouterException("Request timed out", ex);
        }
        catch (OpenRouterException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while processing OpenRouter request");
            throw new OpenRouterException("An unexpected error occurred", ex);
        }
    }

    private ChatCompletionRequest BuildRequestPayload(
        IEnumerable<MessageDto> messages,
        string? model = null,
        IDictionary<string, object>? parameters = null)
    {
        var request = new ChatCompletionRequest
        {
            Model = model ?? _settings.DefaultModel,
            Messages = messages.ToList()
        };

        // Apply parameters if provided, otherwise use defaults
        var finalParams = parameters ?? _settings.DefaultParameters;
        if (finalParams.TryGetValue("temperature", out var temp) && temp is double temperature)
        {
            request.Temperature = temperature;
        }
        if (finalParams.TryGetValue("max_tokens", out var tokens) && tokens is int maxTokens)
        {
            request.MaxTokens = maxTokens;
        }

        return request;
    }

    private async Task HandleHttpErrors(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var errorContent = await response.Content.ReadAsStringAsync();
        _logger.LogError("OpenRouter API error: {StatusCode} - {Content}", 
            response.StatusCode, errorContent);

        switch ((int)response.StatusCode)
        {
            case 401:
                throw new OpenRouterAuthenticationException("Invalid API key or unauthorized access");
                
            case 429:
                var retryAfter = response.Headers.RetryAfter?.Delta?.Seconds ?? 60;
                throw new OpenRouterRateLimitException(
                    "Rate limit exceeded. Please try again later.", 
                    retryAfter);
                
            case >= 500:
                throw new OpenRouterServerException(
                    "OpenRouter API server error", 
                    (int)response.StatusCode);
                
            default:
                throw new OpenRouterException(
                    $"OpenRouter API error: {response.StatusCode} - {errorContent}");
        }
    }
} 