# myCV

Personal bilingual CV website. Backend parses a `.docx` file and translates via DeepL on demand. Frontend renders it with dark mode, pagination, and career cards.

Built with [opencode](https://opencode.ai) under human supervision.

## Stack

| Layer | Technology |
|-------|-----------|
| Frontend | React 18 + TypeScript, Vite 8, Vitest |
| Backend | .NET 10, xUnit + Moq + FluentAssertions |
| Translation | DeepL API (server-side, memory-cached) |
| Data | `.docx` parsed at runtime — no database |

## Quick start

```bash
cd frontend && npm run dev   # starts both (FE :5173, BE :60355)
```

| Command | Action |
|---------|--------|
| `npm run test` | Vitest |
| `npm run build` | Production build |
| `dotnet test Backend.slnx` | 140+ tests |

## Architecture

```
myCV/
├── frontend/              React + TypeScript + Vite
│   ├── domain/            Entities, repository interfaces
│   ├── application/       Use cases
│   ├── infrastructure/    API repository, circuit breaker, retry
│   ├── ui/                Components, hooks, contexts, App
│   ├── styles/            Design system (14 CSS files)
│   └── public/            Favicon, backgrounds, flags, logos
│
├── backend/               .NET 10 (single project, folder layers)
│   ├── src/
│   │   ├── Domain/        Entities, exceptions, interfaces
│   │   ├── Application/   DTOs, GetCV use case, mappings
│   │   ├── Infrastructure/ WordCvSource, DeepL, Discord, CVRepository
│   │   └── Api/           CvController, middleware, Program.cs
│   ├── Dockerfile
│   └── tests/             Domain (5), Application (59), Integration (14)
│
└── .github/workflows/     CI (build+test+Docker) + CD (manual SSH deploy)
```

## How it works

1. **`.docx` → JSON**: `WordCvSource` parses the CV file into structured data at runtime.
2. **`GET /api/v1/cv`**: Returns the CV as JSON, optionally translated via DeepL based on `Accept-Language`.
3. **Frontend**: Fetches CV on load, re-fetches on locale switch. Circuit breaker + retry for resilience.
4. **Security**: Rate limiter, CSP headers, path traversal guard, file size cap (5 MB), Discord alerts on error.

## Key features

- Bilingual (EN/ES, no auto-detect), dark mode, reduced-motion support
- API versioning, Swagger, health endpoint, Serilog logging
- Dockerized (non-root, GHA cache), CI/CD via GitHub Actions
- 0 npm vulns, `TreatWarningsAsErrors`, stable packages only

## Configuration

| Setting | Description |
|---------|-------------|
| `VITE_API_URL` | API URL (dev: `/api/v1/cv`, prod: CI var) |
| `CvSource__FilePath` | Path to `.docx` |
| `DeepL__AuthKey` | DeepL API key (optional, secret) |
| `Discord__WebhookUrl` | Error alert webhook (optional, secret) |
| `SocialLinks__LinkedIn` | LinkedIn URL (injected into CV response) |
| `SocialLinks__GitHub` | GitHub URL (injected into CV response) |

## License

MIT — see [LICENSE](./LICENSE).
