# API Endpoint Implementation Plan: PATCH /suggestions/{suggestionId}

## 1. Przegląd punktu końcowego

Endpoint pozwala użytkownikowi na aktualizację statusu pojedynczej sugestii wygenerowanej dla retrospektywy. Użytkownik może oznaczyć sugestię jako zaakceptowaną lub odrzuconą, co wpływa na liczniki `AcceptedCount` i `RejectedCount` w powiązanej retrospektywie.

## 2. Szczegóły żądania

- Metoda HTTP: PATCH
- Struktura URL: `/suggestions/{suggestionId}`
- Parametry:
  - Wymagane:
    - `suggestionId` (UUID) – identyfikator sugestii przekazywany w ścieżce URL
  - Opcjonalne: brak
- Request Body:
  ```json
  {
    "status": "accepted" | "rejected"
  }
  ```
  - Pole `status` jest wymagane. Wartości dopuszczalne: `accepted`, `rejected`.
  - Walidacja: DTO `UpdateSuggestionRequest` z atrybutem `[Required]` i `JsonStringEnumConverter` obsługującym enum `SuggestionStatus`.

## 3. Wykorzystywane typy

- ComplainatorAPI.DTO.UpdateSuggestionRequest
- ComplainatorAPI.Domain.Entities.Suggestion
- ComplainatorAPI.Domain.Entities.SuggestionStatus (Pending, Accepted, Rejected)

## 4. Szczegóły odpowiedzi

- Sukces:
  - Kod statusu: 204 No Content
  - Brak ciała odpowiedzi
- Błędy:
  - 400 Bad Request – niepoprawny JSON, brak pola `status`, nieprawidłowa wartość enum
  - 401 Unauthorized – brak lub nieważny token JWT
  - 404 Not Found – brak sugestii o podanym `suggestionId` lub sugestia nie należy do użytkownika
  - 500 Internal Server Error – nieoczekiwany błąd po stronie serwera

## 5. Przepływ danych

1. Kontroler `SuggestionsController` (nowy) przyjmuje żądanie.
2. Wyciągnięcie `userId` z tokena JWT (ClaimTypes.NameIdentifier).
3. Deserializacja i wstępna walidacja `UpdateSuggestionRequest`.
4. Wywołanie serwisu biznesowego (`ISuggestionService.UpdateStatusAsync(userId, suggestionId, request.Status)`).
5. W serwisie:
   1. Odczyt sugestii z bazy (`DbContext.Suggestions.Include(r => r.Retrospective)`).
   2. Weryfikacja własności (suggestion.Retrospective.UserId == userId).
   3. Sprawdzenie zmiany statusu:
      - Jeśli nowy status == aktualny: brak operacji (idempotentnie zwrócić sukces)
      - Jeśli `accepted`: ustaw `Suggestion.Status = Accepted` i inkrementuj `Retrospective.AcceptedCount`.
      - Jeśli `rejected`: ustaw `Suggestion.Status = Rejected` i inkrementuj `Retrospective.RejectedCount`.
   4. Zapis `DbContext.SaveChangesAsync()`.
6. Kontroler zwraca `NoContent()`.

## 6. Względy bezpieczeństwa

- Uwierzytelnienie JWT ([Authorize] na kontrolerze).
- Autoryzacja: weryfikacja, czy `Retrospective.UserId` odpowiada `userId` z tokena.
- Unikać ujawniania szczegółów wewnętrznych w komunikatach błędów.
- JSON enum converter `JsonStringEnumConverter` zabezpiecza przed niebezpiecznymi danymi.

## 7. Obsługa błędów

| Scenariusz                                          | Kod | Odpowiedź                                   |
| --------------------------------------------------- | --- | ------------------------------------------- |
| Brak pola `status`                                  | 400 | Standardowy model błędu walidacji           |
| Niepoprawna wartość enum                            | 400 | Standardowy model błędu walidacji           |
| Brak JWT lub nieważny token                         | 401 | Pusta odpowiedź lub { message: ... }        |
| Sugestia nie istnieje lub nie należy do użytkownika | 404 | `{ message: "Suggestion not found" }`       |
| Błąd zapisu do bazy / wyjątek serwera               | 500 | `{ message: "An internal error occurred" }` |

## 8. Wydajność

- Jeden prosty odczyt i zapis w ramach transakcji EF Core.
- Indeks na `Suggestions.Id` i `Suggestions.RetrospectiveId` przyspiesza odczyt.
- Minimalne obciążenie – operacja aktualizacji wiersza.

## 9. Kroki implementacji

1. Utworzyć interfejs `ISuggestionService` z metodą `Task<bool> UpdateStatusAsync(Guid userId, Guid suggestionId, SuggestionStatus status)`.
2. Dodać implementację `SuggestionService`:
   - Wstrzyknąć `ApplicationDbContext` i `ILogger<SuggestionService>`.
   - Zaimplementować logikę sprawdzania i aktualizacji statusu oraz liczników retrospektywy.
3. Zaregisterować serwis w DI w `ServiceCollectionExtensions`.
4. Utworzyć `SuggestionsController`:
   - Oznaczyć `[ApiController]`, `[Route("suggestions")]`, `[Authorize]`, `[ModelStateValidation]`.
   - Dodać metodę:
     ```csharp
     [HttpPatch("{suggestionId}")]
     [ProducesResponseType(StatusCodes.Status204NoContent)]
     [ProducesResponseType(StatusCodes.Status400BadRequest)]
     [ProducesResponseType(StatusCodes.Status401Unauthorized)]
     [ProducesResponseType(StatusCodes.Status404NotFound)]
     public async Task<IActionResult> UpdateStatus(Guid suggestionId, [FromBody] UpdateSuggestionRequest request) { ... }
     ```
5. W metodzie kontrolera:
   - Wyciągnąć `userId` z `User.FindFirstValue(...)`.
   - Wywołać `UpdateStatusAsync` i w zależności od wyniku zwrócić `NoContent()` lub `NotFound()`.
