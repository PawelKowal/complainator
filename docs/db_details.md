<conversation_summary>
<decisions>

1. Tabela `Users` będzie zarządzana przez ASP.NET Identity – nie definiujemy własnej tabeli użytkowników.
2. Wszystkie klucze główne będą typu GUID przechowywanego jako `TEXT`.
3. Brak wsparcia dla archiwizacji danych w MVP.
4. Kategorie notatek oraz statusy sugestii AI będą enumami po stronie kodu – bez oddzielnych tabel słownikowych.
5. W tabeli `Retrospective` pozostają pola `acceptedCount` i `rejectedCount`, zarządzane ręcznie w aplikacji.
6. Dodajemy tabelę `AuditLog` z kolumnami: `id`, `timestamp`, `userId`, `level`, `message` i `retrospectiveId`.
7. RLS realizowane wyłącznie w warstwie aplikacji przez filtrowanie po `userId`.
8. Indeksy na kluczach obcych: `Retrospective(userId)`, `Note(retrospectiveId)`, `AISuggestion(retrospectiveId)`.
9. Daty i czasy przechowujemy jako `TEXT` w formacie ISO8601 z `DEFAULT CURRENT_TIMESTAMP`.
10. Brak triggerów ani transakcji wielotabelowych – logika operacji zarządzana w aplikacji.
    </decisions>

<matched_recommendations>

1. Zdefiniować tabelę `Retrospective`:
   - `id TEXT PRIMARY KEY`
   - `userId TEXT NOT NULL REFERENCES Users(id) ON DELETE CASCADE`
   - `name TEXT NOT NULL`
   - `date TEXT NOT NULL`
   - `createdAt TEXT DEFAULT CURRENT_TIMESTAMP`
   - `acceptedCount INTEGER DEFAULT 0`
   - `rejectedCount INTEGER DEFAULT 0`
2. Zdefiniować tabelę `Note`:
   - `id TEXT PRIMARY KEY`
   - `retrospectiveId TEXT NOT NULL REFERENCES Retrospective(id) ON DELETE CASCADE`
   - `category TEXT NOT NULL`
   - `content TEXT NOT NULL`
   - `createdAt TEXT DEFAULT CURRENT_TIMESTAMP`
3. Zdefiniować tabelę `AISuggestion`:
   - `id TEXT PRIMARY KEY`
   - `retrospectiveId TEXT NOT NULL REFERENCES Retrospective(id) ON DELETE CASCADE`
   - `suggestionText TEXT NOT NULL`
   - `status TEXT NOT NULL DEFAULT 'pending'`
   - `createdAt TEXT DEFAULT CURRENT_TIMESTAMP`
4. Zdefiniować tabelę `AuditLog`:
   - `id INTEGER PRIMARY KEY AUTOINCREMENT`
   - `timestamp TEXT DEFAULT CURRENT_TIMESTAMP`
   - `userId TEXT`
   - `level TEXT NOT NULL`
   - `message TEXT NOT NULL`
   - `retrospectiveId TEXT`
5. Włączyć `PRAGMA foreign_keys = ON` i używać `ON DELETE CASCADE` dla spójności referencyjnej.
6. Utworzyć indeksy:
   - `CREATE INDEX idx_retrospective_user ON Retrospective(userId);`
   - `CREATE INDEX idx_note_retrospective ON Note(retrospectiveId);`
   - `CREATE INDEX idx_aisuggestion_retrospective ON AISuggestion(retrospectiveId);`
7. Stosować `TEXT` (GUID) dla PK i `TEXT` ISO8601 dla znaczników czasu.
8. Zarządzać polami `acceptedCount` i `rejectedCount` w logice aplikacji.
   </matched_recommendations>

<database_planning_summary>
MVP wymaga przechowywania retrospektyw użytkowników (autentykacja przez ASP.NET Identity), z możliwością dodawania notatek w trzech kategoriach oraz generowania i przeglądu sugestii AI (statusy: pending, accepted, rejected). Każda retrospektywa ma liczniki zaakceptowanych i odrzuconych wniosków, a operacje zapisu i logowania błędów/zdarzeń trafiają do tabeli `AuditLog`. Kluczowe encje to:

- `Retrospective` 1–N `Note`
- `Retrospective` 1–N `AISuggestion`
- (`User` 1–N `Retrospective`, zarządzany przez ASP.NET Identity)
- `AuditLog` powiązany z `userId` i opcjonalnie `retrospectiveId`

Bezpieczeństwo opiera się na filtrowaniu po `userId` w warstwie aplikacji (brak DB-level RLS). Dane są niewielkie, więc stawiamy na prostotę: GUID-y, domyślne znaczniki czasu ISO8601, indeksy na kluczach obcych oraz `ON DELETE CASCADE` dla integralności. Logika wielotabelowa (liczniki, tworzenie notatek) odbywa się w kodzie, bez triggerów ani transakcji DB.  
</database_planning_summary>

<unresolved_issues>
Brak nierozwiązanych kwestii.
</unresolved_issues>
</conversation_summary>
