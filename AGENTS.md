# myCV — AGENTS.md

## Structure

Two independent projects at the root:

```
frontend/    TypeScript + Vite
backend/     .NET 10 Clean Architecture
```

## Frontend (`frontend/`)

| Command | Action |
|---------|--------|
| `npm run dev` | Vite dev server (port 5173) |
| `npm run build` | `tsc && vite build` |
| `npm run preview` | Vite preview |

**Vulnerability fix:** Vite pinned to `^6.0.0` (upgraded from `^5.0.0`) to resolve esbuild moderate-severity advisory (GHSA-67mh-4wv8-2f99). Run `npm audit fix` to stay current.

No linter/formatter beyond `.editorconfig` (2-space indent, single quotes for TS).

**Architecture:** Clean Architecture layers — `domain/` → `application/` → `infrastructure/` → `ui/` (components + renderer). Manual DI in `main.ts`.

**API:** Configured via `VITE_API_URL` env var (dev = `/api/v1/cv` via Vite proxy, production = set via CI variable `VITE_API_URL`). Has offline fallback if backend is unreachable.

**Root-level dev command:** `cd frontend && npm run dev` starts both services (via `concurrently`). Pre-kills port 60355.

## Backend (`backend/`)

.NET 10 (SDK 10.0.203, prerelease allowed). Solution file: `Backend.slnx` (.slnx format). Single project at `src/Backend.csproj` with Clean Architecture folder layout.

| Command | Action |
|---------|--------|
| `dotnet restore Backend.slnx` | Restore packages |
| `dotnet build Backend.slnx --no-restore` | Build all projects |
| `dotnet test Backend.slnx --no-build` | Run tests (xUnit + FluentAssertions + Moq) |

**Build hardens:** `TreatWarningsAsErrors`, `AnalysisLevel: latest-recommended`, `EnforceCodeStyleInBuild` — fix all analyzer warnings to build. Every new public class must have corresponding test coverage.

**Folder structure (one project, layers as folders):**

```
src/Backend.csproj
  Domain/         Entities, Exceptions, Interfaces
    → Application/  DTOs, Mappings, UseCases
      → Infrastructure/  Persistence, Sources  
        → Api/   Controllers, Middleware
```

- Each layer registers its own DI via a static `DependencyInjection` class with extension method (`AddApplication()`, `AddInfrastructure()`).
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
- **FrontEnd:** `npm ci` → `npm run build`
- **Backend:** `dotnet restore` → `dotnet build` → `dotnet test`

**Security:** Deploy step removed — CI is build+test only. Actions pinned to major versions.

## Skills (`.opencode/skills/`)

| Skill | Purpose |
|-------|---------|
| `vite` | Vite config, build, plugins |
| `frontend-design` | Production-grade frontend design, aesthetic direction, design systems |
| `typescript-code-review` | TypeScript code review best practices |
| `dotnet-clean-architecture` | Clean Architecture project structure |
| `dotnet-domain-entity-generator` | DDD entity patterns |
| `dotnet-repository-pattern` | Repository pattern with EF Core |
| `moq-testing` | Unit testing with Moq + xUnit + FluentAssertions |

## Gotchas

- **Clean Architecture discipline:** The Domain layer must have ZERO awareness of external concerns — no file paths, no env vars, no DB contexts, no HTTP. Infrastructure handles all external data access. Application orchestrates use cases. API presents results. Before placing any code or file, ask: *which layer does this belong to?*
- Always run commands from the specific subdirectory (`frontend/` or `backend/`).
- Backend uses centralized package management (`Directory.Packages.props`). All packages must be stable (no preview versions).
- `NoWarn` suppresses CS1591 (missing XML docs) and CA1707 (test naming underscores).
- Frontend `index.html` lives in `src/`, so `vite.config.ts` needs `root: 'src'`, `envDir: '..'`, and HTML paths are relative to `src/` (e.g. `./main.ts`, not `./src/main.ts`).
- Backend `global.json` requires `rollForward: latestMajor` (not `latestPatch`) to work on SDK versions newer than 10.0.203.
- Backend tests use **Moq** (not NSubstitute). Test projects: Domain.Tests (4), Application.Tests (32).
- Namespace root is `Backend.*` — no `MyCV` prefix anywhere in the backend.
- **README must be kept in sync** — every change that adds, removes, or modifies a feature, directory, dependency, configuration key, or command must also update `README.md` (stack table, architecture tree, config table, quick start, how it works). This is enforced in code review.
- **DOCX file is immutable** — the `cv.docx` file must never be modified. All derived data (company URLs, logos, etc.) must be provided via hardcoded frontend maps (`CompanyUrl.ts`, `CompanyImage.ts`, `CompanyLogoFile.ts`). The DOCX parser must remain backward-compatible with the old 2-line format (`Company | Location` → `Role | Period`). Company URLs are mapped in `CompanyUrl.ts`, logos via local PNG files in `frontend/src/public/logos/` (downloaded from `icon.horse`, initials SVG fallback if missing), background images in `CompanyImage.ts`.
