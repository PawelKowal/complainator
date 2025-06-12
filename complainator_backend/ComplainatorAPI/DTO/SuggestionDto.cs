using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using ComplainatorAPI.Domain.Entities;

namespace ComplainatorAPI.DTO
{
    public class GenerateSuggestionsResponse
    {
        public List<SuggestionDto> Suggestions { get; set; } = new();
    }

    public class SuggestionDto
    {
        public Guid Id { get; set; }
        public string SuggestionText { get; set; } = string.Empty;
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public SuggestionStatus Status { get; set; }
    }

    public class UpdateSuggestionRequest
    {
        [Required]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public SuggestionStatus Status { get; set; }
    }
} 