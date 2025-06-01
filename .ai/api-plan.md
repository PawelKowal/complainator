# REST API Plan

## 1. Resources

- **Users** (AspNetUsers)
- **Retrospectives** (Retrospective)
- **Notes** (Note)
- **Suggestions** (Suggestion)
- **AuditLogs** (AuditLog)

## 2. Endpoints

### 2.1 Authentication

#### POST /auth/register

- Description: Register a new user with email and password
- Request JSON:
  {
  "email": "user@example.com", // string, required, valid email
  "password": "string" // string, required, min length 8
  }
- Response 201:
  {
  "token": "<JWT>",
  "user": { "id": "<UUID>", "email": "user@example.com" }
  }
- Errors:
  400 Bad Request (validation errors)
  409 Conflict (email already exists)

#### POST /auth/login

- Description: Authenticate user and issue JWT
- Request JSON:
  {
  "email": "user@example.com", // string, required
  "password": "string" // string, required
  }
- Response 200:
  {
  "token": "<JWT>",
  "user": { "id": "<UUID>", "email": "user@example.com" }
  }
- Errors:
  400 Bad Request (validation errors)
  401 Unauthorized (invalid credentials)

### 2.2 Retrospectives

#### GET /retrospectives

- Description: List all retrospectives for authenticated user
- Query Params:
  - `page` (integer, default: 1)
  - `perPage` (integer, default: 10)
  - `sort` (string, `date_desc` or `date_asc`, default: `date_desc`)
- Response 200:
  {
  "items": [
  {
  "id": "<UUID>",
  "name": "Retrospektywa #3 - 01.06.2025",
  "date": "2025-06-01",
  "acceptedSuggestions": [
  { "id": "<UUID>", "suggestionText": "Poprawić dokumentację API" },
  { "id": "<UUID>", "suggestionText": "Usprawnić proces release’u" }
  ]
  },
  ...
  ],
  "total": 42,
  "page": 1,
  "perPage": 10
  }
- Errors:
  401 Unauthorized

#### POST /retrospectives

- Description: Create a new retrospective with auto-generated name and current date
- Request JSON: {} (no body)
- Response 201:
  { "id": "<UUID>", "name": "Retrospektywa #4 - 02.06.2025", "date": "2025-06-02", "acceptedCount": 0, "rejectedCount": 0 }
- Errors:
  401 Unauthorized
  500 Internal Server Error

#### GET /retrospectives/{id}

- Description: Retrieve details of a specific retrospective, including notes and accepted suggestions
- Path Params:
  - `id` (UUID, required)
- Response 200:
  {
  "id": "<UUID>",
  "name": "...",
  "date": "2025-06-02",
  "notes": {
  "improvement_area": [ { "id": "<UUID>", "content": "..." }, ... ],
  "observation": [ ... ],
  "success": [ ... ]
  },
  "acceptedSuggestions": [ { "id": "<UUID>", "suggestionText": "..." }, ... ]
  }
- Errors:
  401 Unauthorized
  404 Not Found (not owned or not exists)

### 2.3 Notes

#### POST /retrospectives/{id}/notes

- Description: Add a new note to a retrospective
- Path Params: `id` (UUID)
- Request JSON:
  {
  "category": "improvement_area" | "observation" | "success",
  "content": "string" // required
  }
- Response 201:
  { "id": "<UUID>", "category": "improvement_area", "content": "string", "createdAt": "..." }
- Errors:
  400 Bad Request (validation)
  401 Unauthorized
  404 Not Found

### 2.4 AI Suggestions

#### POST /retrospectives/{id}/generate-suggestions

- Description: Generate AI suggestions based on notes
- Path Params: `id` (UUID)
- Request JSON: {}
- Response 200:
  { "suggestions": [ { "id": "<UUID>", "suggestionText": "...", "status": "pending" }, ... ] }
- Errors:
  401 Unauthorized
  404 Not Found
  500 Internal Server Error (AI service)

#### PATCH /suggestions/{suggestionId}

- Description: Update suggestion status (accept or reject)
- Path Params: `suggestionId` (UUID)
- Request JSON:
  { "status": "accepted" | "rejected" }
- Response 200:
  { "id": "<UUID>", "status": "accepted", "acceptedCount": 3, "rejectedCount": 1 }
- Errors:
  400 Bad Request (invalid status)
  401 Unauthorized
  404 Not Found

## 3. Authentication and Authorization

- Mechanism: JWT Bearer tokens issued via ASP.NET Identity + JWT (JWT tokens in `Authorization: Bearer <token>` header)
- Public endpoints: `/auth/register`, `/auth/login`
- All other endpoints require valid JWT
- Users can only access their own retrospectives, notes, and suggestions (enforce via userId claims)

## 4. Validation and Business Logic

- **Retrospective**:
  - `id`, `userId`: UUID, auto-generated
  - `name`: non-empty, auto-generated in format `Retrospektywa #{number} - DD.MM.YYYY`
  - `date`: auto-set to current date
- **Note**:
  - `category`: enum [`improvement_area`, `observation`, `success`]
  - `content`: non-empty text
- **Suggestion**:
  - `suggestionText`: non-empty text
  - `status`: enum [`pending`, `accepted`, `rejected`], default `pending`
  - On status update, increment retrospective's `acceptedCount` or `rejectedCount`
- Edge cases:
  - Cannot delete or update notes or retrospectives per product boundaries
  - Idempotent suggestion generation: repeated calls replace pending suggestions or return existing pending
- Performance:
  - Pagination on list endpoints
  - Sorting via database indexes on `user_id`, `retrospective_id`
  - Rate limiting assumed via API gateway / hosting configuration
