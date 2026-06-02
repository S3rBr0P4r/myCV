# Design System — Studio Ghibli · Premium

## Direction

**Forest warmth with hand-crafted soul.** A personal CV inspired by Ghibli's lush woodlands — refined typography, mossy greens, gentle motion, and atmospheric depth — without sacrificing the polish expected of a production-grade web interface.

The design avoids both cold corporate minimalism and generic AI aesthetics. Every choice is intentional: warm over neutral, soft over sharp, organic over rigid.

| Principle | Application |
|-----------|-------------|
| Craft over template | Custom SVG icons, hand-tuned spacing, bespoke color palette |
| Atmosphere | Warm vignette, subtle grain overlay, floating background gradients |
| Gentleness | Soft easing curves, reduced opacity for secondary elements, generous whitespace |
| Premium detail | Custom scrollbar, warm-tinted shadows, polished button micro-interactions |

## Color Palette

### Light Theme

```
Warm parchment bg    #F4F0E9
Card surface         #FCFAF5
Dark forest text     #1E2A1D
Moss secondary       #5A6B55
Sage tertiary        #8FA089
Pale sage border     #DCE4D8

Moss primary         #4A7C59
Gold accent          #D4A54B
Sage secondary       #7A9B6E
Ghibli sky           #7EC8E3
```

### Dark Theme

```
Dark forest bg       #161A15
Card surface         #1E221D
Pale sage text       #E8EDE4
Beige secondary      #A0A898
Muted tertiary       #596151

Moss primary         #5B8C5A (brighter for legibility)
Gold accent          #E8C46A
Ghibli sky           #5BA8C3
```

Shadows are tinted forest green (dark green-based) rather than neutral gray to maintain the natural Ghibli atmosphere even in depths. The amber/gold glow accent provides warm contrast against the cool green base — evoking sunlight filtering through forest canopy.

## Typography

| Role | Font | Weight | Notes |
|------|------|--------|-------|
| Headings | Newsreader | 600–700 | Serif with warmth; optical size axis for readability at any scale |
| Body | Figtree | 400–600 | A sans-serif that avoids the coldness of Inter; rounded terminals, friendly |
| Technical | JetBrains Mono | 400–500 | Clean monospace for dates, tags, labels |

Headings use serif for authority and warmth. Body uses a refined sans for readability. Mono is used sparingly for accent elements (tags, dates, logo) to create texture.

## Spacing & Layout

- **Rhythm**: 8px base unit (4px spacing grain for micro, 8px for components, 24px for sections)
- **Max content width**: 1100px for section content; hero content at 720px for intimate reading
- **Vertical rhythm**: 100px section padding (60px on mobile) with 48px after section titles
- **Asymmetry**: Used sparingly — timeline dots offset, background float offset from center

## Motion & Animation

| Element | Duration | Easing |
|---------|----------|--------|
| Page load staggered reveals | 0.8s | `cubic-bezier(0.22, 1, 0.36, 1)` |
| Scroll reveal sections | 0.9s | Same as above |
| Staggered children | 0.7s | Same as above, 0.05–0.56s delay |
| Hover transitions | 0.35s | Same as above |
| Background float | 25s loop | `ease-in-out infinite alternate` |
| Button glow pulse | 4s loop | `ease-in-out infinite` |

The cubic bezier `(0.22, 1, 0.36, 1)` is a custom "gentle ease-out" — faster to start, slower to settle. It avoids the jarring snap of standard easings.

All animations respect `prefers-reduced-motion` (disabled completely).

## Effects & Atmosphere

- **Noise grain overlay**: Fixed SVG fractal noise at 2.5% opacity across the entire page for a subtle paper-like texture
- **Warm vignette**: Radial gradient from center to edges, tinted warm brown
- **Background float**: Three overlapping radial gradients (moss green, sky, gold) that drift slowly across the viewport
- **Glow on primary CTA**: Subtle amber pulse on the primary button to draw attention
- **Backdrop blur**: Navbar uses `blur(24px) saturate(1.4)` for a frosted glass effect; experience card contents use `blur(12px)` for depth

## Iconography

- No emojis used as icons — sun and moon are inline SVG paths from a custom, lightweight set
- Flags are inline SVG with standard aspect ratios (60:40) and rounded corners via CSS
- Consistent 1.8px stroke width on all UI icons (sun, moon)
- Flag SVGs are minimal and use only the essential geometric paths

## Accessibility

- Color contrast ratios meet WCAG AA (≥4.5:1 for body text in both themes)
- Focus-visible outlines use primary color at 2px with 2px offset
- All interactive elements have `aria-label` attributes
- Touch targets are minimum 40px with adequate spacing
- Reduced motion supported — all animations disabled at user preference
- Semantic heading hierarchy (h1 → h2 → h3, no skips)

## Crafted with

- **Skill**: [anthropics/frontend-design](https://github.com/anthropics/skills/tree/main/skills/frontend-design) from skills.sh
- **LLM**: opencode/deepseek-v4-flash-free
- **Human supervision**: Every line reviewed and directed — no "vibe coding"
