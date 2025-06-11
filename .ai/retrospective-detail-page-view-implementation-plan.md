# Plan implementacji widoku RetrospectiveDetailPage

## 1. Przegląd

Widok szczegółowy retrospektywy pozwala na: wyświetlenie trzech kategorii notatek („Co wymaga poprawy”, „Obserwacje”, „Co poszło dobrze”), dodawanie nowych notatek, generowanie sugestii AI, przeglądanie oraz akceptację lub odrzucanie sugerowanych wniosków.

## 2. Routing widoku

Ścieżka: `/retrospectives/:id`
Komponent montowany w ramach `PrivateRoute` i `PrivateLayout`.

## 3. Struktura komponentów

- `RetrospectiveDetailPage`
  - `Header` (nawigacja i przyciski globalne)
  - `ErrorAlert` (globalne błędy lokalne)
  - `LoadingSkeleton` (szkielet podczas ładowania)
  - `Grid` MUI 3-kolumnowy dla `NoteColumn`
    - `NoteColumn` ×3 (kategorie)
      - `AddNoteButton`
  - `AddNoteDialog`
  - `GenerateSuggestionsButton`
  - `SuggestionsList`

## 4. Szczegóły komponentów

### RetrospectiveDetailPage

- Opis: główny kontener, odpowiedzialny za pobranie danych i orchestrację akcji.
- Główne elementy:
  - `useQuery` do GET `/retrospectives/{id}`
  - Sekcja ładowania (`LoadingSkeleton`) i błędów (`ErrorAlert`).
- Interakcje:
  - Obsługa otwierania dialogu dodawania notatki (`setDialogOpen`, `dialogCategory`).
  - Wywołanie mutacji `addNote` i odświeżenie danych.
  - Wywołanie mutacji `generateSuggestions` i blokowanie dalszych mutacji.
  - Obsługa akceptacji/odrzucenia (`updateSuggestion`) i odświeżenie.
- Walidacja:
  - Sprawdzanie statusu `isLoading`, `isError`.
  - Wyłączanie przycisków jeśli istnieją już sugestie (idempotency).
- Typy:
  - `RetrospectiveDetailResponse`
  - `CreateNoteRequest`, `GenerateSuggestionsResponse`, `UpdateSuggestionRequest`
- Propsy: brak (sam pobiera `id` z params).

### NoteColumn

- Opis: kolumna jednej kategorii notatek.
- Główne elementy:
  - Nagłówek z tytułem (zależny od kategorii).
  - Lista `NoteItem` dla każdej notatki.
  - `AddNoteButton`.
- Interakcje:
  - Klik `AddNoteButton` → wyświetlenie `AddNoteDialog`.
- Walidacja:
  - Przycisk wyłączony, gdy wygenerowano sugestie.
- Typy:
  - `NoteDto`
- Propsy:
  - `notes: NoteDto[]`
  - `category: NoteCategory`
  - `onAdd: (category: NoteCategory) => void`

### AddNoteDialog

- Opis: modal do wprowadzenia treści notatki.
- Główne elementy:
  - `Select` lub ukryty nagłówek pokazujący `category`.
  - `TextField` na treść notatki.
  - `Button` „Zapisz” i „Anuluj”.
- Interakcje:
  - Walidacja: `content.length > 0`.
  - Po kliknięciu „Zapisz”: wywołanie `addNote` z `{ category, content }`.
  - Zamknięcie po sukcesie lub wyświetlenie błędu.
- Walidacja:
  - Brak pustej treści (min 1 znak).
- Typy:
  - `CreateNoteRequest`, `CreateNoteResponse`
- Propsy:
  - `open: boolean`, `category: NoteCategory`, `onClose: () => void`, `onSuccess: () => void`

### GenerateSuggestionsButton

- Opis: przycisk generujący sugestie AI.
- Główne elementy: `Button` MUI.
- Interakcje:
  - Klik → mutacja `generateSuggestions`.
- Walidacja:
  - Wyłączony jeśli brak notatek lub jeśli już istnieją sugerowane wnioski.
- Typy:
  - `GenerateSuggestionsResponse`
- Propsy:
  - `onGenerate: () => void`, `disabled: boolean`

