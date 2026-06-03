import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { Footer } from '../../src/ui/components/Footer';
import { TranslationProvider } from '../../src/ui/contexts/TranslationContext';

describe('Footer', () => {
  it('renders heading', () => {
    render(<TranslationProvider><Footer name="John" /></TranslationProvider>);
    expect(screen.getByText("Let's make things happen")).toBeInTheDocument();
  });

  it('renders email when provided', () => {
    render(
      <TranslationProvider>
        <Footer
          name="John"
          contactInfo={{
            email: 'john@example.com',
            phone: '',
            location: '',
            willingnessToTravel: '',
          }}
        />
      </TranslationProvider>,
    );
    expect(screen.getByText('john@example.com')).toBeInTheDocument();
  });

  it('renders footer with name', () => {
    render(<TranslationProvider><Footer name="John" /></TranslationProvider>);
    expect(screen.getByText(/John/)).toBeInTheDocument();
  });
});
