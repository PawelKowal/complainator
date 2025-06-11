# Plan implementacji widoku RegisterPage

## 1. Przegląd

Strona rejestracji nowego użytkownika. Umożliwia wprowadzenie adresu e-mail i hasła (min. 8 znaków, co najmniej jedna duża litera, cyfra i znak niealfanumeryczny), walidację danych, obsługę błędów (backend i frontend) oraz przekierowanie na dashboard po pomyślnej rejestracji.

## 2. Routing widoku

Ścieżka: `/register` (wsparta `PublicLayout`).

## 3. Struktura komponentów

- PublicLayout
  └─ RegisterPage
  └─ RegisterForm
  ├─ TextField (email)
  ├─ TextField (password)
  ├─ Button ("Zarejestruj się")
  └─ ErrorAlert (MUI Alert)

## 4. Szczegóły komponentów

### PublicLayout

- Cel: opakowanie stron publicznych (login, register). Zawiera MUI `Container` i `Outlet`.
- Propsy: brak.

### RegisterPage

- Opis: wrapper widoku rejestracji, korzysta z `RegisterForm`.
- Główne elementy: `<RegisterForm />`.
- Propsy: brak.

### RegisterForm

- Opis: główny formularz rejestracji.
- Główne elementy:
  - `TextField` email
    - required, type="email"
    - autoFocus
    - walidacja: niepuste, poprawny format (regex)
  - `TextField` password
    - required, type="password"
    - minLength 8
    - walidacja: niepuste, długość ≥8, zawiera co najmniej jedną dużą literę, cyfrę i znak niealfanumeryczny
  - `Button` "Zarejestruj się"
    - type="submit"
    - disabled: loading || !isValid(formState)
  - `ErrorAlert`
    - pokazuje błędy zwrócone przez API (`message`)
- Obsługiwane zdarzenia:
  - onChange pól → aktualizacja stanu formularza
  - onSubmit → walidacja front + `registerMutation.mutate`
- Warunki walidacji:
  - email: required, regex
  - password: required, min 8 znaków, zawiera co najmniej jedną dużą literę, cyfrę i znak niealfanumeryczny
- Typy:
  - `RegisterRequest`
  - `RegisterResponse`
  - `UserDto`
- Propsy: brak (wszystko wewnętrzne)

### ErrorAlert

- Opis: komponent MUI `Alert` do lokalnego wyświetlania błędów
- Propsy:
  - `message: string`
  - `severity?: 'error'` (default)

## 5. Typy

- import z `src/dto/AuthDto.ts`:
  ```ts
  interface RegisterRequest {
    email: string;
    password: string;
  }
  interface RegisterResponse {
    token: string;
    user: UserDto;
  }
  interface UserDto {
    id: string;
    email: string;
  }
  ```
- Nowy ViewModel (opcjonalnie wewnętrzny):
  ```ts
  interface RegisterFormState {
    email: string;
    password: string;
    errors: {
      email?: string;
      password?: string;
      submit?: string;
    };
  }
  ```

## 6. Zarządzanie stanem

- `useState<RegisterFormState>` w `RegisterForm` do pól i błędów.
- `useMutation<RegisterResponse, AxiosError, RegisterRequest>` jako `useRegister`:
  - function: `axios.post('/auth/register', data)`
  - onSuccess: zapisz token (localStorage), `authContext.setUser(user)`, `navigate('/dashboard')`
  - onError: ustaw `formState.errors.submit` wg `error.response?.data.message`

## 7. Integracja API

- Endpoint: `POST /auth/register`
- Request: `RegisterRequest`
- Response: `RegisterResponse`
- Hook:
  ```ts
  const registerMutation = useMutation(
    (data: RegisterRequest) =>
      axios.post<RegisterResponse>("/auth/register", data),
    {
      onSuccess: ({ data }) => {
        localStorage.setItem("token", data.token);
        authContext.setUser(data.user);
        navigate("/dashboard");
      },
      onError: (error: AxiosError) => {
        formState.errors.submit =
          error.response?.data?.message || "Wystąpił błąd";
      },
    }
  );
  ```

## 8. Interakcje użytkownika

1. Wpisanie e-mail → inline walidacja
2. Wpisanie hasła → inline walidacja
3. Klik "Zarejestruj się" → walidacja front → wywołanie API
4. W przypadku błędów API → wyświetlenie `ErrorAlert`
5. W przypadku sukcesu → redirect na `/dashboard`

## 9. Warunki i walidacja

- Frontend:
  - email niepusty + regex
  - password ≥8 znaków, zawiera co najmniej jedną dużą literę, cyfrę i znak niealfanumeryczny
- Backend:
  - 400 Bad Request → walidacja pola
  - 409 Conflict → e-mail już istnieje
- Submit przycisku disabled aż do spełnienia warunków

## 10. Obsługa błędów

- Walidacja front → `helperText` i `error` w `TextField`
- Błędy API → `ErrorAlert` z komunikatem
- Status 500+ → domyślny komunikat "Wystąpił błąd, spróbuj ponownie później"

## 11. Kroki implementacji

1. Utworzyć plik `src/pages/RegisterPage.tsx`, osadzić `PublicLayout` + `RegisterForm`.
2. Dodać route `/register` w `AppRoutes.tsx` w `PublicLayout`.
3. Zaimplementować model stanu `RegisterFormState` + hook `useRegister`.
4. Zbudować JSX formularza z MUI `TextField`, `Button`, `Alert`.
5. Dodać walidację pól i sterowanie disabled przycisku.
6. Skonfigurować `useMutation` dla rejestracji i obsługę onSuccess/onError.
7. Po sukcesie zapisać token i usera, wykonać `navigate('/dashboard')`.
