import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { Intro } from '../../src/ui/components/Intro';
import { TranslationProvider } from '../../src/ui/contexts/TranslationContext';

describe('Intro', () => {
  it('renders name and summary', () => {
    render(<TranslationProvider><Intro name="John **Doe**" summary="Hello **world**" /></TranslationProvider>);
    expect(screen.getByText(/John/)).toBeInTheDocument();
    expect(screen.getByText(/Hello/)).toBeInTheDocument();
  });

  it('renders bold text in strong element', () => {
    render(<TranslationProvider><Intro name="John **Doe**" summary="Hello **world**" /></TranslationProvider>);
    const strong = screen.getByText('world');
    expect(strong.tagName).toBe('STRONG');
  });
});
