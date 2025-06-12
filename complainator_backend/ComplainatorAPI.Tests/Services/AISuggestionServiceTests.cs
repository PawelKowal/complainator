using System.Text.Json;
using ComplainatorAPI.Domain.Entities;
using ComplainatorAPI.DTO.OpenRouter;
using ComplainatorAPI.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace ComplainatorAPI.Tests.Services;

public class AISuggestionServiceTests
{
    private IOpenRouterService _openRouterService = null!;
    private ILogger<AISuggestionService> _logger = null!;
    private AISuggestionService _sut = null!;

    [SetUp]
    public void Setup()
    {
        _openRouterService = Substitute.For<IOpenRouterService>();
        _logger = Substitute.For<ILogger<AISuggestionService>>();
        _sut = new AISuggestionService(_openRouterService, _logger);
    }

    [Test]
    public async Task GenerateAsync_WithValidNotes_ReturnsExpectedSuggestions()
    {
        // Arrange
        var notes = new List<Note>
        {
            new() { Category = NoteCategory.Success, Content = "Team collaboration improved" },
            new() { Category = NoteCategory.ImprovementArea, Content = "Sprint planning needs better estimation" }
        };

        var mockResponse = CreateJsonResponse(@"{
            ""choices"": [{
                ""message"": {
                    ""content"": ""* Implement planning poker for better estimation accuracy\n* Set up weekly team building activities\n* Create a sprint planning checklist""
                }
            }]
        }");

        _openRouterService.SendChatAsync(
            Arg.Any<IEnumerable<MessageDto>>(),
            Arg.Any<string>(),
            Arg.Any<IDictionary<string, object>>()
        ).Returns(mockResponse);

        // Act
        var result = await _sut.GenerateAsync(notes);

        // Assert
        result.Should().HaveCount(3);
        result.Should().Contain("Implement planning poker for better estimation accuracy");
        result.Should().Contain("Set up weekly team building activities");
        result.Should().Contain("Create a sprint planning checklist");
    }

    [Test]
    public async Task GenerateAsync_FormatsNotesCorrectly()
    {
        // Arrange
        var notes = new List<Note>
        {
            new() { Category = NoteCategory.Success, Content = "Test success" },
            new() { Category = NoteCategory.ImprovementArea, Content = "Test improvement" }
        };

        var mockResponse = CreateSuccessResponse(new[] { "Sample suggestion" });

        _openRouterService.SendChatAsync(
            Arg.Any<IEnumerable<MessageDto>>(),
            Arg.Any<string>(),
            Arg.Any<IDictionary<string, object>>()
        ).Returns(mockResponse);

        // Act
        await _sut.GenerateAsync(notes);

        // Assert
        await _openRouterService.Received(1).SendChatAsync(
            Arg.Is<IEnumerable<MessageDto>>(messages =>
                messages.ElementAt(1).Content.Contains("What went well:") &&
                messages.ElementAt(1).Content.Contains("- Test success") &&
                messages.ElementAt(1).Content.Contains("What needs improvement:") &&
                messages.ElementAt(1).Content.Contains("- Test improvement")
            ),
            Arg.Any<string>(),
            Arg.Any<IDictionary<string, object>>()
        );
    }

    [Test]
    public async Task GenerateAsync_WhenOpenRouterReturnsError_ThrowsInvalidOperationException()
    {
        // Arrange
        var errorResponse = CreateJsonResponse(@"{
            ""error"": {
                ""message"": ""API Error""
            }
        }");

        _openRouterService.SendChatAsync(
            Arg.Any<IEnumerable<MessageDto>>(),
            Arg.Any<string>(),
            Arg.Any<IDictionary<string, object>>()
        ).Returns(errorResponse);

        // Act & Assert
        var act = () => _sut.GenerateAsync(new List<Note>());
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("OpenRouter API error: API Error");
    }

