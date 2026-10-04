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

  it('renders README link when gitHubUrl provided', () => {
    render(
      <TranslationProvider>
        <Intro name="John" summary="Hello" gitHubUrl="https://github.com/john" />
      </TranslationProvider>,
    );
    const link = screen.getByText('Review my full stack on GitHub');
    expect(link).toHaveAttribute('href', 'https://github.com/john');
    expect(link).toHaveAttribute('target', '_blank');
    expect(link).toHaveAttribute('rel', 'noopener noreferrer');
  });

  it('omits README link when gitHubUrl missing', () => {
    render(<TranslationProvider><Intro name="John" summary="Hello" /></TranslationProvider>);
    expect(screen.queryByText('Review my full stack on GitHub')).not.toBeInTheDocument();
  });
});
