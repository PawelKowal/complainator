export type NoteCategory = "improvementArea" | "observation" | "success";

export interface CreateNoteRequest {
  category: NoteCategory;
  content: string;
}

export interface CreateNoteResponse {
  id: string; // Guid -> string
  category: NoteCategory;
  content: string;
}

export interface NoteDto {
  id: string; // Guid -> string
  content: string;
}
