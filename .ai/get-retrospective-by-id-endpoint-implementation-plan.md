# Plan implementacji punktu końcowego API: GET /retrospectives/{id}

## 1. Przegląd punktu końcowego

Pobranie szczegółów pojedynczej retrospektywy należącej do uwierzytelnionego użytkownika. Obejmuje id, nazwę, datę, listę notatek pogrupowanych według kategorii oraz listę zaakceptowanych sugestii.

## 2. Szczegóły żądania

- Metoda HTTP: GET
- Ścieżka: `/retrospectives/{id}`
- Parametry:
  - Wymagane:
    - `id` (GUID) – identyfikator retrospektywy
  - Opcjonalne: brak
- Body żądania: brak

## 3. Wykorzystywane typy

- DTO odpowiedzi:
  - `RetrospectiveDetailResponse`
  - `RetrospectiveNotes`
  - `NoteDto`
  - `SuggestionListItem`
- Encje w bazie:
  - `Retrospective`
  - `Note`
  - `Suggestion`
- Usługi:
  - Nowa metoda w `IRetrospectiveService`: `GetByIdAsync(Guid userId, Guid retrospectiveId)`

## 4. Szczegóły odpowiedzi

- 200 OK
  ```json
  {
    "id": "GUID",
    "name": "string",
    "date": "YYYY-MM-DDThh:mm:ssZ",
    "notes": {
      "improvementArea": [ { "id": "GUID", "content": "string" }, ... ],
      "observation":    [ { "id": "GUID", "content": "string" }, ... ],
      "success":        [ { "id": "GUID", "content": "string" }, ... ]
    },
    "acceptedSuggestions": [
      { "id": "GUID", "suggestionText": "string" }, ...
    ]
  }
  ```
- 400 Bad Request – niepoprawny GUID w ścieżce
- 401 Unauthorized – brak lub nieprawidłowy token
- 404 Not Found – brak retrospektywy lub należy do innego użytkownika
- 500 Internal Server Error – błąd serwera

## 5. Przepływ danych

1. Kontroler `RetrospectivesController.GetById(Guid id)`:

   - Odczytuje `userId` z claimu JWT.
   - Waliduje obecność claimu (jeśli brak → 401).
   - Wywołuje serwis:
     ```csharp
     var dto = await _retrospectiveService.GetByIdAsync(userGuid, id);
     ```
   - Jeśli `dto == null` → `return NotFound();`
   - W innych wyjątkach → log + `return StatusCode(500, new { message = "..." });`
   - W sukcesie → `return Ok(dto);`

2. Serwis `RetrospectiveService.GetByIdAsync(Guid userId, Guid retrospectiveId)`:

   - EF Core query:
     ```csharp
     var retrospektywa = await _dbContext.Retrospectives
       .AsNoTracking()
       .Include(r => r.Notes)
       .Include(r => r.Suggestions.Where(s => s.Status == SuggestionStatus.Accepted))
       .FirstOrDefaultAsync(r => r.Id == retrospectiveId && r.UserId == userId);
     ```
   - Jeśli null → zwraca null lub rzuca `NotFoundException`.
   - Mapowanie na `RetrospectiveDetailResponse`:
     - Grupa notatek wg `Category`.
     - Lista zaakceptowanych sugestii.
   - Zwraca DTO.

3. Repozytorium/Baza danych:
   - Tabele: `Retrospectives`, `Notes`, `Suggestions`.

## 6. Względy bezpieczeństwa

- Autoryzacja: `[Authorize]` + weryfikacja `userId`
- Zapobieganie IDOR: w zapytaniu EF filtr po `UserId`
- SQL injection: EF Core parametrów instancjonuje zapytania
- Brak ujawniania szczegółów błędów wewnętrznych w odpowiedzi

## 7. Obsługa błędów

- 400: nieudane parsowanie `id`
- 401: brak/nieprawidłowy token JWT
- 404: brak rekordu lub nie należy do użytkownika
- 500: nieprzewidziane wyjątki (baza danych, logika serwisu)
- W logach:
  - `LogWarning` przy braku claimu lub próbie dostępu do nieistniejącego zasobu
  - `LogError` przy wewnętrznych wyjątkach

## 8. Rozważania dotyczące wydajności

- Użycie `AsNoTracking()` przy odczycie dla poprawy wydajności
- Filtrowane `Include(...)` dla sugestii (zamiast ładowania wszystkich)
- Indeks na kolumnie `UserId` i `Id` w tabeli `Retrospectives`
- Brak potrzeby paginacji (pojedynczy rekord)

## 9. Etapy wdrożenia

1. Rozszerzyć `IRetrospectiveService` o metodę `GetByIdAsync(Guid userId, Guid retrospectiveId)`.
2. Zaimplementować `GetByIdAsync` w `RetrospectiveService` z EF Core + mapowaniem.
3. Dodać implementację `GetById(Guid id)` w `RetrospectivesController`:
   - Pobranie claimu, walidacja, wywołanie serwisu, obsługa null, logi, odpowiedzi.
4. Dodać ewentualne wyjątki domenowe lub zwracanie `null` dla not found.
