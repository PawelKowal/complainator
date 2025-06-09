using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ComplainatorAPI.Domain.Entities;
using ComplainatorAPI.Domain.Settings;
using ComplainatorAPI.DTO;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ComplainatorAPI.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly JwtSettings _jwtSettings;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            IOptions<JwtSettings> jwtSettings,
            ILogger<AuthService> logger)
        {
            _userManager = userManager;
            _jwtSettings = jwtSettings.Value;
            _logger = logger;
        }

        public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
        {
            // Check if user exists
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                _logger.LogWarning("Registration attempt with existing email: {Email}", request.Email);
                throw new InvalidOperationException("Email already exists");
            }

            // Create new user
            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                EmailConfirmed = true // For simplicity, we're auto-confirming emails
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError("User registration failed for {Email}. Errors: {Errors}", request.Email, errors);
                throw new InvalidOperationException($"Failed to create user: {errors}");
            }

            // Generate JWT token
            var token = GenerateJwtToken(user);

            _logger.LogInformation("User registered successfully: {Email}", request.Email);

            return new RegisterResponse
            {
                Token = token,
                User = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email!
                }
            };
        }

        private string GenerateJwtToken(ApplicationUser user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddMinutes(_jwtSettings.DurationInMinutes);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: expires,
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
} 