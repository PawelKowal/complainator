export type SuggestionStatus = "pending" | "accepted" | "rejected";

export interface GenerateSuggestionsResponse {
  suggestions: SuggestionDto[];
}

export interface SuggestionDto {
  id: string; // Guid -> string
  suggestionText: string;
}

export interface SuggestionListItem {
  id: string; // Guid -> string
  suggestionText: string;
}

export interface UpdateSuggestionRequest {
  status: SuggestionStatus;
}
