namespace ComplainatorAPI.Services.Exceptions;

public class OpenRouterServerException : OpenRouterException
{
    public int StatusCode { get; }

    public OpenRouterServerException(string message, int statusCode) : base(message)
    {
        StatusCode = statusCode;
    }

    public OpenRouterServerException(string message, int statusCode, Exception innerException) 
        : base(message, innerException)
    {
        StatusCode = statusCode;
    }
} 