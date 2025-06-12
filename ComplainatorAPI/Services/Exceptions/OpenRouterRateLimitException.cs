namespace ComplainatorAPI.Services.Exceptions;

public class OpenRouterRateLimitException : OpenRouterException
{
    public int RetryAfterSeconds { get; }

    public OpenRouterRateLimitException(string message, int retryAfterSeconds) : base(message)
    {
        RetryAfterSeconds = retryAfterSeconds;
    }

    public OpenRouterRateLimitException(string message, int retryAfterSeconds, Exception innerException) 
        : base(message, innerException)
    {
        RetryAfterSeconds = retryAfterSeconds;
    }
} 