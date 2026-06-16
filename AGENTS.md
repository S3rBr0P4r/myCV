# myCV — AGENTS.md

## Structure

Two independent projects at the root:

```
frontend/    React + TypeScript + Vite
backend/     .NET 10 Clean Architecture
```

## Frontend (`frontend/`)

| Command | Action |
|---------|--------|
| `npm run dev` | Vite dev server + backend |
| `npm run build` | `tsc && vite build` |
| `npm run preview` | Vite preview |
| `npm run test` | `vitest run` |
| `npm run test:watch` | `vitest` (watch mode) |
| `npm run test:coverage` | `vitest run --coverage` |

**Vulnerability fixes:**
- Vite upgraded `^5.0.0` → `^6.0.0` → `^8.0.0` (Rolldown-powered) to resolve esbuild advisory (GHSA-67mh-4wv8-2f99) and reduce transitive deps from 151→111.
- `@vitejs/plugin-react` upgraded `^4.7.0` → `^5.0.0` → `^6.0.2` (uses `@rolldown/plugin-babel` instead of `@babel/core` directly; removes `gensync@1.0.0-beta.2` transitive).
- `vitest` upgraded from `^2.1.9` → `^4.1.8` and `happy-dom` from `^15.11.7` → `^20.10.1` to resolve 6 CVEs (2 critical, 4 moderate).
- Run `npm audit fix` to stay current.

No linter/formatter beyond `.editorconfig` (2-space indent, single quotes for TS).

**Architecture:** Clean Architecture layers — `domain/` → `application/` → `infrastructure/` → `ui/` (React components + hooks + contexts). Manual DI via hooks + React contexts in `main.tsx`.

**React conventions:**
- Functional components only (no classes). File extension `.tsx` for components, `.ts` for pure functions.
- Custom hooks in `src/ui/hooks/` with `use*` prefix (e.g. `useTheme`, `useTranslation`).
- Contexts in `src/ui/contexts/` (Theme, Translation, CV).
- No CSS-in-JS — styles live in component files under `styles/` imported via `design-system.css` manifest + `animations.css`.
- No state management library (Redux/Zustand) — local state + contexts only.
- Domain/application/infrastructure layers remain framework-agnostic `.ts` files.

**CSS file structure (`frontend/src/styles/`):**
```
design-system.css        Manifest — @imports all component files
design-tokens.css        CSS variables + dark theme
reset.css                Box-sizing, body, grain noise, vignette
typography.css           h1-h4, p
navbar.css               Navbar, social-links, theme-toggle, locale-dropdown
scroll-progress.css      Side navigation dots
intro.css                #intro, .main-title, .intro-description
section-common.css       Section, .section-title, .painted-bg
experience.css           Cards, layout, work-mode badges
experience-detail.css   Pagination, description list, links
skills.css               Skill grid, categories, items
contact.css              #contact, .contact-card, .email-link
feedback.css             FAB, overlay, modal, star rating, textarea, toast
lang-transition.css      Language switch shimmer + pulse animation
footer.css               Footer, .footer-attribution
utilities.css            Scrollbar, transition-lock, reduced-motion
animations.css           Stagger + scroll-reveal keyframes
```
Each file ≤ 250 lines. Add new component styles as separate files, never append to `design-system.css`.

**Testing (Vitest + happy-dom + @testing-library/react):**
- Tests live in `tests/` mirroring `src/` structure (mirrors backend layout):
  - `tests/ui/` for `src/ui/` components/hooks/contexts
  - `tests/infrastructure/` for `src/infrastructure/`
- Component tests use `render`, `screen`, `fireEvent` from `@testing-library/react`.
- Custom hook tests use `renderHook` from `@testing-library/react`.
- `happy-dom` environment for DOM simulation (no browser needed).
- CSS imports are auto-mocked (configured in vitest).
- Key mocks: `IntersectionObserver`, `ResizeObserver`, `fetch`.
- Test file naming: `{fileName}.test.ts` / `{componentName}.test.tsx`.
- Vitest configured in `vite.config.ts` under `test` block.

**API:** Configured via `VITE_API_URL` env var (dev = `/api/v1/cv` via Vite proxy, production = set via CI variable `VITE_API_URL`). Has offline fallback if backend is unreachable.

**Root-level dev command:** `cd frontend && npm run dev` starts both services (via `concurrently`).

