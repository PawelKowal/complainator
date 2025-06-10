# API Endpoint Implementation Plan: POST /retrospectives/{id}/generate-suggestions

## 1. Przegląd punktu końcowego

Celem endpointa jest wygenerowanie sugestii AI na podstawie zapisanych notatek (notes) dla retrospektywy należącej do uwierzytelnionego użytkownika. Sugestie są przechowywane w bazie danych ze statusem `pending` i zwracane w odpowiedzi.

## 2. Szczegóły żądania

- Metoda HTTP: POST
- Struktura URL: `/retrospectives/{id}/generate-suggestions`
- Nagłówki:
  - `Authorization: Bearer <token>` (wymagane)
- Parametry ścieżki:
  - `id` (UUID, wymagane) – identyfikator retrospektywy
- Body: brak (puste JSON `{}` lub brak ciała)

## 3. Wykorzystywane typy

- Domain.Entities.Retrospective
- Domain.Entities.Note
- Domain.Entities.Suggestion (nowe wpisy)
- Domain.Entities.SuggestionStatus (`Pending`, `Accepted`, `Rejected`)
- DTO.GenerateSuggestionsResponse (istnieje)

## 4. Szczegóły odpowiedzi

- Kod 200 OK:
  ```json
  {
    "suggestions": [
      { "id": "<UUID>", "suggestionText": "...", "status": "Pending" },
      ...
    ]
  }
  ```
- Błędy:
  - 401 Unauthorized – brak lub nieważny token
  - 404 Not Found – retrospektywa nie istnieje lub nie należy do użytkownika
  - 500 Internal Server Error – błąd podczas wywołania serwisu AI lub problem z bazą danych

## 5. Przepływ danych

1. Kontroler odbiera żądanie, wyciąga `userId` z tokena JWT.
2. Walidacja i autoryzacja: upewnienie się, że retrospektywa o podanym `id` istnieje i należy do `userId`.
3. Pobranie wszystkich notatek (`Note`) dla tej retrospektywy.
4. Sprawdzenie istnienia oczekujących (`Pending`) sugestii:
   - jeśli istnieją, zwrócić je (idempotencja)
   - w przeciwnym razie:
     1. Przekazanie notatek do zamockowanego serwisu AI (np. `IAISuggestionService.GenerateAsync(notes)`)
     2. Dla każdej otrzymanej sugestii:
        - Utworzyć `Suggestion` z nowym `Id`, `RetrospectiveId`, `SuggestionText`, `Status = Pending`, `CreatedAt = DateTime.UtcNow`
        - Dodać do DbContext
     3. Zapis w bazie (`SaveChangesAsync`)
5. Mapowanie encji `Suggestion` na `SuggestionDto` (z uwzględnieniem `Status`).
6. Zwrócenie `GenerateSuggestionsResponse`.

## 6. Względy bezpieczeństwa

- ACL: upewnić się, że użytkownik może operować wyłącznie na własnych retrospektywach.
- Weryfikacja formatu GUID parametru `id` (automatycznie przez model binding).
- Sanityzacja/filtracja danych wejściowych (notatek) przed wysłaniem do AI.
- Rate limiting dla endpointa generacji, żeby zapobiec nadmiernym wywołaniom.

## 7. Obsługa błędów

- 401: gdy `User.FindFirstValue(ClaimTypes.NameIdentifier)` zwróci null/empty.
- 404: gdy \_retrospectiveService.GenerateSuggestionsAsync zwróci null (retrospektywa nieznaleziona).
- 500:
  - błąd połączenia z AI (np. timeout, wyjątek `AIServiceException`)
  - wyjątek podczas dostępu do bazy danych
  - logować wyjątek z pełnym stack trace i zwrócić ujednoliconą strukturę błędu `{ message: string }`.

## 8. Rozważania dotyczące wydajności

- Idempotencja: zwracanie istniejących sugestii pozwala uniknąć niepotrzebnych kosztów AI.
- Bulk delete + bulk insert dla sugestii zamiast pojedynczych operacji.
- Asynchroniczne wywołanie serwisu AI z timeoutem.
- Indeks na kolumnie `RetrospectiveId` w tabeli `Suggestions` dla szybszego wyszukiwania.

## 9. Kroki implementacji

1. Rozszerzyć `IRetrospectiveService` o metodę:
   ```csharp
   Task<GenerateSuggestionsResponse?> GenerateSuggestionsAsync(Guid userId, Guid retrospectiveId);
   ```
2. Utworzyć/nadpisać implementację w `RetrospectiveService`:
   - Walidacja istnienia i własności retrospektywy.
   - Pobranie notatek.
   - Idempotencja oczekujących sugestii.
   - Wywołanie zamockowanego serwisu AI.
   - Usunięcie/persistencja nowych sugestii.
   - Zapisanie zmian i mapowanie do DTO.
3. Dodać mock usługi AI `IAISuggestionService`, z metodą `GenerateAsync(IEnumerable<Note> notes)`.
4. Zarejestrować serwis AI w `Program.cs` (DI).
5. Zmodyfikować `RetrospectivesController`:
   - Dodać endpoint `[HttpPost("{id}/generate-suggestions")]`.
   - Wywołać `_retrospectiveService.GenerateSuggestionsAsync(...)`.
   - Obsłużyć 401, 404, 500 z odpowiednimi `StatusCode` i logowaniem.
