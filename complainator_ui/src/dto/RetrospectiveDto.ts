import type { NoteDto } from "./NoteDto";
import type { SuggestionDto } from "./SuggestionDto";

export type SortOrder = "dateDesc" | "dateAsc";

export interface RetrospectiveListRequest {
  page: number;
  perPage: number;
  sort: SortOrder;
}

export interface RetrospectiveListResponse {
  items: RetrospectiveListItem[];
  total: number;
  page: number;
  perPage: number;
}

export interface RetrospectiveListItem {
  id: string; // Guid -> string
  name: string;
  date: string; // DateTime -> string (ISO 8601)
  acceptedSuggestions: SuggestionDto[];
}

export interface CreateRetrospectiveResponse {
  id: string; // Guid -> string
  name: string;
  date: string; // DateTime -> string (ISO 8601)
}

export interface RetrospectiveDetailResponse {
  id: string; // Guid -> string
  name: string;
  date: string; // DateTime -> string (ISO 8601)
  notes: RetrospectiveNotes;
  suggestions: SuggestionDto[];
}

export interface RetrospectiveNotes {
  improvementArea: NoteDto[];
  observation: NoteDto[];
  success: NoteDto[];
}
