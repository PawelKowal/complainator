# Dokument wymagań produktu (PRD) - Complainator

## 1. Przegląd produktu
Aplikacja Complainator to narzędzie webowe, które przyspiesza i upraszcza proces tworzenia retrospektyw sprintów oraz generowania wniosków na podstawie zgromadzonych uwag. Główne funkcje to:
- tworzenie retrospektywy o nazwie "Retrospektywa #{numer} - DD.MM.YYYY" z automatycznie nadawanym numerem i datą
- dodawanie notatek w trzech kategoriach (Co wymaga poprawy, Obserwacje, Co poszło dobrze)
- generowanie pełnego podsumowania wniosków przez AI oraz akceptacja lub odrzucenie każdej sugestii
- dashboard prezentujący listę retrospektyw (nazwa, data, pełne podsumowanie AI)
- widok szczegółowy z podziałem na kolumny notatek i zaakceptowanymi wnioskami
- podstawowe uwierzytelnianie e-mail/hasło
- globalny komponent obsługi błędów z jednolitym komunikatem
- trwałe przechowywanie danych z polami createdAt, acceptedCount, rejectedCount

## 2. Problem użytkownika
Zespół traci czas na ręczne przygotowywanie tablic do retrospektyw i analizę uwag oraz wniosków. Trudno jest zebrać wszystkie opinie w jednym miejscu i szybko wygenerować użyteczne podsumowanie do kolejnych sprintów.

## 3. Wymagania funkcjonalne
- RF-001: uwierzytelnianie e-mail/hasło
- RF-002: automatyczne generowanie retrospektywy z unikalnym numerem i datą w formacie "Retrospektywa #{numer} - DD.MM.YYYY"
- RF-003: przycisk "Nowa retrospektywa" w nagłówku
- RF-004: dodawanie notatek przez przycisk "Dodaj notatkę" w trzech kategoriach (Co wymaga poprawy, Obserwacje, Co poszło dobrze)
- RF-005: generowanie pełnego podsumowania wniosków przez AI
- RF-006: przegląd wniosków AI z przyciskami do akceptacji lub odrzucenia
- RF-007: zapisywanie tylko zaakceptowanych wniosków i aktualizacja pola acceptedCount lub rejectedCount
- RF-008: dashboard z listą retrospektyw (nazwa, data, pełne podsumowanie)
- RF-009: widok szczegółowy z trzema kolorowymi kolumnami notatek i sekcją zaakceptowanych wniosków
- RF-010: globalny komponent obsługi błędów wyświetlający komunikat "Wystąpił błąd, spróbuj ponownie później"
- RF-011: backendowy logger rejestrujący wszystkie żądania i błędy
- RF-012: model danych z polami createdAt, acceptedCount, rejectedCount
- RF-013: responsywne UI dla minimalnej szerokości 1024×768 pikseli
- RF-014: testy jednostkowe dla kluczowej logiki (generator nazwy, proces tworzenia retrospektywy, podgląd)

## 4. Granice produktu
- brak wsparcia dla wielu użytkowników w jednej retrospektywie
- brak aplikacji mobilnej
- obsługa wyłącznie tekstu (bez multimediów)
- brak możliwości edycji lub usuwania notatek po ich dodaniu
- brak archiwizacji lub usuwania zakończonych retrospektyw
- brak resetu hasła lub weryfikacji e-mail podczas rejestracji

## 5. Historyjki użytkowników

US-001
- Tytuł: Rejestracja konta
- Opis: Jako nowy użytkownik chcę utworzyć konto w aplikacji podając e-mail i hasło, aby uzyskać dostęp do funkcji retrospektyw
- Kryteria akceptacji:
  - formularz rejestracji przyjmuje poprawny adres e-mail i hasło o minimalnej długości 8 znaków
  - po udanej rejestracji użytkownik jest przekierowany do dashboardu
  - przy niepoprawnych danych wyświetlany jest komunikat o błędzie

US-002
- Tytuł: Logowanie
- Opis: Jako zarejestrowany użytkownik chcę się zalogować, aby uzyskać dostęp do aplikacji
- Kryteria akceptacji:
  - formularz logowania przyjmuje e-mail i hasło
  - przy poprawnych danych użytkownik zostaje przekierowany na dashboard
  - przy niepoprawnych danych wyświetlany jest komunikat o błędzie

US-003
- Tytuł: Ochrona dostępu do treści
- Opis: Jako niezalogowany użytkownik nie mogę uzyskać dostępu do dashboardu ani widoków retrospektyw
- Kryteria akceptacji:
  - próba wejścia na chronione strony przekierowuje na stronę logowania

US-004
- Tytuł: Tworzenie retrospektywy
- Opis: Jako zalogowany użytkownik chcę utworzyć nową retrospektywę, aby rozpocząć zbieranie uwag
- Kryteria akceptacji:
  - po kliknięciu "Nowa retrospektywa" system automatycznie generuje unikalny numer
  - retrospektywa jest tworzona o nazwie "Retrospektywa #{numer} - DD.MM.YYYY"

