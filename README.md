# Complainator

> Accelerate and simplify sprint retrospectives with AI-powered insights.

## Table of Contents

1. [Project Description](#project-description)
2. [Tech Stack](#tech-stack)
3. [Getting Started Locally](#getting-started-locally)
4. [Available Scripts](#available-scripts)
5. [Project Scope](#project-scope)
6. [Project Status](#project-status)
7. [License](#license)

## Project Description

Complainator is a web application designed to streamline the creation of sprint retrospectives and generate actionable insights using AI.  
Users can:

- Automatically create retrospectives named: `Retrospektywa #{number} – DD.MM.YYYY`.
- Add notes in three categories:
  - **What needs improvement**
  - **Observations**
  - **What went well**
- Generate a full AI-driven summary of insights.
- Review, accept, or reject each AI suggestion.
- Browse a dashboard listing all retrospectives with names, dates, and full AI summaries.
- View detailed retrospective pages with categorized notes and accepted insights.
- Authenticate via email/password (ASP.NET Identity + JWT).
- Benefit from global error handling (friendly "Wystąpił błąd, spróbuj ponownie później" banner).

## Tech Stack

- **Frontend**
  - React 19
  - TypeScript 5
  - Material-UI
- **Backend**
  - .NET Web API
  - RESTful endpoints
  - SQLite (Entity Framework Core)
  - ASP.NET Identity & JWT authentication
- **AI Integration**
  - Openrouter.ai
- **Testing**
  - NUnit & NSubstitute (Backend unit tests)
  - Jest & React Testing Library (Frontend unit tests)
  - Playwright (E2E tests)
  - Postman (API testing)
- **CI/CD & Hosting**
  - GitHub Actions
  - DigitalOcean

## Getting Started Locally

### Prerequisites

- [.NET SDK 6.0+ or 7.0+](https://dotnet.microsoft.com/download)
- [Node.js 16+](https://nodejs.org/) & npm
- SQLite (bundled via EF Core)
- An Openrouter.ai API key

### Environment Setup

1. Clone the repository
   ```bash
   git clone https://github.com/PawelKowal/complainator.git
   cd complainator
   ```
2. Create environment configuration:

   - Backend (`appsettings.Development.json` or `.env`):
     ```jsonc
     {
       "ConnectionStrings": {
         "DefaultConnection": "Data Source=complainator.db"
       },
       "Openrouter": {
         "ApiKey": "YOUR_OPENROUTER_API_KEY"
       }
     }
     ```
   - Frontend (`ClientApp/.env`):
     ```bash
     REACT_APP_OPENROUTER_API_KEY=YOUR_OPENROUTER_API_KEY
     ```

### Installation & Run

1. **Backend**

   ```bash
   # Restore & run migrations
   dotnet restore
   dotnet ef database update

   # Run API
   dotnet run
   ```

2. **Frontend**
   ```bash
   cd ClientApp
   npm install
   npm start
   ```

Navigate to `http://localhost:3000` (frontend) and `http://localhost:5000` (API) by default.

## Available Scripts

### Frontend (ClientApp)

- `npm start`  
  Start the development server (hot-reload at `localhost:3000`).

- `npm test`  
  Launch the test runner (Jest).

- `npm run build`  
  Build the app for production to `ClientApp/build/`.

### Backend

- `dotnet run`  
  Build and run the Web API (default URL: `http://localhost:5000`).

- `dotnet ef migrations add <Name>`  
  Create a new EF Core migration.

- `dotnet ef database update`  
  Apply pending migrations.

- `dotnet test`  
  Run backend unit tests.

## Project Scope

### In Scope

- Automatic retrospective creation with incremented numbering and current date.
- Note-taking in three categories.
- AI-powered summary generation and review.
- Dashboard for listing retrospectives.
- Detailed retrospective views.
- Email/password authentication.
- Global error banner.
- Persistence of metadata (`createdAt`, `acceptedCount`, `rejectedCount`).
- Responsive design for desktop/tablet.

### Out of Scope

- Multi-user collaboration within a single retrospective.
- Mobile/native app.
- Media attachments (images, videos).
- Editing or deleting notes after creation.
- Archiving or removing completed retrospectives.
- Password reset or email verification.

## Project Status

This project is currently in **active development** (v0.1.0-alpha).

- Targeting ≥80% unit test coverage for key logic.
- Aiming for AI suggestion acceptance rate ≥50% as a measure of quality.

## License

This project is licensed under the **MIT License**.
