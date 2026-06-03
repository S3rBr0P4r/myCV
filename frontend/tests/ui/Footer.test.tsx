import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { Footer } from '../../src/ui/components/Footer';
import { TranslationProvider } from '../../src/ui/contexts/TranslationContext';

describe('Footer', () => {
  it('renders heading', () => {
    render(<TranslationProvider><Footer name="John" /></TranslationProvider>);
    expect(screen.getByText("Let's Talk?")).toBeInTheDocument();
  });

  it('renders contact info when provided', () => {
    render(
      <TranslationProvider>
        <Footer
          name="John"
          contactInfo={{
            email: 'john@example.com',
            phone: '+1 555',
            location: 'NYC',
            willingnessToTravel: 'Yes',
          }}
        />
      </TranslationProvider>,
    );
    expect(screen.getByText('john@example.com')).toBeInTheDocument();
    expect(screen.getByText(/\+1 555/)).toBeInTheDocument();
  });

  it('renders footer with name', () => {
    render(<TranslationProvider><Footer name="John" /></TranslationProvider>);
    expect(screen.getByText(/John/)).toBeInTheDocument();
  });
});
