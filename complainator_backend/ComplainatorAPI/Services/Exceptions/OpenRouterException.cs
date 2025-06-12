namespace ComplainatorAPI.Services.Exceptions;

public class OpenRouterException : Exception
{
    public OpenRouterException(string message) : base(message)
    {
    }

    public OpenRouterException(string message, Exception innerException) : base(message, innerException)
    {
    }
}