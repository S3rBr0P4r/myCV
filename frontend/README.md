# MyCV Frontend

Vanilla TypeScript + Vite project following Clean Architecture (domain → application → infrastructure → ui).

## Development

```bash
npm run dev      # Vite dev server (port 5173)
npm run build    # tsc && vite build
npm run preview  # Vite preview
```

## Architecture

```
src/
├── domain/          # Entities, repository interfaces
├── application/     # Use cases
├── infrastructure/  # API repository implementations
├── ui/              # UI components (renderers)
├── core/            # Animations, utilities
├── styles/          # CSS design system & animations
├── main.ts          # Entry point, DI wiring
└── index.html       # Shell
```

No framework — manual DI in `main.ts`. API hardcoded to `http://localhost:60355/api/cv`.
