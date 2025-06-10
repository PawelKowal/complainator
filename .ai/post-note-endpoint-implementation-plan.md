# API Endpoint Implementation Plan: POST /retrospectives/{id}/notes

## 1. Przegląd punktu końcowego

Endpoint umożliwia uwierzytelnionemu użytkownikowi dodanie nowej notatki (`Note`) do istniejącej retrospektywy (`Retrospective`). Użytkownik otrzymuje w odpowiedzi utworzoną notatkę z jej identyfikatorem oraz znacznikiem czasu.

## 2. Szczegóły żądania

- Metoda HTTP: POST
- Struktura URL: `/retrospectives/{id}/notes`
- Parametry ścieżki:
  - `id` (UUID) – identyfikator retrospektywy, do której dodajemy notatkę
- Parametry zapytania: brak
- Nagłówki:
  - `Authorization: Bearer <token>` (JWT, wymagany)
- Body (JSON):
  ```json
  {
    "category": "ImprovementArea" | "Observation" | "Success",
    "content": "string (1–1000 znaków)"
  }
  ```
  - `category` (string) – wymagana, wartości zgodne z `NoteCategory`
  - `content` (string) – wymagana, niepusta, maks. 1000 znaków

## 3. Szczegóły odpowiedzi

- Kod statusu: **201 Created**
- Body (JSON):
  ```json
  {
    "id": "<UUID>",
    "category": "ImprovementArea",
    "content": "string",
    "createdAt": "2025-06-10T12:34:56Z"
  }
  ```
- Kody błędów:
  - **400 Bad Request** – niepoprawne dane wejściowe (ModelState)
  - **401 Unauthorized** – brak/nieprawidłowy token JWT
  - **404 Not Found** – retrospektywa nie istnieje lub nie należy do użytkownika
  - **500 Internal Server Error** – nieprzewidziany błąd serwera

## 4. Przepływ danych

1. Klient wysyła żądanie POST z JWT i danymi notatki.
2. Kontroler (`RetrospectivesController`) z atrybutem `[Authorize]` przyjmuje `id` oraz DTO `CreateNoteRequest`.
3. Akcja weryfikuje `ModelState`; w przypadku błędów zwraca 400.
4. Wyciągnięcie `userId` z tokenu (`HttpContext.User`).
5. Wywołanie serwisowej metody `AddNoteAsync(userId, retrospectiveId, request)`.
6. W serwisie:
   - Pobranie retrospektywy z bazy przez `DbContext`, weryfikacja własności (userId).
   - Jeśli brak → zwrócenie null lub rzucenie `NotFoundException`.
   - Utworzenie obiektu `Note { Id = Guid.NewGuid(), RetrospectiveId, Category, Content, CreatedAt = DateTime.UtcNow }`.
   - Dodanie do `DbContext.Notes` i `SaveChangesAsync()`.
   - Mapowanie do `CreateNoteResponse` (uwzględnić `CreatedAt`).
7. Kontroler otrzymuje DTO, zwraca **201 Created** z odpowiednim JSON.

## 5. Względy bezpieczeństwa

- Autoryzacja za pomocą `[Authorize]` i JWT Bearer.
- Weryfikacja własności zasobu: retrospektywa musi należeć do zalogowanego użytkownika.
- Ograniczenie długości `content` (1–1000 znaków) i pożądane sanitowanie ewentualnych znaków specjalnych.
- Użycie EF Core zapobiega SQL Injection.

## 6. Obsługa błędów

- **400** – `return BadRequest(ModelState);` przy nieudanej walidacji DTO.
- **401** – zwrócone automatycznie przez `[Authorize]`, brak dostępu.
- **404** – `return NotFound();` gdy retrospektywa nie istnieje lub userId się nie zgadza.
- **500** – globalny middleware lub `catch (Exception ex)` w serwisie, logowanie (`ILogger`/`AuditLog`), `return StatusCode(500, "Internal server error");`.

## 7. Rozważania dotyczące wydajności

- Asynchroniczne operacje (`SaveChangesAsync`).
- Indeksowanie kolumny `RetrospectiveId` w tabeli `Notes`.
- Minimalizacja rozmiaru DTO i ograniczenie ładunku danych.
- Monitorowanie opóźnień bazy i retry policy w razie potrzeby.

## 8. Kroki implementacji

1. Rozszerzyć DTO:
   - Dodać pole `DateTime CreatedAt { get; set; }` do `CreateNoteResponse`.
2. Zaktualizować `IRetrospectiveService`:
   ```csharp
   Task<CreateNoteResponse> AddNoteAsync(Guid userId, Guid retrospectiveId, CreateNoteRequest request);
   ```
3. Dodać implementację `AddNoteAsync` w `RetrospectiveService`:
   - Pobranie i weryfikacja retrospektywy.
   - Utworzenie i zapis `Note`.
   - Mapowanie na `CreateNoteResponse`.
4. W `RetrospectivesController` dodać akcję:
   ```csharp
   [HttpPost("{id}/notes")]
   public async Task<IActionResult> AddNote(Guid id, [FromBody] CreateNoteRequest request)
   {
       if (!ModelState.IsValid) return BadRequest(ModelState);
       var userId = GetUserId();
       var result = await _retrospectiveService.AddNoteAsync(userId, id, request);
       if (result == null) return NotFound();
       return StatusCode(201, result);
   }
   ```
5. Dodać logowanie w serwisie i globalny middleware do `AuditLog`.
