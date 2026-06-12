import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { Feedback } from '../../src/ui/components/Feedback';
import { TranslationProvider } from '../../src/ui/contexts/TranslationContext';
import { ThemeProvider } from '../../src/ui/contexts/ThemeContext';
import type { ReactNode } from 'react';

function Wrapper({ children }: { children: ReactNode }) {
  return (
    <TranslationProvider>
      <ThemeProvider>
        {children}
      </ThemeProvider>
    </TranslationProvider>
  );
}

describe('Feedback', () => {
  it('renders FAB button with aria-label', () => {
    render(<Feedback />, { wrapper: Wrapper });
    const fab = screen.getByRole('button', { name: /send feedback/i });
    expect(fab).toBeInTheDocument();
  });
});