### SuggestionsList

- Opis: lista sugerowanych wniosków z przyciskami akcji.
- Główne elementy:
  - Lista `SuggestionItem`.
- Interakcje:
  - Klik „Zaakceptuj”/„Odrzuć” → mutacja `updateSuggestion`.
- Typy:
  - `SuggestionDto`, `UpdateSuggestionRequest`
- Propsy:
  - `suggestions: SuggestionDto[]`, `onAction: (id: string, status: SuggestionStatus) => void`

### SuggestionItem

- Opis: pojedynczy wniosek AI.
- Główne elementy: tekst i dwa `Button` („Zaakceptuj”, „Odrzuć”).
- Interakcje:
  - Zmiana statusu → wywołanie `onAction`.
- Propsy:
  - `suggestion: SuggestionDto`, `onAction`

## 5. Typy

- RetrospectiveDetailResponse:
  - `id: string`, `name: string`, `date: string`,
  - `notes: { improvementArea: NoteDto[], observation: NoteDto[], success: NoteDto[] }`,
  - `acceptedSuggestions: SuggestionListItem[]`
- NoteDto: `{ id: string; content: string }`
- CreateNoteRequest: `{ category: NoteCategory; content: string }`
- GenerateSuggestionsResponse: `{ suggestions: SuggestionDto[] }`
- SuggestionDto: `{ id: string; suggestionText: string }`
- UpdateSuggestionRequest: `{ status: SuggestionStatus }`

## 6. Zarządzanie stanem

- Użycie React Query:
  - `useQuery(['retrospective', id], fetchDetail)`
  - `useMutation(addNote)`, `useMutation(generateSuggestions)`, `useMutation(updateSuggestion)`
- Local state w `RetrospectiveDetailPage`:
  - `dialogOpen`, `dialogCategory`,
  - `errorLocal` dla mutacji,
  - kontrola przycisków disabled.

## 7. Integracja API

- GET `/retrospectives/{id}` → `fetchDetail(id)` → zwraca `RetrospectiveDetailResponse`
- POST `/retrospectives/{id}/notes` z `CreateNoteRequest` → odśwież `['retrospective', id]`
- POST `/retrospectives/{id}/generate-suggestions` → ustawia `suggestions` w cache
- PATCH `/suggestions/{suggestionId}` z `UpdateSuggestionRequest` → odśwież `['retrospective', id]`

## 8. Interakcje użytkownika

1. Wejście na stronę → szkielet ładowania → render danych.
2. Klik „Dodaj notatkę” przy wybranej kolumnie → dialog.
3. Wpis treści → klik „Zapisz” → spinner w przycisku, zamknięcie, refresh.
4. Klik „Generuj wnioski” → spinner → pojawienie się `SuggestionsList`.
5. Klik „Zaakceptuj”/„Odrzuć” przy wniosku → usunięcie z listy lub zmiana stylu.

## 9. Warunki i walidacja

- Notatka: niepusty `content`.
- Generowanie: wymagana ≥1 notatka i brak istniejących sugestii.
- Akcje na sugestiach: dopuszczalne statusy „accepted”/„rejected”.
- Obsługa 404/401 → przekierowanie lub `ErrorAlert`.

## 10. Obsługa błędów

- Globalne: `ErrorAlert` w layoucie.
- Lokalnie: `Alert` nad listą lub w dialogu.
- Retry przy mutacjach (opcjonalnie).

## 11. Kroki implementacji

1. Stworzyć i skonfigurować mutacje React Query w hookach (useRetrospectiveDetail, addNote, generateSuggestions, updateSuggestion).
2. Dodać routing `/retrospectives/:id` w `PrivateRoutes`.
3. Utworzyć `RetrospectiveDetailPage.tsx` z `useParams` i `useRetrospectiveDetail`.
4. Stworzyć komponenty `NoteColumn`, `AddNoteDialog`, `GenerateSuggestionsButton`, `SuggestionItem`, `SuggestionsList`.
5. Dodać Loading Skeleton i ErrorAlert.
6. Zadbać o responsywność i style MUI `sx` zgodne z resztą aplikacji.
