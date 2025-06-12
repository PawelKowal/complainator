using NUnit.Framework;
using NSubstitute;
using FluentAssertions;
using ComplainatorAPI.Controllers;
using ComplainatorAPI.Services;
using Microsoft.Extensions.Logging;
using ComplainatorAPI.DTO;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace ComplainatorAPI.Tests.Controllers;

[TestFixture]
public class AuthControllerTests
{
    private AuthController _controller;
    private IAuthService _authService;
    private ILogger<AuthController> _logger;

    [SetUp]
    public void Setup()
    {
        // Arrange
        _authService = Substitute.For<IAuthService>();
        _logger = Substitute.For<ILogger<AuthController>>();
        _controller = new AuthController(_authService, _logger);
    }

    [Test]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        // Arrange
        var loginDto = new LoginRequest
        {
            Email = "test@example.com",
            Password = "TestPassword123!"
        };

        var expectedResponse = new LoginResponse
        {
            Token = "test.jwt.token",
            User = new UserDto { Id = Guid.NewGuid(), Email = loginDto.Email }
        };

        _authService.LoginAsync(loginDto).Returns(Task.FromResult(expectedResponse));

        // Act
        var result = await _controller.Login(loginDto);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        var value = okResult!.Value as LoginResponse;
        value.Should().NotBeNull();
        value!.Token.Should().Be(expectedResponse.Token);
        value.User.Email.Should().Be(expectedResponse.User.Email);
    }

    [Test]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var loginDto = new LoginRequest
        {
            Email = "invalid@example.com",
            Password = "InvalidPassword123!"
        };

        _authService.LoginAsync(loginDto)
            .Returns<Task<LoginResponse>>(x => throw new UnauthorizedAccessException());

        // Act
        var result = await _controller.Login(loginDto);

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
        var unauthorized = result as UnauthorizedObjectResult;
        unauthorized.Should().NotBeNull();
    }
}