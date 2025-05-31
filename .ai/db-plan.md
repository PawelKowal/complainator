1. Lista tabel z ich kolumnami, typami danych i ograniczeniami

## Retrospective

| Kolumna        | Typ         | Ograniczenia                                           |
| -------------- | ----------- | ------------------------------------------------------ |
| id             | UUID        | PRIMARY KEY, DEFAULT uuid_generate_v4()                |
| user_id        | UUID        | NOT NULL, REFERENCES AspNetUsers(Id) ON DELETE CASCADE |
| name           | TEXT        | NOT NULL                                               |
| date           | DATE        | NOT NULL                                               |
| created_at     | TIMESTAMPTZ | NOT NULL, DEFAULT now()                                |
| accepted_count | INTEGER     | NOT NULL, DEFAULT 0                                    |
| rejected_count | INTEGER     | NOT NULL, DEFAULT 0                                    |

## Note

| Kolumna          | Typ           | Ograniczenia                                             |
| ---------------- | ------------- | -------------------------------------------------------- |
| id               | UUID          | PRIMARY KEY, DEFAULT uuid_generate_v4()                  |
| retrospective_id | UUID          | NOT NULL, REFERENCES Retrospective(id) ON DELETE CASCADE |
| category         | note_category | NOT NULL                                                 |
| content          | TEXT          | NOT NULL                                                 |
| created_at       | TIMESTAMPTZ   | NOT NULL, DEFAULT now()                                  |

## Suggestion

| Kolumna          | Typ               | Ograniczenia                                             |
| ---------------- | ----------------- | -------------------------------------------------------- |
| id               | UUID              | PRIMARY KEY, DEFAULT uuid_generate_v4()                  |
| retrospective_id | UUID              | NOT NULL, REFERENCES Retrospective(id) ON DELETE CASCADE |
| suggestion_text  | TEXT              | NOT NULL                                                 |
| status           | suggestion_status | NOT NULL, DEFAULT 'pending'                              |
| created_at       | TIMESTAMPTZ       | NOT NULL, DEFAULT now()                                  |

## AuditLog

| Kolumna          | Typ         | Ograniczenia                                    |
| ---------------- | ----------- | ----------------------------------------------- |
| id               | BIGSERIAL   | PRIMARY KEY                                     |
| timestamp        | TIMESTAMPTZ | NOT NULL, DEFAULT now()                         |
| user_id          | UUID        | REFERENCES AspNetUsers(Id) ON DELETE SET NULL   |
| level            | TEXT        | NOT NULL                                        |
| message          | TEXT        | NOT NULL                                        |
| retrospective_id | UUID        | REFERENCES Retrospective(id) ON DELETE SET NULL |

2. Relacje między tabelami

- AspNetUsers 1—N Retrospective (user_id)
- Retrospective 1—N Note (retrospective_id)
- Retrospective 1—N Suggestion (retrospective_id)
- AspNetUsers 1—N AuditLog (user_id)
- Retrospective 1—N AuditLog (retrospective_id)

3. Indeksy

```sql
CREATE INDEX idx_retrospective_user ON Retrospective(user_id);
CREATE INDEX idx_note_retrospective ON Note(retrospective_id);
CREATE INDEX idx_suggestion_retrospective ON Suggestion(retrospective_id);
CREATE INDEX idx_auditlog_user ON AuditLog(user_id);
CREATE INDEX idx_auditlog_retrospective ON AuditLog(retrospective_id);
```

4. Wszelkie dodatkowe uwagi lub wyjaśnienia dotyczące decyzji projektowych

- Należy utworzyć typy enum:
  ```sql
  CREATE TYPE note_category AS ENUM ('improvement_area', 'observation', 'success');
  CREATE TYPE suggestion_status AS ENUM ('pending', 'accepted', 'rejected');
  ```
- Do generowania UUID z użyciem `uuid_generate_v4()` włączyć rozszerzenie `uuid-ossp` (lub `pgcrypto` dla `gen_random_uuid()`).
- Enum `note_category` i `suggestion_status` zapewniają integralność danych na poziomie bazy.
- Klucze obce z `ON DELETE CASCADE` gwarantują usunięcie powiązanych rekordów Retrospective.
- `ON DELETE SET NULL` dla AuditLog zachowuje logi przy usunięciu użytkownika lub retrospektywy.
- Pola `accepted_count` i `rejected_count` zarządzane w warstwie aplikacji, aby uniknąć triggerów w bazie.