US-005
- Tytuł: Historia retrospektyw
- Opis: Jako użytkownik chcę zobaczyć na dashboardzie listę swoich poprzednich retrospektyw, aby szybko nawigować do dowolnej z nich.
- Kryteria akceptacji:
  - na dashboardzie znajduje się sekcja "Historia retrospektyw", wyświetlająca retrospektywy w porządku malejącym (najnowsze na górze)
  - każda pozycja zawiera nazwę retrospektywy, datę i zaakcpetowane wnioski
  - każda pozycja jest klikalna i prowadzi do szczegółowego widoku retrospektywy
  - jeśli użytkownik nie ma żadnych retrospektyw, wyświetlany jest komunikat "Brak retrospektyw do wyświetlenia"

US-006
- Tytuł: Dodawanie notatki "Co wymaga poprawy"
- Opis: Jako użytkownik chcę dodać notatkę w kategorii Co wymaga poprawy, aby wskazać obszary do poprawy
- Kryteria akceptacji:
  - przy kliknięciu "Dodaj notatkę" można wybrać kategorię Co wymaga poprawy
  - po wpisaniu treści i zatwierdzeniu notatka pojawia się w kolumnie Co wymaga poprawy

US-007
- Tytuł: Dodawanie notatki "Obserwacje"
- Opis: Jako użytkownik chcę dodać notatkę w kategorii Obserwacje, aby zapisać obserwacje bez oceny
- Kryteria akceptacji:
  - przy kliknięciu "Dodaj notatkę" można wybrać kategorię Obserwacje
  - po wpisaniu treści i zatwierdzeniu notatka pojawia się w kolumnie Obserwacje

US-008
- Tytuł: Dodawanie notatki "Co poszło dobrze"
- Opis: Jako użytkownik chcę dodać notatkę w kategorii Co poszło dobrze, aby wyróżnić dobre elementy sprintu
- Kryteria akceptacji:
  - przy kliknięciu "Dodaj notatkę" można wybrać kategorię Co poszło dobrze
  - po wpisaniu treści i zatwierdzeniu notatka pojawia się w kolumnie Co poszło dobrze

US-009
- Tytuł: Generowanie podsumowania AI
- Opis: Jako użytkownik chcę, aby aplikacja automatycznie wygenerowała podsumowanie wniosków na podstawie zebranych notatek
- Kryteria akceptacji:
  - dostępny jest przycisk "Generuj wnioski"
  - po kliknięciu podsumowanie zostaje wygenerowane i wyświetlone na ekranie

US-010
- Tytuł: Przeglądanie i akceptacja wniosków AI
- Opis: Jako użytkownik chcę przeglądać wnioski AI i akceptować lub odrzucać te, które są przydatne
- Kryteria akceptacji:
  - każdy wniosek AI ma przyciski Zaakceptuj i Odrzuć
  - po zaakceptowaniu wniosek jest zapisywany i pole acceptedCount zostaje zwiększone
  - po odrzuceniu wniosek nie jest zapisywany a pole rejectedCount zostaje zwiększone

US-011
- Tytuł: Podgląd retrospektywy
- Opis: Jako użytkownik chcę zobaczyć szczegóły retrospektywy, w tym wszystkie dodane notatki i zaakceptowane wnioski
- Kryteria akceptacji:
  - po kliknięciu wybranej retrospektywy na dashboardzie otwiera się widok szczegółowy
  - widok zawiera trzy kolumny z notatkami i sekcję z zaakceptowanymi wnioskami

US-012
- Tytuł: Obsługa błędów globalnych
- Opis: Jako użytkownik chcę otrzymać czytelny komunikat w razie błędu sieci lub serwera
- Kryteria akceptacji:
  - przy błędzie sieci lub serwera wyświetlany jest banner z komunikatem "Wystąpił błąd, spróbuj ponownie później"

US-013
- Tytuł: Responsywność UI
- Opis: Jako użytkownik chcę, aby interfejs działał poprawnie na desktopie i tablecie
- Kryteria akceptacji:
  - interfejs jest czytelny i funkcjonalny przy minimalnej szerokości 1024×768 pikseli

## 6. Metryki sukcesu
- wskaźnik akceptacji wniosków AI (acceptedCount/total) co najmniej 50%
- monitorowanie wartości acceptedCount i rejectedCount dla każdej retrospektywy
- poprawne działanie rejestracji i logowania w 100% przypadków
- wyświetlanie komunikatu o błędzie przy awarii sieci lub serwera
- poprawne wyświetlanie UI na ekranach o szerokości co najmniej 1024×768
- pokrycie testami jednostkowymi kluczowej logiki na poziomie co najmniej 80% 