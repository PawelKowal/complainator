# Plan Testów dla Projektu "Complainator"

## 1. Wprowadzenie i cele testowania

### 1.1. Wprowadzenie

Niniejszy dokument opisuje plan testów dla aplikacji "Complainator", narzędzia do przeprowadzania retrospektyw z wykorzystaniem sugestii generowanych przez sztuczną inteligencję. Plan obejmuje strategię, zakres, zasoby i harmonogram działań testowych mających na celu zapewnienie wysokiej jakości produktu.

### 1.2. Cele testowania

Główne cele procesu testowania to:

- **Weryfikacja funkcjonalności:** Upewnienie się, że wszystkie funkcje aplikacji działają zgodnie z wymaganiami.
- **Zapewnienie bezpieczeństwa:** Sprawdzenie, czy dane użytkowników są bezpieczne i czy system jest odporny na podstawowe ataki.
- **Ocena niezawodności:** Weryfikacja stabilności aplikacji oraz jej odporności na błędy, w tym błędy pochodzące z usług zewnętrznych (AI).
- **Zapewnienie użyteczności:** Sprawdzenie, czy interfejs użytkownika jest intuicyjny i łatwy w obsłudze.
- **Identyfikacja i raportowanie defektów:** Znalezienie i udokumentowanie błędów w celu ich naprawy przed wdrożeniem produkcyjnym.

## 2. Zakres testów

### 2.1. Funkcjonalności objęte testami

- **Moduł uwierzytelniania i autoryzacji:**
  - Rejestracja nowego użytkownika.
  - Logowanie i wylogowywanie.
  - Zarządzanie sesją użytkownika (tokeny JWT).
  - Ochrona endpointów wymagających autoryzacji.
- **Zarządzanie retrospektywami:**
  - Tworzenie nowej, pustej retrospektywy.
  - Wyświetlanie listy retrospektyw z paginacją i sortowaniem.
  - Wyświetlanie szczegółów pojedynczej retrospektywy.
  - Dodawanie notatek do retrospektywy.
- **Integracja ze sztuczną inteligencją:**
  - Generowanie sugestii na podstawie notatek z retrospektywy.
  - Obsługa odpowiedzi (sukces, błąd, brak sugestii) z serwisu AI.
- **Zarządzanie sugestiami:**
  - Akceptowanie i odrzucanie sugestii.

### 2.2. Funkcjonalności wyłączone z testów

- Testy wydajnościowe pod dużym obciążeniem (poza zakresem bieżącej fazy).
- Zaawansowane testy penetracyjne (poza zakresem bieżącej fazy).
- Testy kompatybilności na szerokiej gamie niszowych przeglądarek i systemów operacyjnych.

## 3. Typy testów do przeprowadzenia

- **Testy jednostkowe (Unit Tests):**
  - **Backend:** Testowanie logiki w klasach serwisów (`*.Service.cs`) w izolacji od bazy danych i API (z użyciem mocków).
  - **Frontend:** Testowanie pojedynczych komponentów React (`*.tsx`) w izolacji, weryfikacja renderowania i logiki.
- **Testy integracyjne (Integration Tests):**
  - **Backend:** Testowanie endpointów API (`*Controller.cs`) w połączeniu z warstwą serwisową i testową bazą danych (in-memory). Weryfikacja pełnego przepływu żądania od kontrolera do bazy danych.
- **Testy E2E (End-to-End):**
  - Symulowanie pełnych scenariuszy użytkownika w przeglądarce, obejmujących interakcję frontendu z backendem.
- **Testy API (manualne i automatyczne):**
  - Weryfikacja kontraktu API (endpointy, metody, kody odpowiedzi, formaty DTO) za pomocą narzędzi takich jak Postman/Insomnia.
- **Testy bezpieczeństwa:**
  - Sprawdzanie kontroli dostępu (czy użytkownik A nie ma dostępu do danych użytkownika B).
  - Weryfikacja ochrony endpointów.
- **Testy regresji:**
  - Wykonywane po każdej istotnej zmianie w kodzie, aby upewnić się, że nowe funkcje nie zepsuły istniejących. Obejmują kluczowe testy jednostkowe, integracyjne i E2E.

## 4. Scenariusze testowe dla kluczowych funkcjonalności

### 4.1. Scenariusz: Rejestracja i logowanie użytkownika

1.  **Kroki:**
    1.  Otwórz stronę rejestracji.
    2.  Wypełnij formularz poprawnymi danymi i prześlij go.
    3.  Oczekiwany rezultat: Użytkownik zostaje zalogowany i przekierowany do panelu głównego.
    4.  Wyloguj się.
    5.  Otwórz stronę logowania.
    6.  Wprowadź dane użyte podczas rejestracji.
    7.  Oczekiwany rezultat: Użytkownik zostaje zalogowany.
2.  **Warunki brzegowe:** Rejestracja z istniejącym adresem e-mail, puste pola, niepoprawny format e-maila/hasła.

