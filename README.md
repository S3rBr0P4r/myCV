# myCV

Personal bilingual CV website. Backend parses a `.docx` file, translates via DeepL on demand, collects viewer feedback. Frontend renders it with dark mode, pagination, career cards, and a feedback FAB.

Built with [opencode](https://opencode.ai) under human supervision.

## Stack

| Layer | Technology |
|-------|-----------|
| Frontend | React 18 + TypeScript, Vite 8, Vitest |
| Backend | .NET 10, xUnit + Moq + FluentAssertions |
| Translation | DeepL API (server-side, memory-cached) |
| Alerts | Discord webhook (errors, 1h cooldown) |
| Feedback | Second Discord webhook (no cooldown) |
| Data | `.docx` parsed at runtime — no database |

## Quick start

```bash
cd frontend && npm run dev   # starts both services
```

| Command | Action |
|---------|--------|
| `npm run test` | Vitest |
| `npm run build` | Production build |
| `dotnet test Backend.slnx` | 160+ tests |

## Architecture

```
myCV/
├── frontend/              React + TypeScript + Vite
│   ├── domain/            Entities, repository interfaces
│   ├── application/       Use cases
│   ├── infrastructure/    API repository, circuit breaker, retry
│   ├── ui/                Components, hooks, contexts, App
│   │   └── components/
│   │       └── Feedback.tsx   FAB → modal → toast flow
│   ├── styles/            Design system (15 CSS files)
│   │   └── feedback.css   FAB, overlay, modal, stars, toast
│   └── public/
│       └── errors/        backend_not_responding.webp
│
├── backend/               .NET 10 (single project, folder layers)
│   ├── src/
│   │   ├── Domain/        Entities, exceptions, interfaces
│   │   ├── Application/   DTOs (incl. FeedbackRequest), GetCV use case, mappings
│   │   ├── Infrastructure/ WordCvSource, DeepL, DiscordErrorNotifier,
│   │   │                  DiscordFeedbackNotifier, CVRepository
│   │   └── Api/           CvController, FeedbackController, middleware, Program.cs
│   ├── Dockerfile
│   └── tests/             Domain (5), Application (73), Integration (21)
│
└── .github/workflows/     CI (build+test+Docker) + CD (manual SSH deploy)
```

## How it works

1. **`.docx` → JSON**: `WordCvSource` parses the CV file into structured data at runtime.
2. **`GET /api/v1/cv`**: Returns the CV as JSON, optionally translated via DeepL based on `Accept-Language`.
3. **`POST /api/v1/feedback`**: Stores viewer feedback (name, rating, country, comment) and forwards it to a Discord webhook as a green embed.
4. **Frontend**: Fetches CV on load, re-fetches on locale switch. Resets to a friendly offline page when the backend is unreachable. Feedback FAB opens a modal with star rating, name, and optional comment; submission shows a toast notification.
5. **Alerts**: Errors (DOCX parse failure, DeepL failures, path traversal) send a red embed to a separate Discord webhook with a 1-hour cooldown.

## Key features

- Bilingual (EN/ES, no auto-detect), dark mode, reduced-motion support
- API versioning, Swagger, health endpoint, Serilog logging
- Viewer feedback collection (star rating, auto-detected country, optional comment)
- Two Discord webhooks: error alerts (1h cooldown) + feedback (no cooldown)
- Offline fallback with localized Ghibli-inspired error page
- Dockerized (non-root, GHA cache), CI/CD via GitHub Actions
- 0 npm vulns, `TreatWarningsAsErrors`, stable packages only

## Configuration

| Setting | Description |
|---------|-------------|
| `VITE_API_URL` | API URL (dev: `/api/v1/cv`, prod: CI var) |
| `CvSource__FilePath` | Path to `.docx` |
| `DeepL__AuthKey` | DeepL API key (optional, secret) |
| `Discord__ErrorWebhookUrl` | Error alert webhook (optional, secret) |
| `Discord__FeedbackWebhookUrl` | Feedback webhook (optional, secret) |
| `SocialLinks__LinkedIn` | LinkedIn URL (injected into CV response) |
| `SocialLinks__GitHub` | GitHub URL (injected into CV response) |

## License

MIT — see [LICENSE](./LICENSE).
