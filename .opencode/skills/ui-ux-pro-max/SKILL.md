---
name: ui-ux-pro-max
description: "UI/UX design intelligence for web and mobile. Includes 50+ styles, 161 color palettes, 57 font pairings, 161 product types with reasoning rules, 99 UX guidelines, and 25 chart types across 10 stacks."
---

# UI/UX Pro Max - Design Intelligence

Comprehensive design guide for web and mobile applications. Contains 50+ styles, 161 color palettes, 57 font pairings, 161 product types with reasoning rules, 99 UX guidelines, and 25 chart types across 10 technology stacks.

## When to Apply

This Skill should be used when the task involves **UI structure, visual design decisions, interaction patterns, or user experience quality control**.

### Must Use
- Designing new pages (Landing Page, Dashboard, Admin, SaaS, Mobile App)
- Creating or refactoring UI components (buttons, modals, forms, tables, charts, etc.)
- Choosing color schemes, typography systems, spacing standards, or layout systems
- Reviewing UI code for user experience, accessibility, or visual consistency
- Making product-level design decisions (style, information hierarchy, brand expression)

### Recommended
- UI looks "not professional enough" but the reason is unclear
- Pre-launch UI quality optimization
- Building design systems or reusable component libraries

### Skip
- Pure backend logic development
- Only involving API or database design
- Non-visual scripts or automation tasks

## Rule Categories by Priority

| Priority | Category | Impact | Key Checks |
|----------|----------|--------|------------|
| 1 | Accessibility | CRITICAL | Contrast 4.5:1, Alt text, Keyboard nav, Aria-labels |
| 2 | Touch & Interaction | CRITICAL | Min size 44×44px, 8px+ spacing, Loading feedback |
| 3 | Performance | HIGH | WebP/AVIF, Lazy loading, Reserve space (CLS < 0.1) |
| 4 | Style Selection | HIGH | Match product type, Consistency, SVG icons (no emoji) |
| 5 | Layout & Responsive | HIGH | Mobile-first breakpoints, Viewport meta, No horizontal scroll |
| 6 | Typography & Color | MEDIUM | Base 16px, Line-height 1.5, Semantic color tokens |
| 7 | Animation | MEDIUM | Duration 150-300ms, Motion conveys meaning |
| 8 | Forms & Feedback | MEDIUM | Visible labels, Error near field, Helper text |
| 9 | Navigation Patterns | HIGH | Predictable back, Bottom nav ≤5, Deep linking |
| 10 | Charts & Data | LOW | Legends, Tooltips, Accessible colors |

## Quick Reference

### 1. Accessibility (CRITICAL)
- `color-contrast` - Minimum 4.5:1 ratio for normal text (3:1 large text)
- `focus-states` - Visible focus rings on interactive elements (2-4px)
- `alt-text` - Descriptive alt text for meaningful images
- `aria-labels` - aria-label for icon-only buttons
- `keyboard-nav` - Tab order matches visual order
- `form-labels` - Use label with for attribute
- `skip-links` - Skip to main content for keyboard users
- `heading-hierarchy` - Sequential h1→h6, no level skip
- `color-not-only` - Don't convey info by color alone (add icon/text)
- `dynamic-type` - Support system text scaling
- `reduced-motion` - Respect prefers-reduced-motion
- `voiceover-sr` - Meaningful accessibilityLabel

### 2. Touch & Interaction (CRITICAL)
- `touch-target-size` - Min 44×44pt (Apple) / 48×48dp (Material)
- `touch-spacing` - Minimum 8px/8dp gap between touch targets
- `hover-vs-tap` - Use click/tap for primary interactions
- `loading-buttons` - Disable button during async operations
- `error-feedback` - Clear error messages near problem
- `cursor-pointer` - Add cursor-pointer to clickable elements
- `tap-delay` - Use touch-action: manipulation
- `press-feedback` - Visual feedback on press (ripple/highlight)
- `haptic-feedback` - Use haptic for confirmations
- `swipe-clarity` - Swipe actions must show clear affordance

