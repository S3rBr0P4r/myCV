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
| `npm run dev` | Vite dev server (port 5173) + backend |
| `npm run build` | `tsc && vite build` |
| `npm run preview` | Vite preview |
| `npm run test` | `vitest run` |
| `npm run test:watch` | `vitest` (watch mode) |
| `npm run test:coverage` | `vitest run --coverage` |

**Vulnerability fix:** Vite pinned to `^6.0.0` (upgraded from `^5.0.0`) to resolve esbuild moderate-severity advisory (GHSA-67mh-4wv8-2f99). Run `npm audit fix` to stay current.

No linter/formatter beyond `.editorconfig` (2-space indent, single quotes for TS).

**Architecture:** Clean Architecture layers — `domain/` → `application/` → `infrastructure/` → `ui/` (React components + hooks + contexts). Manual DI via hooks + React contexts in `main.tsx`.

**React conventions:**
- Functional components only (no classes). File extension `.tsx` for components, `.ts` for pure functions.
- Custom hooks in `src/ui/hooks/` with `use*` prefix (e.g. `useTheme`, `useTranslation`).
- Contexts in `src/ui/contexts/` (Theme, Translation, CV).
- No CSS-in-JS — all styles live in `design-system.css` + `animations.css`.
- No state management library (Redux/Zustand) — local state + contexts only.
- Domain/application/infrastructure layers remain framework-agnostic `.ts` files.

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

**Root-level dev command:** `cd frontend && npm run dev` starts both services (via `concurrently`). Pre-kills port 60355.

## Backend (`backend/`)

.NET 10 (SDK 10.0.203, prerelease allowed). Solution file: `Backend.slnx` (.slnx format). Three projects: `src/Backend.csproj` + two test projects.

| Command | Action |
|---------|--------|
| `dotnet restore Backend.slnx` | Restore packages |
| `dotnet build Backend.slnx --no-restore` | Build all projects |
| `dotnet test Backend.slnx --no-build` | Run all tests (unit + integration) |

**Build hardens:** `TreatWarningsAsErrors`, `AnalysisLevel: latest-recommended`, `EnforceCodeStyleInBuild` — fix all analyzer warnings to build. Every new public class must have corresponding test coverage.

**Folder structure (one project, layers as folders):**

```
src/Backend.csproj
  Domain/         Entities, Exceptions, Interfaces
    → Application/  DTOs, Mappings, UseCases
      → Infrastructure/  Persistence, Sources  
        → Api/   Controllers, Middleware
tests/
  Domain.Tests/           Entity + exception unit tests
  Application.Tests/      Use case + service + middleware unit tests
  Integration.Tests/      Full HTTP pipeline via WebApplicationFactory
```

- Each layer registers its own DI via a static `DependencyInjection` class with extension method (`AddApplication()`, `AddInfrastructure()`), plus `AddApiServices()` in the API layer.
- DTOs are `record` types; mapping is hand-written extensions (no AutoMapper).
- Use cases follow CQRS-lite: `GetCVQuery` / `GetCVHandler` / `GetCVResult`.
- `GlobalExceptionHandler` middleware maps `DomainException` → 400, `NotFoundException` → 404.
- Namespace root: `Backend.*` (not `MyCV.*`).
- API uses `[ApiController]` + `[Route("api/v{version:apiVersion}/[controller]")]` with URL segment versioning. Current version: `v1`. Add `[ApiVersion("X.Y")]` to controllers. Swagger via `Swashbuckle.AspNetCore` (no `Microsoft.AspNetCore.OpenApi`).

**Backend launch URL:** `http://localhost:60355` (from `Properties/launchSettings.json`). Only HTTP in development — firewall-friendly and matches Vite proxy scheme.

**CORS:** Allows `http://localhost:5173`, `http://127.0.0.1:5173`, `https://localhost:5173` with `AllowCredentials()`.

**Editorconfig conventions (C#):** 4-space indent, CRLF, file-scoped namespaces, `_camelCase` private fields, `I` prefix for interfaces, `Async` suffix for async methods.

## CI (`.github/workflows/CI.yml`)

Triggers on push to `main`. Two independent jobs:
- **FrontEnd:** `npm ci` → `npm run build` → `npm run test`
- **Backend:** `dotnet restore` → `dotnet build` → `dotnet test`

**Security:** Deploy step removed — CI is build+test only. Actions pinned to major versions.

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

## Gotchas

- **Clean Architecture discipline:** The Domain layer must have ZERO awareness of external concerns — no file paths, no env vars, no DB contexts, no HTTP. Infrastructure handles all external data access. Application orchestrates use cases. API presents results. Before placing any code or file, ask: *which layer does this belong to?*
- Always run commands from the specific subdirectory (`frontend/` or `backend/`).
- Backend uses centralized package management (`Directory.Packages.props`). All packages must be stable (no preview versions).
- `NoWarn` suppresses CS1591 (missing XML docs) and CA1707 (test naming underscores).
- Frontend `index.html` lives in `src/`, so `vite.config.ts` needs `root: 'src'`, `envDir: '..'`, and HTML paths are relative to `src/` (e.g. `./main.tsx`, not `./src/main.tsx`).
- Backend `global.json` requires `rollForward: latestMajor` (not `latestPatch`) to work on SDK versions newer than 10.0.203.
- Backend tests use **Moq** (not NSubstitute). Test projects: Domain.Tests (5), Application.Tests (34), Integration.Tests (14).
- Integration.Tests uses `WebApplicationFactory<Program>` with a temp .docx fixture. Always reset static state (e.g. `DiscordNotifier.ResetCooldown()`) in test setup. `Program.cs` declares `public partial class Program { }` at file end so `WebApplicationFactory<Program>` can reference it — no `InternalsVisibleTo` needed.
- Namespace root is `Backend.*` — no `MyCV` prefix anywhere in the backend.
- **README must be kept in sync** — every change that adds, removes, or modifies a feature, directory, dependency, configuration key, or command must also update `README.md` (stack table, architecture tree, config table, quick start, how it works). This is enforced in code review.
- **DOCX file is immutable** — the `cv.docx` file must never be modified. All derived data (company URLs, logos, etc.) must be provided via hardcoded frontend maps (`CompanyUrl.ts`, `CompanyImage.ts`, `CompanyLogoFile.ts`). The DOCX parser must remain backward-compatible with the old 2-line format (`Company | Location` → `Role | Period`). Company logo/URL/image matching uses accent-normalized prefix-based fuzzy matching.
- Frontend Vitest setup: `tests/test-setup.ts` imports `@testing-library/jest-dom/vitest`. Vitest config in `vite.config.ts` `test` block with `happy-dom` environment, `VITE_API_URL` env var, and `globals: true`.
