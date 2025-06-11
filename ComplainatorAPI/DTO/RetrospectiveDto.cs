using System;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using ComplainatorAPI.Domain.Entities;

namespace ComplainatorAPI.DTO
{
    public class RetrospectiveListRequest
    {
        [Range(1, int.MaxValue, ErrorMessage = "Page must be greater than 0")]
        public int Page { get; set; } = 1;

        [Range(1, int.MaxValue, ErrorMessage = "PerPage must be greater than 0")]
        public int PerPage { get; set; } = 10;
        
        [JsonConverter(typeof(JsonStringEnumConverter))]
        [EnumDataType(typeof(SortOrder), ErrorMessage = "Invalid sort order")]
        public SortOrder Sort { get; set; } = SortOrder.DateDesc;
    }

    public enum SortOrder
    {
        DateDesc,
        DateAsc
    }

    public class RetrospectiveListResponse
    {
        public List<RetrospectiveListItem> Items { get; set; } = new();
        public int Total { get; set; }
        public int Page { get; set; }
        public int PerPage { get; set; }
    }

    public class RetrospectiveListItem
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public List<SuggestionDto> Suggestions { get; set; } = new();
    }

    public class CreateRetrospectiveResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime Date { get; set; }
    }

    public class RetrospectiveDetailResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public RetrospectiveNotes Notes { get; set; } = new();
        public List<SuggestionDto> Suggestions { get; set; } = new();
    }

    public class RetrospectiveNotes
    {
        public List<NoteDto> ImprovementArea { get; set; } = new();
        public List<NoteDto> Observation { get; set; } = new();
        public List<NoteDto> Success { get; set; } = new();
    }

    public class NoteDto
    {
        public Guid Id { get; set; }
        public string Content { get; set; } = string.Empty;
    }
} 