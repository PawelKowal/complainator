namespace ComplainatorAPI.Services.Exceptions;

public class OpenRouterAuthenticationException : OpenRouterException
{
    public OpenRouterAuthenticationException(string message) : base(message)
    {
    }

    public OpenRouterAuthenticationException(string message, Exception innerException) : base(message, innerException)
    {
    }
} 