### 3. Performance (HIGH)
- `image-optimization` - Use WebP/AVIF, responsive images
- `image-dimension` - Declare width/height to prevent layout shift
- `font-loading` - Use font-display: swap/optional
- `lazy-loading` - Lazy load non-hero components via dynamic import
- `bundle-splitting` - Split code by route/feature
- `third-party-scripts` - Load async/defer
- `virtualize-lists` - Virtualize lists with 50+ items
- `progressive-loading` - Use skeleton screens
- `debounce-throttle` - For high-frequency events

### 4. Style Selection (HIGH)
- `style-match` - Match style to product type
- `consistency` - Use same style across all pages
- `no-emoji-icons` - Use SVG icons (Heroicons, Lucide), not emojis
- `color-palette-from-product` - Choose palette from product/industry
- `effects-match-style` - Shadows, blur, radius aligned with style
- `dark-mode-pairing` - Design light/dark variants together
- `icon-style-consistent` - Use one icon set across the product
- `primary-action` - Each screen should have only one primary CTA

### 5. Layout & Responsive (HIGH)
- `viewport-meta` - width=device-width initial-scale=1 (never disable zoom)
- `mobile-first` - Design mobile-first, then scale up
- `breakpoint-consistency` - Use systematic breakpoints (375/768/1024/1440)
- `readable-font-size` - Minimum 16px body text on mobile
- `line-length-control` - Mobile 35-60 chars; desktop 60-75 chars
- `horizontal-scroll` - No horizontal scroll on mobile
- `spacing-scale` - Use 4pt/8dp incremental spacing system
- `touch-density` - Keep component spacing comfortable for touch
- `container-width` - Consistent max-width on desktop
- `z-index-management` - Define layered z-index scale
- `visual-hierarchy` - Establish hierarchy via size, spacing, contrast

### 6. Typography & Color (MEDIUM)
- `line-height` - Use 1.5-1.75 for body text
- `line-length` - Limit to 65-75 characters per line
- `font-pairing` - Match heading/body font personalities
- `font-scale` - Consistent type scale (12/14/16/18/24/32)
- `contrast-readability` - Darker text on light backgrounds
- `color-semantic` - Define semantic color tokens
- `color-dark-mode` - Test contrast separately for dark mode
- `color-not-decorative-only` - Functional color must include icon/text

### 7. Animation (MEDIUM)
- `duration-timing` - 150-300ms for micro-interactions
- `transform-performance` - Use transform/opacity only
- `loading-states` - Show skeleton if loading >300ms
- `excessive-motion` - Animate 1-2 key elements per view max
- `easing` - Use ease-out for entering, ease-in for exiting
- `motion-meaning` - Every animation must express a cause-effect relationship
- `continuity` - Maintain spatial continuity (shared element, directional slide)
- `spring-physics` - Prefer spring curves over linear/cubic-bezier
- `interruptible` - Animations must be interruptible
- `modal-motion` - Modals animate from trigger source (scale+fade)

### 8. Forms & Feedback (MEDIUM)
- `input-labels` - Visible label per input (not placeholder-only)
- `error-placement` - Show error below the related field
- `submit-feedback` - Loading then success/error state
- `required-indicators` - Mark required fields
- `empty-states` - Helpful message when no content
- `toast-dismiss` - Auto-dismiss toasts in 3-5s
- `confirmation-dialogs` - Confirm before destructive actions
- `inline-validation` - Validate on blur, not keystroke
- `input-type-keyboard` - Use semantic input types for correct keyboard
- `form-autosave` - Long forms should auto-save drafts
- `error-recovery` - Error messages must include a clear recovery path

