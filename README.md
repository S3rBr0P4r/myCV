# myCV

Personal bilingual CV/resume web application. A visitor can view my CV in English or Spanish — the backend reads a `.docx` file, parses it into structured data, and translates it via DeepL when a non-English locale is selected.

Built under human supervision using [opencode](https://opencode.ai) with `opencode/deepseek-v4-flash-free` as the LLM model. Every line is reviewed and directed — no "vibe coding."

## Stack

| Layer | Technology |
|-------|-----------|
| Frontend | React 18 + TypeScript, Vite 8, Vitest |
| Backend | .NET 10, xUnit + Moq + FluentAssertions |
| Translation | DeepL API (server-side, memory-cached) |
| Data | `.docx` file parsed at runtime — no database |

## Quick start

```bash
cd frontend && npm run dev   # starts both services concurrently
```

Launches frontend (port 5173) and backend (port 60355). Vite proxies `/api` to the backend in dev mode — no CORS issues.

```bash
cd frontend
npm run build        # tsc + vite build
npm run test         # vitest (unit + integration)
npm run test:coverage
npm run preview      # preview production build

cd backend
dotnet build Backend.slnx
dotnet test Backend.slnx      # 78 backend + 62 frontend = 140+ tests, 0 warnings
```

## Architecture

```
myCV/
├── frontend/              React 18 + TypeScript + Vite
│   ├── domain/            Entities, repository interfaces
│   ├── application/       Use cases
│   ├── infrastructure/    API repository, translations (en/es), circuit breaker + retry
│   ├── ui/
│   │   ├── components/    React components (AnimatedBackground, ErrorBoundary,
│   │   │                   Experience, Footer, Header, Intro, Skills)
│   │   ├── hooks/         Custom React hooks (useCV, useParallax, useScrollReveal,
│   │   │                   useTheme, useTranslation)
│   │   ├── contexts/      React contexts (CV, Theme, Translation)
│   │   ├── CompanyData.ts  Company lookup (URL, image, logo maps)
│   │   ├── CompanyMatch.ts Factory helpers (accent-normalized fuzzy matching)
│   │   ├── format.ts      Text formatting helpers (bold, links)
│   │   └── App.tsx        Root component
│   ├── styles/            Design system (14 component CSS files, design-tokens, animations)
│   ├── public/            Static assets (favicon, backgrounds/, flags/, logos/)
│   ├── main.tsx           Entry point with React root + provider chain
│   └── index.html         Shell with CSP meta tag
│
├── backend/               .NET 10 (single project, folder layers)
│   ├── src/
│   │   ├── Domain/        Entities, exceptions, repository & service interfaces
│   │   ├── Application/   DTOs, GetCV use case, mappings, DI registration
│   │   ├── Infrastructure/ WordCvSource (.docx parser), DeepLTranslationService,
│   │   │                   DiscordNotifier, CVRepository, options, DI registration
│   │   ├── Api/           CvController, GlobalExceptionHandler, SecurityHeadersMiddleware,
│   │   │                   API versioning, CORS, Swagger, DI registration, launch settings
│   │   └── Program.cs     Middleware pipeline (rate limiter, CORS, security headers,
│   │                        response compression, HSTS, Serilog)
│   ├── Dockerfile         Multi-stage container build
│   └── tests/
│       ├── Domain.Tests/       Entity + exception unit tests
│       ├── Application.Tests/  Use case + service + middleware unit tests (DI, mappings, repo)
│       └── Integration.Tests/  Full HTTP pipeline via WebApplicationFactory
│
└── .github/workflows/      CI (automatic build + test) + CD (manual deploy via SSH)
```

## How it works

1. **Data source**: CV content lives in a `.docx` file at `Infrastructure/Data/cv.docx` (gitignored). The path is configured via `CvSource:FilePath` in `appsettings.Development.json` or an environment variable.
2. **Parsing**: `WordCvSource` reads the `.docx` using `DocumentFormat.OpenXml` and extracts labeled sections (Name, Summary, Experiences with company URL/location/work mode, Skills, etc.).
3. **Serving**: `CvController` returns the CV as JSON via `GET /api/v1/cv`. Responses are cached for 1 hour via `ResponseCache`.
4. **Translation**: When the `Accept-Language` header is non-English and a DeepL API key is configured, the backend translates all CV fields into the target language and caches the result per language (default 24h TTL). English pass-through when no key configured.
5. **Frontend**: React 18 app with contexts (CV, Theme, Translation) and custom hooks (`useCV`, `useTheme`, `useTranslation`, `useParallax`, `useScrollReveal`). Components are functional-only. Fetches CV from the API on load, re-fetches on locale switch. UI chrome uses `t()` from the translation context. CV data fields come directly from the API response — no client-side translation.
6. **Resilience**: Frontend has a circuit breaker (3 failures → 30s open → half-open), retry with exponential backoff (2 attempts), and locale-aware in-memory cache. Falls back to a hardcoded offline placeholder on repeated failure. Tests via Vitest + @testing-library/react + happy-dom (140+ tests across both projects).
7. **Security**: Rate limiter (100 req/min), file size cap (5 MB), UNC path rejection, HSTS, `SecurityHeadersMiddleware` (`X-Content-Type-Options`, `X-Frame-Options`, `Referrer-Policy`, `Permissions-Policy`), CSP defense-in-depth on both frontend meta tag and backend header.
8. **Career cards**: Company logos via local WebP files (initials SVG fallback via accent-normalized prefix matching), Pexels background images, location/work mode badges, paginated (2 per page).

## Key features

- **Bilingual**: English (default) / Spanish — locale persisted in `localStorage`, no auto-detection
- **Dark mode**: Theme toggle with no-transition lock, `prefers-color-scheme` auto-detection on first visit
- **Responsive**: Design system with CSS custom properties, reduced-motion support, grain noise overlay
- **API versioning**: URL segment versioning (`api/v1/…`), Swagger UI at `/swagger`
- **Observability**: Serilog request logging, health endpoint (`GET /health`), Discord alerts on CV source errors (1h cooldown)
- **Dockerized**: Multi-stage `Dockerfile`, targeted COPY for layer caching, non-root user, GHA cache
- **CI/CD**: GitHub Actions — CI on every push + manual dispatch (build, test, Docker push), CD manually deployed via SSH key
- **Code quality**: All packages stable (no prereleases), `TreatWarningsAsErrors` + `EnforceCodeStyleInBuild` on backend, `npm audit` 0 vulnerabilities, 140+ tests

## Configuration

| Setting | Description | Source |
|---------|-------------|--------|
| `VITE_API_URL` | Frontend API URL (dev = `/api/v1/cv`, production = CI variable) | CI/CD `vars.*` |
| `VITE_CSP_SCRIPT_SRC` | Content Security Policy `script-src` (production: `'self'`) | CI/CD `vars.*` |
| `VITE_CSP_STYLE_SRC` | Content Security Policy `style-src` (production: `'self' 'unsafe-inline'`) | CI/CD `vars.*` |
| `VITE_CSP_CONNECT_SRC` | Content Security Policy `connect-src` (production: `'self'`) | CI/CD `vars.*` |
| `AllowedHosts` | Production host filter (dev overrides to `*`) | Environment variable |
| `CvSource__FilePath` | Path to the `.docx` CV file | `appsettings.Development.json` or env var |
| `CvSource__AllowedDirectory` | Directory allowed for CV file access (path traversal guard) | `appsettings.Development.json` or env var |
| `DeepL__AuthKey` | DeepL API key (optional — English pass-through when empty) | Environment variable (secret) |
| `Discord__WebhookUrl` | Discord webhook for CV source error alerts (optional, 1h cooldown) | Environment variable (secret) |
| `Cors__AllowedOrigins` | CORS allowed origin URLs | `appsettings.Development.json` or env var |
| `Cors__FrontendUrl` | Additional frontend URL appended to CORS origins (optional) | `appsettings.Development.json` or env var |
| `SocialLinks__LinkedIn` | LinkedIn profile URL (injected into CV response) | `appsettings.Development.json` or env var |
| `SocialLinks__GitHub` | GitHub profile URL (injected into CV response) | `appsettings.Development.json` or env var |

> Use .NET's `__` (double underscore) separator for nested keys when setting environment variables. These override the defaults from `appsettings.json` automatically.

## License

MIT — see [LICENSE](./LICENSE) for details.
