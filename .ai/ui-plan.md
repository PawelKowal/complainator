# Architektura UI dla Complainator

## 1. Przegląd struktury UI

Interfejs podzielony jest na dwie główne strefy:

- Publiczną (autoryzacja): `/login`, `/register` z `PublicLayout`.
- Prywatną (dashboard, retrospektywy): `PrivateLayout` chroniony JWT.

Routing: React Router v6 z zagnieżdżonymi trasami; dane: React Query; klient HTTP: Axios z `baseURL` i `withCredentials: true`.

Layout desktop-first (minWidth 1024px), style: MUI (`sx` prop).

## 2. Lista widoków

### LoginPage

- Ścieżka: `/login`
- Cel: umożliwić zalogowanie użytkownika.
- Kluczowe informacje: pola `email`, `password`.
- Komponenty: `TextField`, `Button`, `Alert` (błędy).
- UX/dostępność/bezpieczeństwo: autofocus na `email`, walidacja pola, aria-label, redirect po sukcesie.
- Mapowanie user stories: US-001, US-002, US-003

### RegisterPage

- Ścieżka: `/register`
- Cel: rejestracja nowego użytkownika.
- Kluczowe informacje: pola `email`, `password` (min 8 znaków).
- Komponenty: analogiczne jak w `LoginPage`.
- UX/dostępność/bezpieczeństwo: walidacja hasła, aria-label.
- Mapowanie user stories: US-001

### DashboardPage

- Ścieżka: `/` lub `/dashboard`
- Cel: prezentacja listy retrospektyw z infinite scroll.
- Kluczowe informacje: nazwa retrospektywy, data, teksty zaakceptowanych sugestii.
- Komponenty: `InfiniteScrollList`, `Button` "Nowa retrospektywa", `Skeleton`, `Alert`, placeholder "Brak retrospektyw do wyświetlenia".
- UX: przycisk "Nowa retrospektywa" disabled podczas mutacji, czytelny układ listy.
- Mapowanie user stories: US-004, US-005, US-012, US-013

### RetrospectiveDetailPage

- Ścieżka: `/retrospectives/:id`
- Cel: szczegółowe zarządzanie retrospektywą.
- Kluczowe informacje: kolumny notatek (`Co wymaga poprawy`, `Obserwacje`, `Co poszło dobrze`), lista zaakceptowanych sugestii AI.
- Komponenty: `Grid` 3-kolumnowy, `NoteColumn` z `AddNoteButton`, `AddNoteDialog`, `Button` "Generuj wnioski", `SuggestionsList` z przyciskami `Zaakceptuj`/`Odrzuć`, `Skeleton`, `Alert`.
- UX: idempotentne generowanie sugestii, przyciski disabled podczas ładowania, błędy lokalne w `Alert`.
- UX: jeżeli retrospektywa zawiera już sugestie, przyciski do dodawania notatek i generowania sugestii powinny być disabled.
- Mapowanie user stories: US-006, US-007, US-008, US-009, US-010, US-011, US-012

### NotFoundPage

- Ścieżka: `*`
- Cel: obsługa nieznanych tras.
- Komponenty: informacja "Strona nie znaleziona", link do dashboard.
- Mapowanie user stories: brak

## 3. Mapa podróży użytkownika

1. Wejście na `/login` lub `/register`.
2. Po uwierzytelnieniu redirect do `/dashboard`.
3. Na `DashboardPage`:
   - Infinite scroll listuje retrospektywy.
   - Klik "Nowa retrospektywa" -> `POST /retrospectives` -> refetch listy.
   - Klik w element listy -> `/retrospectives/:id`.
4. Na `RetrospectiveDetailPage`:
   - Dodanie notatki przez `AddNoteDialog` -> `POST /retrospectives/{id}/notes`.
   - Klik "Generuj wnioski" -> `POST /retrospectives/{id}/generate-suggestions` -> wyświetlenie sugestii.
   - Akceptacja/odrzucenie -> `PATCH /suggestions/{id}` -> refetch szczegółów.
   - Powrót do dashboard przez header lub breadcrumb.

## 4. Układ i struktura nawigacji

- `PublicLayout` (ścieżki: `/login`, `/register`):
  - `Container`, `Outlet`.
- `PrivateLayout` (ścieżki: `/`, `/dashboard`, `/retrospectives/:id`):
  - `AppBar` z tytułem i `Button` "Nowa retrospektywa".
  - Globalny `ErrorAlert`.
  - `Container`, `Outlet`.
- `PrivateRoute`: sprawdzenie JWT, redirect na `/login` jeśli brak.

## 5. Kluczowe komponenty

- **Header**: `AppBar` z przyciskiem "Nowa retrospektywa".
- **InfiniteScrollList**: wrapper do `useInfiniteQuery`.
- **NoteColumn**: nagłówek kolumny, lista notatek.
- **AddNoteDialog**: modal z select kategorii i `TextField`.
- **SuggestionsList**: lista sugestii z przyciskami akceptacji/odrzucenia.
- **LoadingSkeleton**: `Skeleton`.
- **ErrorAlert**: wrapper dla MUI `Alert`, globalne i lokalne błędy.
- **PrivateRoute**: ochrona tras.