### 9. Navigation Patterns (HIGH)
- `bottom-nav-limit` - Bottom navigation max 5 items
- `back-behavior` - Back navigation must be predictable
- `deep-linking` - All key screens must be reachable via deep link
- `nav-label-icon` - Navigation items must have both icon and text label
- `nav-hierarchy` - Primary vs secondary nav must be clearly separated
- `modal-escape` - Modals must offer clear close affordance
- `state-preservation` - Back must restore scroll position and state
- `tab-badge` - Use badges on nav items sparingly
- `avoid-mixed-patterns` - Don't mix Tab + Sidebar + Bottom Nav at same level

### 10. Charts & Data (LOW)
- `chart-type` - Match chart type to data type
- `color-guidance` - Use accessible color palettes
- `data-table` - Provide table alternative for accessibility
- `legend-visible` - Always show legend
- `tooltip-on-interact` - Tooltips on hover/tap showing exact values
- `axis-labels` - Label axes with units
- `responsive-chart` - Charts must reflow on small screens
- `empty-data-state` - Show meaningful empty state
- `loading-chart` - Use skeleton placeholder while loading
- `no-pie-overuse` - Avoid pie/donut for >5 categories

## How to Use This Skill

Use this skill when the user requests any of the following:

| Scenario | Start From |
|----------|------------|
| **New project / page** | Step 1 → Step 2 (design system) |
| **New component** | Step 3 (domain search: style, ux) |
| **Choose style / color / font** | Step 2 (design system) |
| **Review existing UI** | Quick Reference checklist |
| **Fix a UI bug** | Quick Reference → relevant section |
| **Improve / optimize** | Domain search (ux, react) |
| **Add charts / data viz** | Domain search (chart) |

### Step 1: Analyze User Requirements

Extract key information:
- **Product type**: Tool, Productivity, Entertainment, or hybrid
- **Target audience**: C-end consumer users
- **Style keywords**: minimal, vibrant, dark mode, content-first, etc.
- **Stack**: React, Next.js, Vue, Svelte, React Native, Flutter, HTML+Tailwind

### Step 2: Generate Design System

Use Python search tool to get comprehensive recommendations:

```bash
python3 skills/ui-ux-pro-max/scripts/search.py "<product_type> <industry> <keywords>" --design-system [-p "Project Name"]
```

This searches domains in parallel and returns: pattern, style, colors, typography, effects, anti-patterns.

### Step 3: Supplement with Detailed Searches

```bash
python3 skills/ui-ux-pro-max/scripts/search.py "<keyword>" --domain <domain> [-n <max_results>]
```

| Domain | Use For |
|--------|---------|
| `product` | Product type recommendations |
| `style` | UI styles, colors, effects |
| `typography` | Font pairings |
| `color` | Color palettes by product type |
| `landing` | Page structure, CTA strategies |
| `chart` | Chart types, library recommendations |
| `ux` | Best practices, anti-patterns |
| `google-fonts` | Individual Google Fonts lookup |
| `react` | React/Next.js performance |
| `web` | App interface guidelines |
| `prompt` | AI prompts, CSS keywords |

### Step 4: Stack Guidelines

```bash
python3 skills/ui-ux-pro-max/scripts/search.py "<keyword>" --stack react-native
```

## Pre-Delivery Checklist

### Visual Quality
- [ ] No emojis as icons (use SVG instead)
- [ ] All icons come from a consistent icon family
- [ ] Pressed-state visuals do not shift layout
- [ ] Semantic theme tokens used consistently

### Interaction
- [ ] All tappable elements provide pressed feedback
- [ ] Touch targets meet minimum size (44x44pt)
- [ ] Micro-interaction timing stays 150-300ms
- [ ] Disabled states are visually clear
- [ ] Screen reader focus matches visual order

### Light/Dark Mode
- [ ] Primary text contrast >=4.5:1 in both modes
- [ ] Dividers and interaction states distinguishable in both modes
- [ ] Modal scrim preserves foreground legibility

### Layout
- [ ] Safe areas respected for headers, tab bars, CTAs
- [ ] Scroll content not hidden behind fixed bars
- [ ] Verified on small phone, large phone, tablet
- [ ] 4/8dp spacing rhythm maintained

