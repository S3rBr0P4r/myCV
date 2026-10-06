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
└── .github/workflows/     CI (build+test+Docker) + CD (fetch CV from NAS + SSH deploy)
```

## How it works

1. **`.docx` → JSON**: The CV file never enters the repo — CD fetches it from a password-protected QNAP File Station share link, validates it, and installs it at `/opt/mycv/data/cv.docx`. At runtime, `WordCvSource` parses the file into structured data.
2. **`GET /api/v1/cv`**: Returns the CV as JSON, optionally translated via DeepL based on `Accept-Language`. Experience narratives and the certifications & relevant training section are translated. Narratives are always rendered in first-person singular (the English source is prefixed with `I ` before sending to DeepL, since English past tense doesn't mark person and DeepL would otherwise flip between *desarrolló* and *desarrollé*). Marker labels (`Tech Stack`, `Core`, `Tooling`) are translated too — `Core` is canonicalized to `Núcleo` — but the payloads after them stay in English and the `**bold**`/`:` decoration is preserved byte-for-byte, so list indentation renders identically in both languages. If DeepL detects a text as already being in the target language and returns it unchanged (e.g. a mixed English/Spanish course title), it is retried once with a forced English source so the English parts still get translated.
3. **`POST /api/v1/feedback`**: Stores viewer feedback (name, rating, country, comment) and forwards it to a Discord webhook as a green embed.
4. **Frontend**: Fetches CV on load, re-fetches on locale switch. Resets to a friendly offline page when the backend is unreachable. Feedback FAB opens a modal with star rating, name, and optional comment; submission shows a toast notification.
5. **Alerts**: Errors (DOCX parse failure, DeepL failures, path traversal) send a red embed to a separate Discord webhook with a 1-hour cooldown.

## Key features

- Bilingual (EN/ES, no auto-detect), dark mode, reduced-motion support
- API versioning, Swagger, health endpoint, Serilog logging
- Viewer feedback collection (star rating, auto-detected country, optional comment)
- Two Discord webhooks: error alerts (1h cooldown) + feedback (no cooldown)
- Offline fallback with localized Ghibli-inspired error page
- Dockerized (non-root, GHA cache), CI/CD via GitHub Actions (CV fetched from NAS share link at deploy)
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
| `CV_SHARE_URL` | File Station shared-folder link containing `cv.docx` (CD secret) |
| `CV_SHARE_PASSWORD` | Share link access code (CD secret) |

### Updating the CV

Replace `cv.docx` in the NAS shared folder — the share link (`ssid`) stays valid — then trigger CD (push to `main` or manual workflow dispatch). The fetch step validates the download (≤5 MB, OOXML magic bytes, `word/document.xml`) and atomically replaces `/opt/mycv/data/cv.docx` before recreating the backend container; any failure aborts the deploy so the old container keeps serving the previous CV.

## License

MIT — see [LICENSE](./LICENSE).
