# API Endpoint Implementation Plan: POST /auth/register

## 1. Przegląd punktu końcowego

Endpoint służy do rejestracji nowego użytkownika poprzez podanie unikalnego adresu e-mail i hasła. Po pomyślnym utworzeniu konta generowany jest token JWT oraz zwracane są podstawowe dane użytkownika.

## 2. Szczegóły żądania

- Metoda HTTP: POST
- URL: `/auth/register`
- Nagłówki:
  - `Content-Type: application/json`
- Parametry ścieżki i zapytania: brak
- Body (JSON):
  ```json
  {
    "email": "user@example.com", // string, wymagany, prawidłowy e-mail
    "password": "string" // string, wymagany, min długość 8
  }
  ```
- Walidacja:
  - `email`: DataAnnotations `[Required]`, `[EmailAddress]`
  - `password`: `[Required]`, `[StringLength(100, MinimumLength = 8)]`

## 3. Szczegóły odpowiedzi

- 201 Created
  ```json
  {
    "token": "<JWT>",
    "user": {
      "id": "<GUID>",
      "email": "user@example.com"
    }
  }
  ```
- 400 Bad Request – błędy walidacji ModelState lub nieudane tworzenie użytkownika (szczegóły błędów)
- 409 Conflict – użytkownik z podanym e-mailem już istnieje
- 500 Internal Server Error – nieoczekiwane błędy po stronie serwera

## 4. Przepływ danych

1. API Controller przyjmuje `RegisterRequest` i automatycznie waliduje dane.
2. Service (`AuthService.RegisterAsync`) sprawdza istnienie użytkownika przez `UserManager.FindByEmailAsync`.
3. Jeśli użytkownik istnieje → zwrócenie 409.
4. Tworzenie `ApplicationUser` i wywołanie `UserManager.CreateAsync`.
5. W przypadku błędów tworzenia → zwrócenie 400 z listą komunikatów.
6. Po sukcesie generacja tokena JWT (`JwtSecurityTokenHandler`) z ustawieniami z `appsettings.json`.
7. Mapowanie na `RegisterResponse` i zwrócenie 201 Created.

## 5. Względy bezpieczeństwa

- Haszowanie haseł i standardowe ustawienia ASP.NET Identity.
- Przechowywanie klucza JWT w bezpiecznej konfiguracji (sekcja `Jwt` w `appsettings.json`).
- Wymuszenie HTTPS.
- Ograniczenie długości i formatu pól wejściowych.
- Ochrona przed wyczerpaniem zasobów (bruteforce) – w przyszłości rate limiting.
- Unifikowanie komunikatów błędu tworzenia użytkownika, by nie ujawniać szczegółów.

## 6. Obsługa błędów

- 400: ModelState invalid → zwrócić `BadRequest(ModelState)`.
- 409: istniejący e-mail → `Conflict(new { message = "Email already exists" })`.
- 400: błędy z `IdentityResult.Errors` → agregacja `IdentityError.Description`.
- 500: przechwytywane przez globalny middleware → `InternalServerError` z logowaniem.

## 7. Wydajność

- Całość operacji asynchroniczna (`async/await`).
- Indeks na `NormalizedEmail` zapewniony przez Identity.
- Unikać blokujących wywołań.
- Token generowany lokalnie, bez zewnętrznych wywołań.

## 8. Kroki implementacji

1. Utworzyć klasę konfiguracyjną `JwtSettings` i dodać do `appsettings.json`:
   ```json
   "Jwt": {
     "Issuer": "YourIssuer",
     "Audience": "YourAudience",
     "Key": "YourSuperSecretKey",
     "ExpiresInMinutes": 60
   }
   ```
2. Zarejestrować `JwtSettings` w DI (`builder.Services.Configure<JwtSettings>(...)`).
3. Utworzyć interfejs `IAuthService` z metodą `Task<RegisterResponse> RegisterAsync(RegisterRequest request)`.
4. Implementować `AuthService`:
   - Injekcja `UserManager<ApplicationUser>`, `IOptions<JwtSettings>`, `ILogger<AuthService>`.
   - Logika rejestracji i generacji JWT.
5. Zarejestrować `IAuthService` w DI (`AddScoped<IAuthService, AuthService>()`).
6. Utworzyć `Controllers/AuthController`:
   - Atrybuty `[ApiController]`, `[Route("auth")]`, `[AllowAnonymous]`.
   - Endpoint `Post("register")`, injekt `IAuthService`.
   - Sprawdzanie `ModelState` oraz obsługa wyjątków i kodów.