### Accessibility
- [ ] All meaningful images/icons have accessibility labels
- [ ] Form fields have labels, hints, clear errors
- [ ] Color is not the only indicator
- [ ] Reduced motion supported without layout breakage

## Common Rules for Professional UI

### Icons & Visual Elements
| Rule | Standard |
|------|----------|
| No Emoji as Structural Icons | Use vector-based icons (Lucide, react-native-vector-icons) |
| Vector-Only Assets | Use SVG or platform vector icons |
| Consistent Icon Sizing | Define icon sizes as design tokens (icon-sm, icon-md = 24pt) |
| Stroke Consistency | Use a consistent stroke width within same visual layer |
| Filled vs Outline Discipline | Use one icon style per hierarchy level |
| Icon Contrast | Follow WCAG: 4.5:1 for small elements, 3:1 for larger glyphs |

### Interaction (App)
| Rule | Do | Don't |
|------|----|-------|
| Tap feedback | Pressed feedback within 80-150ms | No visual response on tap |
| Animation timing | 150-300ms with native easing | Instant or >500ms |
| Disabled state clarity | Use disabled semantics | Controls that look tappable but do nothing |
| Touch target minimum | >=44x44pt / >=48x48dp | Tiny tap targets |

### Light/Dark Mode Contrast
| Rule | Do | Don't |
|------|----|-------|
| Surface readability | Clear card separation | Overly transparent surfaces |
| Text contrast (light) | Body text >=4.5:1 | Low-contrast gray text |
| Text contrast (dark) | Primary >=4.5:1, secondary >=3:1 | Text blending into background |
| Token-driven theming | Semantic color tokens per theme | Hardcoded per-screen hex values |

### Layout & Spacing
| Rule | Do | Don't |
|------|----|-------|
| Safe-area compliance | Respect top/bottom safe areas | UI under notch/gesture area |
| 8dp spacing rhythm | Consistent 4/8dp spacing system | Random spacing increments |
| Section spacing hierarchy | Define vertical rhythm tiers | Inconsistent spacing |
| Adaptive gutters | Increase insets on larger widths | Same narrow gutter on all sizes |

## Pre-Delivery Checklist (App UI)

### Visual Quality
- [ ] No emojis as icons (use SVG instead)
- [ ] All icons from consistent icon family and style
- [ ] Pressed-state visuals do not shift layout bounds
- [ ] Semantic theme tokens used consistently (no ad-hoc per-screen hex)

### Interaction
- [ ] All tappable elements provide clear pressed feedback (ripple/opacity/elevation)
- [ ] Touch targets meet minimum size (>=44x44pt iOS, >=48x48dp Android)
- [ ] Micro-interaction timing stays in 150-300ms range
- [ ] Disabled states are visually clear and non-interactive
- [ ] Screen reader focus order matches visual order

### Light/Dark Mode
- [ ] Primary text contrast >=4.5:1 in both light and dark mode
- [ ] Secondary text contrast >=3:1 in both modes
- [ ] Dividers/borders and interaction states distinguishable in both modes
- [ ] Modal/drawer scrim preserves foreground legibility (40-60% black)
- [ ] Both themes tested before delivery

### Layout
- [ ] Safe areas respected for headers, tab bars, bottom CTAs
- [ ] Scroll content not hidden behind fixed/sticky bars
- [ ] Verified on small phone, large phone, tablet (portrait + landscape)
- [ ] 4/8dp spacing rhythm maintained across component, section, page levels
- [ ] Readable text measure on larger devices (no edge-to-edge paragraphs)

### Accessibility
- [ ] All meaningful images/icons have accessibility labels
- [ ] Form fields have labels, hints, and clear error messages
- [ ] Color is not the only indicator
- [ ] Reduced motion and dynamic text size supported without breakage
- [ ] Accessibility traits/roles/states announced correctly