**Docker build:** Multi-stage `Dockerfile` at `frontend/Dockerfile` — build stage with `node:22-alpine`, runtime with `nginx:alpine`. Nginx config in `frontend/nginx.conf` uses template syntax (`listen ${PORT};`) processed by nginx's built-in `envsubst` entrypoint. Pass `-e PORT=<value>` to set the listen port.

## Backend (`backend/`)

.NET 10 (SDK 10.0.203, prerelease allowed). Solution file: `Backend.slnx` (.slnx format). Three projects: `src/Backend.csproj` + two test projects.

| Command | Action |
|---------|--------|
| `dotnet restore Backend.slnx` | Restore packages |
| `dotnet build Backend.slnx --no-restore` | Build all projects |
| `dotnet test Backend.slnx --no-build` | Run all tests (unit + integration) |

**Build hardens:** `TreatWarningsAsErrors`, `AnalysisLevel: latest-recommended`, `EnforceCodeStyleInBuild` — fix all analyzer warnings to build. Every new class must have corresponding test coverage — mandatory, no exceptions.

**Folder structure (one project, layers as folders):**

```
src/Backend.csproj
  Domain/         Entities, Exceptions, Interfaces
    → Application/  DTOs, Mappings, UseCases
      → Infrastructure/  Persistence, Sources, Options, Services
        → Api/   Controllers, Middleware, DependencyInjection
tests/
  Domain.Tests/           Entity + exception unit tests
  Application.Tests/      Use case + service + middleware unit tests
    Helpers/               Shared test factories (CVTestDataFactory, DeepLTestFixture)
    Sources/               WordCvSourceTests + DocxTestDocumentBuilder
  Integration.Tests/      Full HTTP pipeline via WebApplicationFactory
```

**SOLID — Single Responsibility enforced:** Every production file must stay ≤ 250 lines. Large classes are split into focused helpers:

```
Infrastructure/Sources/
  WordCvSource.cs           Orchestrator: DI, caching, file validation
  CvDocumentReader.cs       Opens DOCX, iterates body, collects sections
  TextFormatter.cs          Bold + hyperlink extraction from paragraphs
  SectionHelper.cs          Section headers, map building, line extraction
  ContactParser.cs          ContactInfo from lines (email, phone, etc.)
  ExperienceParser.cs       Experience entries (new + legacy format)
  SkillsParser.cs           Skills from text lines or table
```

- Each layer registers its own DI via a static `DependencyInjection` class with extension method (`AddApplication()`, `AddInfrastructure()`), plus `AddApiServices()` in the API layer.
- DTOs are `record` types; mapping is hand-written extensions (no AutoMapper).
- Use cases follow CQRS-lite: `GetCVQuery` / `GetCVHandler` / `GetCVResult`.
- `GlobalExceptionHandler` middleware maps `DomainException` → 400, `NotFoundException` → 404.
- Namespace root: `Backend.*` (not `MyCV.*`).
- API uses `[ApiController]` + `[Route("api/v{version:apiVersion}/[controller]")]` with URL segment versioning. Current version: `v1`. Add `[ApiVersion("X.Y")]` to controllers. Swagger via `Swashbuckle.AspNetCore` (no `Microsoft.AspNetCore.OpenApi`).

**Backend launch URL:** `http://localhost` (from `Properties/launchSettings.json`). Only HTTP in development — firewall-friendly and matches Vite proxy scheme.

**CORS:** Allows `http://localhost`, `http://127.0.0.1`, `https://localhost` with `AllowCredentials()`.

