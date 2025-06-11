# Plan implementacji widoku LoginPage

## 1. Przegląd

Widok `LoginPage` umożliwia użytkownikowi zalogowanie się do aplikacji poprzez wprowadzenie adresu e-mail oraz hasła. Po pomyślnym uwierzytelnieniu użytkownik jest przekierowywany na stronę dashboardu. W razie błędów walidacji lub nieprawidłowych danych wyświetlane są odpowiednie komunikaty.

## 2. Routing widoku

- Ścieżka: `/login`
- Layout: `PublicLayout` (zapewnia wspólny kontener i strukturę dla publicznych stron)
- PublicRoute: dostępny bez JWT, w przypadku zalogowania powinien przekierować na dashboard.

## 3. Struktura komponentów

- PublicLayout
  - `Outlet`
    - LoginPage
      - LoginForm
        - EmailField (MUI TextField)
        - PasswordField (MUI TextField, typ password)
        - SubmitButton (MUI Button)
        - ErrorAlert (MUI Alert)

## 4. Szczegóły komponentów

### LoginPage

- Opis: kontener strony, zarządza meta (tytuł strony), inicjuje autofocus.
- Główne elementy:
  - Nagłówek (nazwa aplikacji/logo)
  - `LoginForm`
- Propsy: brak
- Zdarzenia: brak, deleguje do `LoginForm`

### LoginForm

- Opis: formularz logowania z lokalnym stanem pól i obsługą mutacji.
- Główne elementy:
  - `TextField` email
  - `TextField` password
  - `Button` „Zaloguj się”
  - `Alert` dla błędów (generalny i walidacji)
- Obsługiwane interakcje:
  - onChange dla każdego pola (aktualizacja stanu formularza)
  - onSubmit formularza -> wywołanie mutacji `useLoginMutation`
- Warunki walidacji:
  - email: required, poprawny format (`/^[^@\s]+@[^@\s]+\.[^@\s]+$/`)
  - password: required
  - przy niepoprawnym statusie 400 mapowanie błędów per pole
  - przy statusie 401 wyświetlenie błędu generalnego "Nieprawidłowy email lub hasło"
- Typy:
  - Props: none
  - Lokalny ViewModel:
    ```ts
    interface LoginFormValues {
      email: string;
      password: string;
    }
    interface LoginFormErrors {
      email?: string;
      password?: string;
      general?: string;
    }
    ```
- Zewnętrzne typy DTO:
  - `LoginRequestDto`
  - `AuthResponseDto`

## 5. Typy

- LoginRequestDto (z `AuthDto.ts`)
- AuthResponseDto (z `AuthDto.ts`)
- LoginFormValues { email: string; password: string; }
- LoginFormErrors { email?: string; password?: string; general?: string; }

## 6. Zarządzanie stanem

- Formularz: **React Hook Form** lub `useState` + `useEffect` dla walidacji onBlur.
- Mutacja: `useMutation` z React Query:
  ```ts
  const loginMutation = useMutation<
    AuthResponseDto,
    AxiosError,
    LoginRequestDto
  >((values) => axios.post("/auth/login", values), {
    onSuccess: handleSuccess,
    onError: handleError,
  });
  ```
- Kontekst uwierzytelnienia:
  - `useAuthContext()` do ustawienia tokena i danych użytkownika
  - Po `loginMutation.mutate` na `onSuccess` wywołać `auth.setAuth(token, user)` i `navigate('/dashboard')`

## 7. Integracja API

- Endpoint: `POST /auth/login`
- Request: `LoginRequestDto` (`{ email, password }`)
- Response: `AuthResponseDto` (`{ token, user }`)
- Błędy:
  - 400: `ValidationError` -> mapować na `LoginFormErrors`
  - 401: unauthorized -> ustawić `errors.general`
  - inne: show lokalny `Alert` z tekstem „Wystąpił błąd, spróbuj ponownie później”

## 8. Interakcje użytkownika

1. Użytkownik wchodzi na `/login` – focus na polu email
2. Wypełnia email i hasło (walidacja onBlur)
3. Klik przycisku „Zaloguj się”:
   - Formularz waliduje wymagane pola
   - Wywołanie mutacji (button disabled, loader)
4. OnSuccess: ustawienie kontekstu, redirect do `/dashboard`
5. OnError:
   - 400: błędy pod polami
   - 401: generalny Alert
   - inne: generalny Alert

## 9. Warunki i walidacja

- Disabled submit gdy pola są puste lub walidacja błędna
- Walidacja email regex
- Błędy backendu synchronizowane z polami formularza

## 10. Obsługa błędów

- Lokalny `Alert` w `LoginForm` dla błędów niezwiązanych z polami
- Renderowanie listy błędów walidacyjnych pod odpowiednimi polami
- W razie awarii sieci: catch-blok z generalnym komunikatem

## 11. Kroki implementacji

1. Utworzyć plik `pages/LoginPage.tsx` i komponent bazowy z `PublicLayout` + `LoginForm`
2. Zainstalować i skonfigurować React Hook Form (opcjonalnie)
3. Utworzyć `components/LoginForm.tsx`:
   - Stany, walidacja, obsługa onSubmit
   - Mapowanie typów DTO
4. Dodać hook `useLoginMutation.ts` wykorzystujący React Query
5. Dodać `AuthContext`/`useAuthContext` jeśli jeszcze nie istnieje (metody `setAuth`)
6. Skonfigurować Axios (interceptor do ustawiania JWT)
7. Dodać trasę w `AppRouter.tsx`: `<Route path="/login" element={<PublicRoute><LoginPage/></PublicRoute>} />`
8. Dodać stylizacje MUI `sx`
