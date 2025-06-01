import type { NoteDto } from "./NoteDto";
import type { SuggestionListItem } from "./SuggestionDto";

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
  acceptedSuggestions: SuggestionListItem[];
}

export interface CreateRetrospectiveResponse {
  id: string; // Guid -> string
  name: string;
  date: string; // DateTime -> string (ISO 8601)
  acceptedCount: number;
  rejectedCount: number;
}

export interface RetrospectiveDetailResponse {
  id: string; // Guid -> string
  name: string;
  date: string; // DateTime -> string (ISO 8601)
  notes: RetrospectiveNotes;
  acceptedSuggestions: SuggestionListItem[];
}

export interface RetrospectiveNotes {
  improvementArea: NoteDto[];
  observation: NoteDto[];
  success: NoteDto[];
}