    [Test]
    public async Task GenerateAsync_WhenEmptyResponse_ThrowsInvalidOperationException()
    {
        // Arrange
        var emptyResponse = CreateJsonResponse(@"{
            ""choices"": [{
                ""message"": {
                    ""content"": """"
                }
            }]
        }");

        _openRouterService.SendChatAsync(
            Arg.Any<IEnumerable<MessageDto>>(),
            Arg.Any<string>(),
            Arg.Any<IDictionary<string, object>>()
        ).Returns(emptyResponse);

        // Act & Assert
        var act = () => _sut.GenerateAsync(new List<Note>());
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Empty response from OpenRouter API");
    }

    [Test]
    public async Task GenerateAsync_SendsCorrectParameters()
    {
        // Arrange
        var notes = new List<Note>();
        var mockResponse = CreateSuccessResponse(new[] { "Sample suggestion" });

        _openRouterService.SendChatAsync(
            Arg.Any<IEnumerable<MessageDto>>(),
            Arg.Any<string>(),
            Arg.Is<IDictionary<string, object>>(parameters =>
                parameters["temperature"].Equals(0.7) &&
                parameters["max_tokens"].Equals(2000)
            )
        ).Returns(mockResponse);

        // Act
        await _sut.GenerateAsync(notes);

        // Assert
        await _openRouterService.Received(1).SendChatAsync(
            Arg.Any<IEnumerable<MessageDto>>(),
            Arg.Any<string>(),
            Arg.Is<IDictionary<string, object>>(parameters =>
                parameters["temperature"].Equals(0.7) &&
                parameters["max_tokens"].Equals(2000)
            )
        );
    }

    [Test]
    public async Task GenerateAsync_ProcessesSuggestionsCorrectly()
    {
        // Arrange
        var mockResponse = CreateJsonResponse(@"{
            ""choices"": [{
                ""message"": {
                    ""content"": ""* First suggestion\n  * Second suggestion\n* Third suggestion with extra spaces""
                }
            }]
        }");

        _openRouterService.SendChatAsync(
            Arg.Any<IEnumerable<MessageDto>>(),
            Arg.Any<string>(),
            Arg.Any<IDictionary<string, object>>()
        ).Returns(mockResponse);

        // Act
        var result = await _sut.GenerateAsync(new List<Note>());

        // Assert
        result.Should().HaveCount(3);
        result.Should().OnlyContain(s => !s.Contains("*"));
        result.Should().OnlyContain(s => !string.IsNullOrWhiteSpace(s));
        result.Should().Contain("First suggestion");
        result.Should().Contain("Second suggestion");
        result.Should().Contain("Third suggestion with extra spaces");
    }

    [Test]
    public async Task GenerateAsync_WithEmptyNotesList_StillFormatsCorrectly()
    {
        // Arrange
        var emptyNotes = new List<Note>();
        var mockResponse = CreateSuccessResponse(new[] { "Sample suggestion" });

        _openRouterService.SendChatAsync(
            Arg.Any<IEnumerable<MessageDto>>(),
            Arg.Any<string>(),
            Arg.Any<IDictionary<string, object>>()
        ).Returns(mockResponse);

        // Act
        await _sut.GenerateAsync(emptyNotes);

        // Assert
        await _openRouterService.Received(1).SendChatAsync(
            Arg.Is<IEnumerable<MessageDto>>(messages =>
                messages.ElementAt(1).Content.Contains("- No notes in this category")
            ),
            Arg.Any<string>(),
            Arg.Any<IDictionary<string, object>>()
        );
    }

    [Test]
    public async Task GenerateAsync_WhenOpenRouterThrowsException_PropagatesException()
    {
        // Arrange
        _openRouterService.SendChatAsync(
            Arg.Any<IEnumerable<MessageDto>>(),
            Arg.Any<string>(),
            Arg.Any<IDictionary<string, object>>()
        ).Returns(Task.FromException<object>(new Exception("API connection error")));

        // Act & Assert
        var act = () => _sut.GenerateAsync(new List<Note>());
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("API connection error");
    }

    private static object CreateJsonResponse(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }

    private static object CreateSuccessResponse(string[] suggestions)
    {
        var content = string.Join("\n", suggestions.Select(s => $"* {s}"));
        return CreateJsonResponse($@"{{
            ""choices"": [{{
                ""message"": {{
                    ""content"": ""{content}""
                }}
            }}]
        }}");
    }
}