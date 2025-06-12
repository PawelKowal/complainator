using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using Microsoft.Extensions.Logging;

namespace ComplainatorAPI.Tests.Helpers;

public static class TestHelper
{
    public static ClaimsPrincipal CreateTestUser(string userId = "test-user-id", string email = "test@example.com")
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Email, email)
        };

        return new ClaimsPrincipal(new ClaimsIdentity(claims));
    }

    public static HttpContext CreateMockHttpContext(ClaimsPrincipal? user = null)
    {
        var context = Substitute.For<HttpContext>();
        context.User = user ?? CreateTestUser();
        return context;
    }

    public static ILogger<T> CreateMockLogger<T>() where T : class
    {
        return Substitute.For<ILogger<T>>();
    }
}