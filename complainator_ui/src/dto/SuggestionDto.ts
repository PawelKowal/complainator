export type SuggestionStatus = "Pending" | "Accepted" | "Rejected";

export interface GenerateSuggestionsResponse {
  suggestions: SuggestionDto[];
}

export interface SuggestionDto {
  id: string; // Guid -> string
  suggestionText: string;
  status: SuggestionStatus;
}

export interface UpdateSuggestionRequest {
  status: SuggestionStatus;
}
