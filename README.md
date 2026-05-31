# myCV

Personal bilingual CV/resume web application. A visitor can view my CV in English or Spanish — the backend reads a `.docx` file, parses it into structured data, and translates it via DeepL when a non-English locale is selected.

Built under human supervision using [opencode](https://opencode.ai) with `opencode/deepseek-v4-flash-free` as the LLM model. Every line is reviewed and directed — no "vibe coding."

## Stack

| Layer | Technology |
|-------|-----------|
| Frontend | Vanilla TypeScript, Vite 6 |
| Backend | .NET 10 |
| Translation | DeepL API (server-side, memory-cached) |
| Data | `.docx` file parsed at runtime — no database |

## Quick start

```bash
cd frontend && npm run dev   # starts both services concurrently
```

Launches frontend (port 5173) and backend (ports 60354/60355). Vite proxies `/api` to the backend in dev mode — no CORS issues.

```bash
cd frontend
npm run build        # tsc + vite build
npm run preview      # preview production build

cd backend
dotnet build Backend.slnx
dotnet test Backend.slnx      # 36 tests, 0 warnings
```

## Architecture

```
myCV/
├── frontend/              TypeScript + Vite
│   ├── domain/            Entities, repository interfaces
│   ├── application/       Use cases
│   ├── infrastructure/    API repository, translations (en/es), circuit breaker + retry
│   ├── ui/                DOM API components, renderer
│   ├── core/              Translation service, theme manager, animations
│   ├── public/            Static assets (favicon, etc.)
│   ├── main.ts            Entry point, manual DI wiring
│   └── index.html         Shell with CSP meta tag
│
├── backend/               .NET 10 (single project, folder layers)
│   └── src/
│       ├── Domain/        Entities, exceptions, repository & service interfaces
│       ├── Application/   DTOs, GetCV use case, mappings, DI registration
│       ├── Infrastructure/ WordCvSource (.docx parser), DeepLTranslationService,
│       │                   DiscordNotifier, CVRepository, options, DI registration
│       ├── Api/           CvController, GlobalExceptionHandler, API versioning,
│       │                   CORS, Swagger, DI registration, launch settings
│       ├── Program.cs     Middleware pipeline (rate limiter, CORS, security headers, HSTS, Serilog)
│       └── Dockerfile     Multi-stage container build
│
└── .github/workflows/      CI (automatic build + test) + CD (manual deploy via SSH)
```

## How it works

1. **Data source**: CV content lives in a password-protected `.docx` file outside the repo. The path is configured via `CvSource:FilePath` in `appsettings.json` or an environment variable.
2. **Parsing**: `WordCvSource` reads the `.docx` using `DocumentFormat.OpenXml` and extracts labeled sections (Name, Summary, Experiences, Skills, etc.).
3. **Serving**: `CvController` returns the CV as JSON via `GET /api/v1/cv`. Responses are cached for 1 hour.
4. **Translation**: When the `Accept-Language` header is non-English and a DeepL API key is configured, the backend translates all CV fields into the target language and caches the result per language (default 24h TTL).
5. **Frontend**: Static TypeScript app with manual DI. Fetches CV from the API on load, re-fetches on locale switch. UI chrome (nav, labels, footer) uses `t()` from a minimal translation service. CV data fields are displayed directly from the API response — no client-side CV translation.
6. **Resilience**: Frontend has a circuit breaker (3 failures → 30s open → half-open), retry with exponential backoff (2 attempts), and in-memory cache. Offline fallback shows a localized message.
7. **Security**: Rate limiter (100 req/min), file size cap (5 MB), UNC path rejection, HSTS, security headers (`X-Content-Type-Options`, `X-Frame-Options`, `Referrer-Policy`), CSP in `index.html`. Production bundle is obfuscated.

## Key features

- **Bilingual**: English (default) / Spanish — locale persisted in `localStorage`
- **Dark mode**: Theme toggle with instant icon swap, prefers-color-scheme auto-detection
- **Responsive**: Design system with CSS custom properties, reduced-motion support
- **API versioning**: URL segment versioning (`api/v1/…`), Swagger UI at `/swagger`
- **Observability**: Serilog request logging, health endpoint (`GET /health`), Discord alerts on CV source errors
- **Dockerized**: Multi-stage `Dockerfile`, non-root user, published to GitHub Container Registry
- **CI/CD**: GitHub Actions — CI on every push (build + test), CD manually triggered with SSH password

## Configuration

| Setting | Description |
|---------|-------------|
| `VITE_API_URL` | Frontend API URL (`.env.development` = `/api/v1/cv`, `.env.production` = `https://cv-api.s3rbr0p4r.com/api/v1/cv`) |
| `CvSource:FilePath` | Absolute path to the `.docx` CV file |
| `DeepL:AuthKey` | DeepL API authentication key (optional — English-only when empty) |
| `Discord:WebhookUrl` | Discord webhook for CV source error alerts (optional) |
| `AllowedHosts` | Production host filter (`cv-api.s3rbr0p4r.com`); overridden to `*` in development |
| `Cors:AllowedOrigins` | CORS allowed origin URLs |
| `Cors:FrontendUrl` | Additional frontend URL appended to CORS origins (optional) |

## License

All rights reserved.
