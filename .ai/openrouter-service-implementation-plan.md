# OpenRouter Service Implementation Plan

**Stack techniczny:** .NET Web API, C#, ASP.NET Core, HttpClient, Polly, System.Text.Json

## 1. Opis usługi

Usługa `OpenRouterService` zapewnia abstrakcję komunikacji z API OpenRouter.ai, umożliwiając wysyłanie wiadomości systemowych oraz użytkownika, definiowanie response_format (schemat JSON), wybór modelu i parametrów, a następnie odbiór i walidację odpowiedzi.

## 2. Opis konstruktora

```csharp
public class OpenRouterService : IOpenRouterService
{
    private readonly HttpClient _httpClient;
    private readonly OpenRouterSettings _settings;
    private readonly ILogger<OpenRouterService> _logger;

    public OpenRouterService(HttpClient httpClient,
                             IOptions<OpenRouterSettings> options,
                             ILogger<OpenRouterService> logger)
    {
        _httpClient = httpClient;
        _settings = options.Value;
        _logger = logger;
    }
}
```

- **HttpClient** skonfigurowany w DI z BaseAddress i nagłówkiem `Authorization: Bearer {ApiKey}`.
- **OpenRouterSettings**: pola `ApiKey`, `EndpointUrl`, `DefaultModel`, `DefaultParameters`.

## 3. Publiczne metody i pola

```csharp
Task<ChatCompletionResponse> SendChatAsync(IEnumerable<MessageDto> messages,
                                          ResponseFormatDto responseFormat,
                                          string model = null,
                                          IDictionary<string, object> parameters = null);
```

- **messages**: kolekcja `MessageDto { string Role; string Content; }` z rolami `system` i `user`.
- **responseFormat**: typ i schemat:
  ```json
  {
    "type": "json_schema",
    "json_schema": {
      "name": "SchemaName",
      "strict": true,
      "schema": {
        /* ... */
      }
    }
  }
  ```
- **model**: np. `gpt-3.5-turbo`; domyślnie z ustawień.
- **parameters**: temperatura, max_tokens itd.

## 4. Prywatne metody i pola

- `BuildRequestPayload(...)`: konstruuje `ChatCompletionRequest` z `Model`, `Messages`, `ResponseFormat`, i `Parameters`.
- `ValidateAndParseResponse(...)`: waliduje JSON wg `responseFormat.JsonSchema` i mapuje na `ChatCompletionResponse`.
- `HandleHttpErrors(HttpResponseMessage response)`: rzuca niestandardowe wyjątki w zależności od statusu.

## 5. Obsługa błędów

1. **NetworkFailure** (HttpRequestException)
2. **Timeout** (TaskCanceledException)
3. **Unauthorized** (401) → AuthenticationException
4. **RateLimit** (429) → RateLimitException z informacją o `Retry-After`
5. **ServerError** (5xx) → ServerException
6. **InvalidJson** → JsonValidationException

## 6. Kwestie bezpieczeństwa

- Przechowywać `ApiKey` w zmiennych środowiskowych.
- Nie logować pełnych treści wiadomości/zapytań.
- Restruktura retry/circuit-breaker (Polly) zapobiegająca przeciążeniom.
- Walidacja schematu odporniejsza na ataki typu injection.

## 7. Plan wdrożenia krok po kroku

1. **Konfiguracja ustawień**
   - Dodaj klasę `OpenRouterSettings` i skonfiguruj mapowanie z `appsettings.json`:
     ```json
     "OpenRouter": {
       "EndpointUrl": "https://api.openrouter.ai/v1/chat/completions",
       "ApiKey": "<TWÓJ_KLUCZ>",
       "DefaultModel": "gpt-3.5-turbo",
       "DefaultParameters": { "temperature": 0.7, "max_tokens": 500 }
     }
     ```
2. **Rejestracja HttpClient w DI**
   ```csharp
   services.AddHttpClient<IOpenRouterService, OpenRouterService>(client => {
       client.BaseAddress = new Uri(Configuration["OpenRouter:EndpointUrl"]);
       client.DefaultRequestHeaders.Authorization =
         new AuthenticationHeaderValue("Bearer", Configuration["OpenRouter:ApiKey"]);
   })
   .AddPolicyHandler(Policy<HttpResponseMessage>
       .Handle<HttpRequestException>()
       .OrResult(r => (int)r.StatusCode >= 500)
       .RetryAsync(3));
   ```
3. **Definicja DTO**
   - `MessageDto { string Role; string Content; }`
   - `ChatCompletionRequest { string Model; List<MessageDto> Messages; ResponseFormatDto ResponseFormat; Dictionary<string, object> Parameters; }`
   - `ResponseFormatDto { string Type; JsonSchemaDefinition JsonSchema; }`
   - `JsonSchemaDefinition { string Name; bool Strict; object Schema; }`
   - `ChatCompletionResponse { /* dopasowane do schema */ }`
4. **Implementacja publicznej metody**
   - Zbieranie `messages`, tworzenie payload w `BuildRequestPayload`, serializacja do JSON.
   - `await _httpClient.PostAsJsonAsync("", payload)`.
   - Obsługa kodów statusu w `HandleHttpErrors`.
   - Deserializacja odpowiedzi i walidacja w `ValidateAndParseResponse`.
5. **Przykład wstawiania wiadomości i response_format**
   ```csharp
   var systemMsg = new MessageDto { Role = "system", Content = "You are an assistant." };
   var userMsg = new MessageDto { Role = "user", Content = userInput };
   var responseFormat = new ResponseFormatDto {
     Type = "json_schema",
     JsonSchema = new JsonSchemaDefinition {
       Name = "OpenRouterResponse",
       Strict = true,
       Schema = new {
         suggestions = new { type = "array", items = "string" }
       }
     }
   };
   var result = await openRouterService.SendChatAsync(
       new [] { systemMsg, userMsg },
       responseFormat);
   ```
6. **Walidacja odp.**
   - Użyj `System.Text.Json` z `JsonDocument` i opcjonalnie `JsonSchema.Net` do potwierdzenia zgodności.
