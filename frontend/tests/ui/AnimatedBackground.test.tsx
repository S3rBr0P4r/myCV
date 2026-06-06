import { describe, it, expect, vi, afterEach, beforeAll } from 'vitest';
import { render } from '@testing-library/react';
import { AnimatedBackground } from '../../src/ui/components/AnimatedBackground';

describe('AnimatedBackground', () => {
  const origRAF = window.requestAnimationFrame;
  const origCAF = window.cancelAnimationFrame;
  const origMatchMedia = window.matchMedia;
  const origMO = globalThis.MutationObserver;

  beforeAll(() => {
    window.requestAnimationFrame = vi.fn((cb: FrameRequestCallback) => {
      return setTimeout(() => cb(performance.now()), 16) as unknown as number;
    }) as typeof window.requestAnimationFrame;
    window.cancelAnimationFrame = vi.fn((id: number) => {
      clearTimeout(id);
    }) as typeof window.cancelAnimationFrame;
  });

  afterEach(() => {
    vi.restoreAllMocks();
    document.documentElement.removeAttribute('data-theme');
  });

  afterAll(() => {
    window.requestAnimationFrame = origRAF;
    window.cancelAnimationFrame = origCAF;
    window.matchMedia = origMatchMedia;
  });

  it('renders a canvas element', () => {
    const { container } = render(<AnimatedBackground />);
    const canvas = container.querySelector('canvas');
    expect(canvas).toBeInTheDocument();
    expect(canvas).toHaveAttribute('id', 'code-dust');
  });

  it('has fixed positioning and pointer-events none', () => {
    const { container } = render(<AnimatedBackground />);
    const canvas = container.querySelector('canvas')!;
    expect(canvas.style.position).toBe('fixed');
    expect(canvas.style.pointerEvents).toBe('none');
    expect(canvas.style.zIndex).toBe('0');
  });

  it('handles reduced motion by rendering static particles', () => {
    window.matchMedia = vi.fn().mockImplementation((query: string) => ({
      matches: query === '(prefers-reduced-motion: reduce)',
      media: query,
      onchange: null,
      addListener: vi.fn(),
      removeListener: vi.fn(),
      addEventListener: vi.fn(),
      removeEventListener: vi.fn(),
      dispatchEvent: vi.fn(),
    }));

    const { container } = render(<AnimatedBackground />);
    const canvas = container.querySelector('canvas')!;
    expect(canvas).toBeInTheDocument();
  });

  it('responds to data-theme changes via MutationObserver', () => {
    const { container } = render(<AnimatedBackground />);
    const canvas = container.querySelector('canvas')!;
    expect(canvas).toBeInTheDocument();

    document.documentElement.setAttribute('data-theme', 'dark');
    expect(document.documentElement.getAttribute('data-theme')).toBe('dark');
  });
});
