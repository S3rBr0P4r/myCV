# myCV

Personal CV/resume web application with a bilingual (English/Spanish) frontend and a .NET backend serving the CV data.

## Stack

| Layer | Technology |
|-------|-----------|
| Frontend | TypeScript, Vite, Clean Architecture |
| Backend | .NET 10, Clean Architecture, Controllers, Swagger |
| Database | None — CV data is embedded as structured source |

## Quick start

```bash
cd frontend
npm run dev
```

Launches both frontend (port 5173) and backend (ports 60354/60355) concurrently.

## Structure

```
frontend/     TypeScript + Vite
  domain/       Entities, repositories interfaces
  application/  Use cases (CQRS-lite)
  infrastructure/  API repository, translations (en/es)
  ui/           Components, renderer, styles

backend/      .NET 10
  src/
    Domain/       Entities, interfaces
    Application/  DTOs, use cases
    Infrastructure/  Persistence, CV data source
    Api/          Controllers, middleware, Swagger
```
