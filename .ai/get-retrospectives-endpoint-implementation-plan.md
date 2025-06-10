# API Endpoint Implementation Plan: GET /retrospectives

## 1. Przegląd punktu końcowego

Punkt końcowy `GET /retrospectives` umożliwia uwierzytelnionemu użytkownikowi pobranie spersonalizowanej, stronicowanej i posortowanej listy swoich retrospektyw.

## 2. Szczegóły żądania

- Metoda HTTP: GET
- URL: `/retrospectives`
- Parametry zapytania:
  - Wymagane: brak
  - Opcjonalne:
    - `page` (integer, ≥1, domyślnie 1)
    - `perPage` (integer, ≥1, domyślnie 10)
    - `sort` (string, enum: `date_desc` lub `date_asc`, domyślnie `date_desc`)
- Body: brak

## 3. Wykorzystywane typy

- DTO:
  - `RetrospectiveListRequest` (Page, PerPage, Sort)
  - `RetrospectiveListResponse` (Items, Total, Page, PerPage)
  - `RetrospectiveListItem` (Id, Name, Date, List<SuggestionListItem>)
  - `SuggestionListItem` (Id, SuggestionText)
- Command Models: brak (reusing `RetrospectiveListRequest`)

## 4. Szczegóły odpowiedzi

- **200 OK**

```json
{
  "items": [
    {
      "id": "<UUID>",
      "name": "Retrospektywa #3 - 01.06.2025",
      "date": "2025-06-01",
      "acceptedSuggestions": [
        { "id": "<UUID>", "suggestionText": "Poprawić dokumentację API" }
      ]
    }
  ],
  "total": 42,
  "page": 1,
  "perPage": 10
}
```

- **400 Bad Request** – nieprawidłowe parametry `page`, `perPage` lub `sort`
- **401 Unauthorized** – brak lub nieważny token JWT
- **500 Internal Server Error** – nieoczekiwany błąd po stronie serwera

## 5. Przepływ danych

1. Klient wysyła żądanie `GET /retrospectives?page=&perPage=&sort=` z nagłówkiem `Authorization: Bearer <token>`.
2. ASP.NET Core weryfikuje token JWT i ustawia `User` w kontekście HTTP.
3. W akcji kontrolera `GetList([FromQuery] RetrospectiveListRequest request)`:
   - Wyciągnięcie `userId` z `User.FindFirstValue(ClaimTypes.NameIdentifier)`.
   - Walidacja `request` (guard clauses lub ModelState).
   - Wywołanie `_retrospectiveService.GetListAsync(userId, request)`.
4. W `RetrospectiveService.GetListAsync`:
   - Filtr `dbContext.Retrospectives.Where(r => r.UserId == userId)`.
   - Pobranie `total = CountAsync()`.
   - Sortowanie wg `request.Sort` (`OrderByDescending(r => r.Date)` lub `OrderBy(r => r.Date)`).
   - Stronicowanie: `Skip((Page-1)*PerPage).Take(PerPage)`.
   - Projekcja do `RetrospectiveListItem`:
     - Id, Name, Date
     - `AcceptedSuggestions`: `r.Suggestions.Where(s => s.Status == SuggestionStatus.Accepted)`
       .Select(s => new `SuggestionListItem`)
   - Zwrócenie `RetrospectiveListResponse`.
5. Kontroler zwraca `Ok(response)`.

## 6. Względy bezpieczeństwa

- Uwierzytelnianie: `[Authorize]` na kontrolerze wymusza obecność i ważność tokena JWT.
- Autoryzacja: filtrowanie `Retrospectives` po `UserId` zapewnia dostęp tylko do własnych danych.
- Walidacja wejścia: zapobiega atakom typu parameter tampering.
- Projekcja danych: zwracane tylko niezbędne pola.
- EF Core zabezpiecza przed SQL injection.

## 7. Obsługa błędów

- **400 Bad Request**: użycie `ModelState.IsValid` lub ręczna walidacja `page < 1` / `perPage < 1` / nieobsługiwany `sort` → `BadRequest(ModelState)` lub `BadRequest(new { message = ... })`.
- **401 Unauthorized**: brak lub nieważny JWT (obsłużone przez middleware).
- **500 Internal Server Error**:
  - W serwisie catch(Exception ex) → `_logger.LogError(ex, "Error fetching retrospectives for user {UserId}", userId);` → rethrow lub zwróć `StatusCode(500)`.

## 8. Rozważania dotyczące wydajności

- Indeks na kolumnie `UserId` (już istnieje) przyspiesza filtrowanie.
- Stronicowanie (`Skip`/`Take`) ogranicza liczbę załadowanych rekordów.
- Projekcja tylko potrzebnych kolumn i relacji.
- Unikaj N+1: używaj `Include` lub direct projection z nawigacji `Stories`.

## 9. Kroki implementacji

1. **Interfejs serwisu**: w `IRetrospectiveService` dodaj:
   ```csharp
   Task<RetrospectiveListResponse> GetListAsync(Guid userId, RetrospectiveListRequest request);
   ```
2. **Serwis**: w `RetrospectiveService` zaimplementuj `GetListAsync` zgodnie z przepływem danych.
3. **Kontroler**: w `RetrospectivesController` stwórz akcję:
   ```csharp
   [HttpGet]
   [ProducesResponseType(typeof(RetrospectiveListResponse), StatusCodes.Status200OK)]
   [ProducesResponseType(StatusCodes.Status400BadRequest)]
   [ProducesResponseType(StatusCodes.Status401Unauthorized)]
   public async Task<IActionResult> GetList([FromQuery] RetrospectiveListRequest request)
   {
       var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
       if (string.IsNullOrEmpty(userId)) return Unauthorized();
       if (!ModelState.IsValid) return BadRequest(ModelState);
       var response = await _retrospectiveService.GetListAsync(Guid.Parse(userId), request);
       return Ok(response);
   }
   ```
4. **Walidacja**: dodaj do `RetrospectiveListRequest` atrybuty `[Range(1, int.MaxValue)]` na `Page` i `PerPage` oraz `[EnumDataType(typeof(SortOrder))]` na `Sort`.
