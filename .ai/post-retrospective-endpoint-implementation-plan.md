# API Endpoint Implementation Plan: POST /retrospectives

## 1. Przegląd punktu końcowego

Punkt końcowy służy do utworzenia nowej retrospektywy dla zalogowanego użytkownika. Nazwa retrospektywy jest generowana automatycznie w formacie `Retrospektywa #{numer} - DD.MM.YYYY`, gdzie `numer` to kolejna liczba oparta na dotychczasowej liczbie retrospektyw użytkownika, a data to bieżący dzień.

## 2. Szczegóły żądania

- Metoda HTTP: POST
- Ścieżka: `/retrospectives`
- Nagłówki:
  - `Authorization: Bearer <token>` (wymagany)
  - `Content-Type: application/json`
- Parametry URL: brak
- Query Params: brak
- Request Body: brak ciała

## 3. Wykorzystywane typy

- **DTO – odpowiedź**: `CreateRetrospectiveResponseDto`
  ```csharp
  public class CreateRetrospectiveResponseDto {
    public Guid Id { get; set; }
    public string Name { get; set; }
    public DateTime Date { get; set; }
    public int AcceptedCount { get; set; }
    public int RejectedCount { get; set; }
  }
  ```

## 3. Szczegóły odpowiedzi

- Status 201 Created
- Body:
  ```json
  {
    "id": "<UUID>",
    "name": "Retrospektywa #4 - 02.06.2025",
    "date": "2025-06-02",
    "acceptedCount": 0,
    "rejectedCount": 0
  }
  ```
- Nagłówki:
  - `Location: /retrospectives/{id}` (opcjonalnie)

## 4. Przepływ danych

1. Klient wywołuje POST `/retrospectives` z nagłówkiem Authorization.
2. Kontroler (`RetrospectivesController`) otrzymuje żądanie i wyciąga `userId` z tokenu JWT.
3. Kontroler wywołuje `IRetrospectiveService.CreateAsync(userId)`.
4. Serwis:
   - Pobiera liczbę istniejących retrospektyw użytkownika (`repository.CountByUserAsync(userId)`).
   - Generuje nazwę: `Retrospektywa #{count+1} - {DateTime.UtcNow:dd.MM.yyyy}`.
   - Tworzy instancję encji `Retrospective` z `UserId`, `Name`, `Date`.
   - Zapisuje encję w bazie (`repository.AddAsync(entity)` + `SaveChangesAsync`).
   - Mapuje encję na `CreateRetrospectiveResponseDto`.
5. Kontroler zwraca wynik z kodem 201.

## 5. Względy bezpieczeństwa

- Uwierzytelnianie: wymóg JWT Bearer i atrybut `[Authorize]` na kontrolerze/metodzie.
- Autoryzacja: tworzenie tylko dla zalogowanego użytkownika; `userId` nie może być podmieniony.
- Brak wejścia od klienta – ataki typu sql injection nie mają bezpośredniego wpływu.
- Potencjalne ryzyko wyścigu: jednoczesne wywołania mogą wygenerować tę samą nazwę – rozważyć transakcję lub blokadę optymistyczną.

## 6. Obsługa błędów

- 401 Unauthorized: gdy brak lub nieważny token.
- 500 Internal Server Error: przy nieoczekiwanym wyjątku w serwisie lub problemach z bazą.
- Logowanie błędów: używać `ILogger<RetrospectiveService>` do zapisu szczegółów wyjątków.
- Format błędu:
  ```json
  {
    "error": "Opis błędu",
    "details": "Szczegóły techniczne (opcjonalnie)"
  }
  ```

## 7. Rozważania dotyczące wydajności

- Liczenie retrospektyw użytkownika powinno być szybkie (indeks na `UserId`).
- Możliwość cachowania lub optymalizacji, gdy duża liczba retrospektyw.
- Monitorować czas zapisu do bazy.

## 8. Kroki wdrożenia

1. Dodać metodę POST `/retrospectives` w `RetrospectivesController` z `[Authorize]`.
2. Wstrzyknąć `IRetrospectiveService` w kontrolerze.
3. Zaimplementować `RetrospectiveService.CreateAsync(Guid userId)`:
   - Pobranie liczby retrospektyw; generowanie nazwy; utworzenie encji; zapis; mapowanie.
4. Skonfigurować obsługę wyjątków i middleware globalny dla formatowania odpowiedzi błędów, użyć istniejącego middleware jeśli możliwe.
