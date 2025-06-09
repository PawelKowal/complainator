using ComplainatorAPI.DTO;

namespace ComplainatorAPI.Services
{
    public interface IAuthService
    {
        Task<RegisterResponse> RegisterAsync(RegisterRequest request);
    }
} 