# Plan implementacji widoku DashboardPage

## 1. Przegląd

DashboardPage wyświetla użytkownikowi listę jego retrospektyw w formie przewijanej (infinite scroll), pozwala na utworzenie nowej retrospektywy oraz wylogowanie. Zapewnia też obsługę błędów i pokazuje komunikat przy braku danych.

## 2. Routing widoku

Ścieżka: `/dashboard` (alias `/`). Komponent `DashboardPage` osadzony w `PrivateLayout`, chroniony przez `PrivateRoute`.

## 3. Struktura komponentów

- PrivateLayout  
  ├─ Header (AppBar z przyciskami "Nowa retrospektywa" i "Wyloguj się")  
  ├─ ErrorAlert (globalny banner)  
  └─ DashboardPage  
   ├─ InfiniteScrollList  
   │ └─ RetrospectiveCard (lista elementów)  
   ├─ EmptyState (gdy brak retrospektyw)  
   └─ LoadingSkeleton (podczas wczytywania)

## 4. Szczegóły komponentów

### Header

- Opis: pasek aplikacji z tytułem i kontrolkami
- Główne elementy:
  - Button "Nowa retrospektywa" (onClick → utworzenie retrospektywy)
  - Button "Wyloguj się" (onClick → czyszczenie JWT + redirect)
- Obsługiwane zdarzenia: click
- Warunki walidacji: przycisk "Nowa retrospektywa" disabled gdy trwa mutacja
- Propsy: brak (globalnie w `PrivateLayout`)

### ErrorAlert

- Opis: pokazuje banner z komunikatem "Wystąpił błąd, spróbuj ponownie później"
- Główne elementy: MUI `Alert`
- Obsługiwane zdarzenia: —
- Warunki walidacji: pojawia się gdy dowolne zapytanie zwróci błąd (401 lub 5xx)
- Propsy: `message: string`, `severity?: 'error'`

### DashboardPage

- Opis: główny widok z listą retrospektyw
- Główne elementy:
  - InfiniteScrollList (wrapper `useInfiniteQuery`)
  - EmptyState (`Typography` z komunikatem "Brak retrospektyw do wyświetlenia")
- Obsługiwane zdarzenia:
  - scroll do końca → fetchNextPage
- Warunki walidacji: —
- Typy:
  - RetrospectiveListResponse, RetrospectiveListItem
- Propsy: brak (samodzielny widok)

### InfiniteScrollList

- Opis: wrapper do `useInfiniteQuery` dla GET `/retrospectives`
- Główne elementy:
  - MUI `List` / `Grid`
  - RetrospectiveCard dla każdego elementu
- Obsługiwane zdarzenia:
  - fetchNextPage przy scroll
- Warunki walidacji:
  - `hasNextPage` z `page * perPage < total`
- Typy:
  - RetrospectiveListRequest, RetrospectiveListResponse
- Propsy:
  - `queryKey`, `queryFn`, `renderItem`

### RetrospectiveCard

- Opis: pojedynczy element listy retrospektywy
- Główne elementy:
  - `Card` z `Typography` name/date
  - lista `acceptedSuggestions` w `ListItem`
- Obsługiwane zdarzenia: click → `navigate(/retrospectives/${id})`
- Warunki walidacji: —
- Typy:
  - RetrospectiveListItem
- Propsy:
  - `item: RetrospectiveListItem`

### EmptyState

- Opis: komponent wyświetlany przy braku danych
- Główne elementy: `Typography`
- Propsy: opcjonalny `message`

### LoadingSkeleton

- Opis: placeholder podczas ładowania
- Główne elementy: MUI `Skeleton`
- Propsy: brak

## 5. Typy

```ts
// Żądanie listy
interface RetrospectiveListRequest {
  page: number;
  perPage: number;
  sort: "dateDesc" | "dateAsc";
}

// Odpowiedź listy
interface RetrospectiveListResponse {
  items: RetrospectiveListItem[];
  total: number;
  page: number;
  perPage: number;
}

// Pojedynczy element listy
interface RetrospectiveListItem {
  id: string;
  name: string;
  date: string;
  acceptedSuggestions: SuggestionListItem[];
}

// DTO do tworzenia retrospektywy
interface CreateRetrospectiveResponse {
  id: string;
  name: string;
  date: string;
  acceptedCount: number;
  rejectedCount: number;
}
```

## 6. Zarządzanie stanem

- Custom hook `useRetrospectives` korzystający z `useInfiniteQuery` (React Query)
- Hook `useCreateRetrospective` z `useMutation` do POST `/retrospectives`
- Globalny store/auth hook do logout (Axios + clear token + redirect)

## 7. Integracja API

- GET `/retrospectives?page=&perPage=&sort=date_desc`
  - queryFn zwraca `RetrospectiveListResponse`
- POST `/retrospectives`
  - body: `{}`
  - odpowiedź: `CreateRetrospectiveResponse`
  - onSuccess: refetch listy lub `queryClient.invalidateQueries('retrospectives')`

## 8. Interakcje użytkownika

1. Załadowanie widoku → pobranie pierwszej strony retrospektyw
2. Scroll → kolejne strony
3. Klik "Nowa retrospektywa" → mutacja → po sukcesie refetch + scroll to top
4. Klik elementu → nawigacja do szczegółów
5. Klik "Wyloguj się" → czyszczenie JWT + redirect `/login`

## 9. Warunki i walidacja

- `hasNextPage` = `page * perPage < total`
- `NewRetroButton.disabled` gdy `isLoading` mutacji
- PrivateRoute waliduje obecność JWT; brak → redirect

## 10. Obsługa błędów

- Błędy 401 → wywołanie logout + redirect `/login`
- Błędy sieci/500 → pokaz `ErrorAlert`
- Retry: przycisk "Spróbuj ponownie" w `ErrorAlert` → refetch

## 11. Kroki implementacji

1. Utworzyć hooki `useRetrospectives` i `useCreateRetrospective`.
2. Dodać route `/dashboard` w konfiguracji React Router w `PrivateRoute`.
3. Zaimplementować komponent `DashboardPage` z `InfiniteScrollList`, `EmptyState`, `LoadingSkeleton`.
4. Dodać `Header` z przyciskami i powiązać akcje mutacji i logout.
5. Stworzyć `RetrospectiveCard` i podłączyć navigację.
6. Obsłużyć błędy przez globalny `ErrorAlert`.
7. Zapewnić responsywność (minWidth 1024px) i style przez `sx` MUI.
