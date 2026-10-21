import { describe, it, expect, afterEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import { LoadingState } from '../../src/ui/components/LoadingState';
import { TranslationProvider } from '../../src/ui/contexts/TranslationContext';
import { ThemeProvider } from '../../src/ui/contexts/ThemeContext';

function renderLoadingState() {
  return render(
    <ThemeProvider>
      <TranslationProvider>
        <LoadingState />
      </TranslationProvider>
    </ThemeProvider>,
  );
}

function spriteSrc(container: HTMLElement): string | null {
  return container.querySelector('.loading-state-sprite')?.getAttribute('src') ?? null;
}

describe('LoadingState', () => {
  afterEach(() => {
    localStorage.removeItem('cv-theme');
    localStorage.removeItem('cv-locale');
    document.documentElement.removeAttribute('data-theme');
    document.documentElement.removeAttribute('data-locale');
  });

  it('renders the sprite for the current locale and theme', () => {
    const { container } = renderLoadingState();
    expect(spriteSrc(container)).toBe('/loading/en-light.webp');
  });

  it('renders the dark sprite when the dark theme is active', () => {
    localStorage.setItem('cv-theme', 'dark');
    const { container } = renderLoadingState();
    expect(spriteSrc(container)).toBe('/loading/en-dark.webp');
  });

  it('renders the Spanish sprite when the locale is Spanish', () => {
    localStorage.setItem('cv-locale', 'es');
    const { container } = renderLoadingState();
    expect(spriteSrc(container)).toBe('/loading/es-light.webp');
  });

  it('renders the Spanish dark sprite for the Spanish dark theme', () => {
    localStorage.setItem('cv-locale', 'es');
    localStorage.setItem('cv-theme', 'dark');
    const { container } = renderLoadingState();
    expect(spriteSrc(container)).toBe('/loading/es-dark.webp');
  });

  it('declares the sprite as decorative', () => {
    const { container } = renderLoadingState();
    const sprite = container.querySelector('.loading-state-sprite');
    expect(sprite).toHaveAttribute('alt', '');
  });

  it('exposes the loading message to assistive technology', () => {
    renderLoadingState();
    expect(screen.getByRole('status')).toHaveTextContent('Loading…');
  });

  it('announces the loading message in Spanish', () => {
    localStorage.setItem('cv-locale', 'es');
    renderLoadingState();
    expect(screen.getByRole('status')).toHaveTextContent('Cargando…');
  });
});