**Editorconfig conventions (C#):** 4-space indent, CRLF, file-scoped namespaces, `_camelCase` private fields, `I` prefix for interfaces, `Async` suffix for async methods.

**Clean Code (C#):**
- **Single exit per method**: Validate early with guard clauses (`if (condition) return;`), then proceed. This avoids deep nesting and keeps the main path linear.
- **Extract condition logic**: Complex boolean expressions should be extracted into well-named private methods (e.g. `IsKnownWorkMode(value)`, `HasSectionHeader(lines)`).
- **Methods do one thing**: If a method contains a loop that does two different things, split it. Each loop, switch, or condition block should have a single responsibility.
- **Name by intent, not by format**: Parsing helpers must not use `Word` or `Docx` prefixes — they describe the domain structure, not the file format. The top-level orchestrator (e.g. `WordCvSource`) is the single exception; if the source format changes, create a new orchestrator and reader class rather than renaming existing ones.
- **Self-documenting code over comments**: No inline documentation comments (`//`, `///`) on implementation logic. Use expressive method/variable names instead. Reserve XML doc comments for public API surfaces.
- **Small private methods**: Keep each private method under ~30 lines. Validate-and-throw guard blocks should be extracted into named validation methods. Parsing state-machine steps should be extracted into focused helpers.

## CI (`.github/workflows/ci.yml`) + CD (`.github/workflows/cd.yml`)

Triggers on push to `main` (or manual `workflow_dispatch`). Concurrency group `ci-${{ github.ref }}` with `cancel-in-progress: true` prevents Docker tag races. Four jobs (frontend 10m, backend 10m, docker-api 15m, docker-frontend 15m timeouts):
- **Frontend:** `npm ci` → `npm run build` → `npm run test`
- **Backend:** `dotnet restore` → `dotnet build` → `dotnet test`
- **Docker API:** Builds and pushes API image to GHCR (only on `main`).
- **Docker Frontend:** Builds and pushes frontend image to GHCR (only on `main`). Uses `build-args` for `VITE_API_URL` and `VITE_CSP_*` vars.

**CD:** Manual `workflow_dispatch` or automatic after CI succeeds on `main` (via `workflow_run`). Four sequential jobs: `ensure-network` (5m) → `deploy-api` (10m) → `health-check` (5m) → `deploy-frontend` (10m). Each SSH job sets up its own keys. `deploy-frontend` only runs if `health-check` passes.
**Security:** All GitHub Actions pinned to commit SHA digests (with `# vX.Y.Z` version comment).
**Public repo hygiene:** Any infrastructure detail (hostnames, URLs, IPs) that appears in workflow logs must use `${{ secrets.* }}` (masked) instead of `${{ vars.* }}` (visible). The CD workflow's `API_HOST` is a secret for this reason. CI vars like `VITE_API_URL` are intentionally public (inlined into the website's JS bundle).

## Skills (`.opencode/skills/` + `.agents/skills/`)

| Skill | Purpose |
|-------|---------|
| `vite` | Vite config, build, plugins |
| `frontend-design` | Production-grade frontend design, aesthetic direction, design systems |
| `typescript-code-review` | TypeScript code review best practices |
| `dotnet-clean-architecture` | Clean Architecture project structure |
| `dotnet-domain-entity-generator` | DDD entity patterns |
| `dotnet-repository-pattern` | Repository pattern with EF Core |
| `moq-testing` | Unit testing with Moq + xUnit + FluentAssertions |
| `vercel-react-best-practices` | React performance patterns (installed from skills.sh at `.agents/skills/`) |

## Principles

- **DRY — Don't Repeat Yourself.** Any duplicated logic across both frontend and backend must be extracted into a shared module or utility. This applies to pure functions, type definitions, configuration maps, string normalization, and any other repeated code. If a pattern appears more than once, it should be unified — provided the extraction makes semantic sense and doesn't introduce unnecessary indirection.

- **SOLID — Single Responsibility enforced:** Every production file (frontend CSS/TS, backend C#) must stay ≤ 250 lines. Large classes are split into focused helpers. This applies to both frontend and backend code.

## Cleanup Hygiene

Before marking any feature/refactor PR as complete, verify these common dead-code sources are not reintroduced:

### Frontend
- **Dead CSS**: After removing a component/feature, grep all 13 component files under `styles/` + `animations.css` for any class names or `@keyframes` that are no longer referenced in any `.tsx`/`.ts` file. Remove the orphaned rules.
- **Unused translation keys**: After removing a UI element, check if its `t('key')` call is gone. If so, delete the key from both `en.ts` and `es.ts`. Keep `nav.dot*` keys (used by scroll-progress in `App.tsx`).
- **Orphaned utility files**: When consolidating or refactoring (e.g. merging 3 company lookup files into 1), delete the old files and update all imports. Check AGENTS.md + README.md for stale file references.
- **One-off scripts**: Image conversion, data migration, or generation scripts (`scripts/`) that were run once should be deleted after use. Remove their devDependencies too (e.g. `sharp`).
- **Empty directories**: Delete empty dirs like `src/core/` left behind after refactors.

### Backend
- **Orphaned exception/class**: If a domain exception or class is no longer thrown, caught, or imported by any code, delete the file.
- **Dead variables**: After extracting config setup, check for orphaned `var` assignments where the value is never read.
- **Unused `using` directives**: After signature changes, remove any `using` that is no longer required for compilation.
- **`GenerateDocumentationFile`**: Turn off if no XML doc comments exist and no consumer reads the generated `.xml` file. Remove `1591` from `NoWarn` alongside it.

### Both
- **Temp files**: Word lock files (`~$*`) and OS artifacts should be in `.gitignore`, never committed.
- **Translation fallback strings**: If offline/fallback strings are hardcoded in the repository code, the corresponding translation keys should be deleted. Don't maintain both.
- **README + AGENTS.md drift**: Every file rename, deletion, or structural change must update the architecture tree and file references in both docs.

- **Clean Architecture discipline:** The Domain layer must have ZERO awareness of external concerns — no file paths, no env vars, no DB contexts, no HTTP. Infrastructure handles all external data access. Application orchestrates use cases. API presents results. Before placing any code or file, ask: *which layer does this belong to?*
- Always run commands from the specific subdirectory (`frontend/` or `backend/`).
- Backend uses centralized package management (`Directory.Packages.props`). All packages must be stable (no preview versions).
- `NoWarn` suppresses CS1591 (missing XML docs) and CA1707 (test naming underscores).
- Frontend `index.html` lives in `src/`, so `vite.config.ts` needs `root: 'src'`, `envDir: '..'`, and HTML paths are relative to `src/` (e.g. `./main.tsx`, not `./src/main.tsx`).
- Backend `global.json` requires `rollForward: latestMajor` (not `latestPatch`) to work on SDK versions newer than 10.0.203.
- Backend tests use **Moq** (not NSubstitute). Test projects: Domain.Tests (5), Application.Tests (73), Integration.Tests (21).
- Integration.Tests uses `WebApplicationFactory<Program>` with a temp .docx fixture. Always reset static state (e.g. `DiscordErrorNotifier.ResetCooldown()`) in test setup. `Program.cs` declares `public partial class Program { }` at file end so `WebApplicationFactory<Program>` can reference it — no `InternalsVisibleTo` needed.
- Namespace root is `Backend.*` — no `MyCV` prefix anywhere in the backend.
- **README must be kept in sync** — every change that adds, removes, or modifies a feature, directory, dependency, configuration key, or command must also update `README.md` (stack table, architecture tree, config table, quick start, how it works). This is enforced in code review.
- **DOCX file is immutable** — the `cv.docx` file must never be modified. All derived data (company URLs, logos, etc.) must be provided via hardcoded frontend maps (`CompanyData.ts` with `CompanyMatch.ts` factories). The DOCX parser must remain backward-compatible with the old 2-line format (`Company | Location` → `Role | Period`). Company logo/URL/image matching uses accent-normalized prefix-based fuzzy matching.
- Frontend Vitest setup: `tests/test-setup.ts` imports `@testing-library/jest-dom/vitest`. Vitest config in `vite.config.ts` `test` block with `happy-dom` environment, `VITE_API_URL` env var, and `globals: true`.
- **Feedback flow**: FAB (fixed bottom-right) → modal with name, stars, optional comment → toast notification (bottom-right, 5s auto-dismiss, gold left accent). Country auto-detected via `Intl.DisplayNames`, sent in POST body but not shown. Comment is optional, only included in Discord embed when non-empty.
- **Two Discord webhooks**: `DiscordErrorNotifier` (red embed, 1h cooldown via static `_lastAlertTime`, uses `ErrorWebhookUrl`). `DiscordFeedbackNotifier` (green embed with fields array, no cooldown, uses `FeedbackWebhookUrl`). Both have empty-URL and invalid-URL guards, 5s timeout. CD passes both via `Discord__ErrorWebhookUrl` and `Discord__FeedbackWebhookUrl` env vars.

## Security & Quality Baselines

Every file change must uphold these invariants:

### Supply Chain
- All GitHub Actions must be pinned to **commit SHA digests**, not major version tags. Add a `# vX.Y.Z` comment after the SHA.
- `npm audit` must report **0 vulnerabilities** before any merge.
- All packages (npm + NuGet) must be **stable releases** — no `-beta`, `-alpha`, `-rc`, `-preview`, `-next`, `-dev`, or `-canary` prerelease suffixes. Every direct dependency version must be a stable SemVer release with no tag suffix. Transitive dependencies with prerelease tags should be flagged and evaluated for replacement if viable.
- All NuGet packages must use central version management (`Directory.Packages.props`), with stable versions only (no preview versions).

### Docker
- `Dockerfile` must set `ASPNETCORE_URLS` explicitly to match `EXPOSE` (backend only).
- `Dockerfile` must have a `HEALTHCHECK` instruction.
- `Dockerfile` should use targeted `COPY` paths (not `COPY . .`).
- Frontend Docker image: `ghcr.io/s3rbr0p4r/mycv/mycv-frontend` (nginx:alpine, serves built `dist/` on configurable port via `${PORT}` env var).
- Backend Docker image: `ghcr.io/s3rbr0p4r/mycv/mycv-api` (aspnet:10.0).
- Run as non-root via `USER $APP_UID`.
- **Docker networking**: Both containers run on a shared `mycv-net` bridge network. Backend has `--network-alias mycv-api`. Frontend nginx proxies `/api/` requests to `http://mycv-api:${BACKEND_PORT}/api/`. CD workflow creates the network with `docker network create mycv-net 2>/dev/null || true` before starting containers.
- **nginx reverse proxy** (`frontend/nginx.conf`): Location block `/api/` proxies to backend, sets `Host`, `X-Real-IP`, `X-Forwarded-For`, `X-Forwarded-Proto`, and `Origin` (from `FRONTEND_URL` env var) so `OriginValidationMiddleware` allows proxied requests.
- `FRONTEND_URL` env var must be passed to the frontend container (used by nginx template via `envsubst`). Set in CD via `-e FRONTEND_URL=${{ vars.FRONTEND_URL }}`.

### Frontend Build
- `vite.config.ts` must have `build.emptyOutDir: true` to prevent stale asset accumulation.
- `.env.production` must exist and provide production-safe CSP values (`'unsafe-inline'` only in dev).
- `dist/` must not contain orphaned or stale assets from prior builds.

### CSP
- Production CSP must NOT include `'unsafe-inline'` for `script-src` (Vite extracts all JS to separate files).
- Frontend CSP: both `<meta>` tag (in `index.html`) and backend `Content-Security-Policy` header must be kept in sync.
- Backend responses should include `Content-Security-Policy` header as defense-in-depth.
- CSP env vars in `.env` files must be **double-quoted** to preserve single quotes for CSP keywords (e.g. `VITE_CSP_SCRIPT_SRC="'self'"`). Node.js 24's `parseEnv` strips bare single quotes. GitHub variables bypass this and use bare CSP syntax (e.g. `'self'`).

### Testing Coverage
- Every **class** in the backend must have corresponding test coverage (unit or integration) — mandatory, no exceptions.
- Every **React component** should have at least a smoke test (renders without error).
- Every **custom hook** should have behavior tests via `renderHook`.
- Integration tests must reset static state (e.g. `DiscordErrorNotifier.ResetCooldown()`).
- **Removing production code must also remove its tests** — when a feature, header, endpoint, or class is deleted, all corresponding test assertions and test files must be deleted alongside it. Orphaned tests that assert deleted functionality will fail.

### Accessibility
- Interactive elements must have `aria-label` or visible text labels.
- Dropdown menus need `aria-expanded`, `aria-haspopup`, `role="menu"`, and `role="menuitem"`.
- Skip-to-content link recommended at the top of `App.tsx`.

### Clean Code (enforced)
- Methods do one thing (≤ ~30 lines per private method).
- Single exit per method — guard clauses first, main path linear.
- Complex boolean conditions extracted into named private methods.
- No inline documentation comments (`//`, `///`) on implementation logic.
- Parsing classes describe the domain structure, not the file format (name by intent).
- **Middleware and inline logic extracted into dedicated classes** — inline `app.Use(...)` lambdas in `Program.cs` that contain standalone logic (security headers, health checks, etc.) must be extracted into `IMiddleware` implementations registered via DI, following the same pattern as `GlobalExceptionHandler` and `SecurityHeadersMiddleware`. This keeps `Program.cs` focused on the pipeline composition, not the implementation details.
