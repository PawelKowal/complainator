using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using ComplainatorAPI.Domain.Entities;

namespace ComplainatorAPI.DTO
{
    public class CreateNoteRequest
    {
        [Required]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public NoteCategory Category { get; set; }

        [Required]
        [StringLength(1000, MinimumLength = 1, ErrorMessage = "Content cannot be empty and must be less than 1000 characters")]
        public string Content { get; set; } = string.Empty;
    }

    public class CreateNoteResponse
    {
        public Guid Id { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public NoteCategory Category { get; set; }

        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}