### 4.2. Scenariusz: Pełen cykl życia retrospektywy

1.  **Kroki:**
    1.  Zaloguj się do aplikacji.
    2.  Utwórz nową retrospektywę.
    3.  Oczekiwany rezultat: Nowa retrospektywa pojawia się na liście.
    4.  Wejdź w szczegóły nowej retrospektywy.
    5.  Dodaj kilka notatek (np. co poszło dobrze, co poszło źle).
    6.  Oczekiwany rezultat: Notatki są widoczne na stronie.
    7.  Uruchom generowanie sugestii AI.
    8.  Oczekiwany rezultat: Po chwili na liście pojawiają się sugestie wygenerowane przez AI.
    9.  Zaakceptuj jedną sugestię i odrzuć inną.
    10. Oczekiwany rezultat: Akcje są odzwierciedlone w interfejsie.
2.  **Warunki brzegowe:** Generowanie sugestii bez notatek, obsługa błędów API (AI).

### 4.3. Scenariusz: Izolacja danych użytkowników

1.  **Kroki:**
    1.  Zarejestruj i zaloguj Użytkownika A.
    2.  Utwórz retrospektywę (ID: Retro_A) dla Użytkownika A.
    3.  Zarejestruj i zaloguj Użytkownika B.
    4.  Spróbuj uzyskać dostęp do Retro_A przez bezpośrednie zapytanie API (`GET /retrospectives/{Retro_A_ID}`) jako Użytkownik B.
    5.  Oczekiwany rezultat: API zwraca błąd 404 Not Found (lub 403 Forbidden), uniemożliwiając dostęp.

## 5. Środowisko testowe

- **Baza danych:** SQLite (dla testów lokalnych i integracyjnych: wersja in-memory).
- **Frontend:** Dowolna nowoczesna przeglądarka (Chrome, Firefox).
- **Backend:** Środowisko uruchomieniowe .NET 8.
- **Infrastruktura:** Testy E2E uruchamiane lokalnie oraz w ramach pipeline'u CI/CD.

## 6. Narzędzia do testowania

- **Testy jednostkowe (.NET):** NUnit, NSubstitute.
- **Testy integracyjne (.NET):** `WebApplicationFactory`, NUnit.
- **Testy jednostkowe (React):** Jest, React Testing Library.
- **Testy E2E:** Playwright.
- **Testy manualne API:** Postman.
- **CI/CD:** GitHub Actions (do automatycznego uruchamiania testów).

## 7. Harmonogram testów

Testowanie powinno być procesem ciągłym, zintegrowanym z cyklem rozwoju oprogramowania.

- **Testy jednostkowe i integracyjne:** Pisane na bieżąco przez deweloperów wraz z nowymi funkcjonalnościami.
- **Testy E2E i manualne testy API:** Wykonywane przed każdym wdrożeniem na środowisko testowe/produkcyjne.
- **Pełna regresja:** Uruchamiana automatycznie w ramach CI/CD po każdym pushu do głównej gałęzi kodu.

## 8. Kryteria akceptacji testów

- **Kryterium wejścia (rozpoczęcia testów):**
  - Kod został zintegrowany na gałęzi deweloperskiej.
  - Aplikacja pomyślnie się kompiluje i uruchamia.
- **Kryterium wyjścia (zakończenia testów):**
  - 100% zdefiniowanych scenariuszy testowych zostało wykonanych.
  - Wszystkie testy automatyczne (jednostkowe, integracyjne) przechodzą pomyślnie.
  - Nie istnieją żadne otwarte błędy krytyczne ani blokujące.
  - Pokrycie kodu testami jednostkowymi i integracyjnymi utrzymuje się na uzgodnionym poziomie (np. > 70%).

## 9. Role i odpowiedzialności

- **Deweloperzy:**
  - Pisanie testów jednostkowych i integracyjnych.
  - Poprawianie błędów wykrytych podczas testów.
  - Utrzymywanie i rozwijanie środowiska testowego.
- **Inżynier QA / Tester:**
  - Tworzenie i utrzymywanie planu testów.
  - Projektowanie i wykonywanie scenariuszy E2E i manualnych.
  - Raportowanie i weryfikacja błędów.
  - Analiza wyników testów i raportowanie stanu jakości.

## 10. Procedury raportowania błędów

Wszystkie wykryte defekty powinny być raportowane w systemie do śledzenia zadań (np. GitHub Issues) i zawierać następujące informacje:

- **Tytuł:** Krótki, zwięzły opis problemu.
- **Kroki do reprodukcji:** Szczegółowa lista kroków prowadzących do wystąpienia błędu.
- **Wynik oczekiwany:** Co powinno się wydarzyć.
- **Wynik rzeczywisty:** Co faktycznie się wydarzyło.
- **Środowisko:** Wersja aplikacji, przeglądarka, system operacyjny.
- **Zrzuty ekranu / Logi:** Dowolne materiały pomocnicze.
- **Priorytet:** Krytyczny, Wysoki, Średni, Niski.
