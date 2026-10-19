import { describe, it, expect, beforeEach } from 'vitest';
import { render, screen, fireEvent } from '@testing-library/react';
import { Footer } from '../../src/ui/components/Footer';
import { TranslationProvider } from '../../src/ui/contexts/TranslationContext';
import {
  CONSENT_STORAGE_KEY,
  COOKIE_NOTICE_REOPEN_EVENT,
  setConsent,
} from '../../src/ui/consent';

describe('Footer', () => {
  beforeEach(() => {
    localStorage.clear();
  });

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

  it('clears the consent record and dispatches reopen on cookie settings click', () => {
    setConsent('acknowledged');
    let reopened = false;
    window.addEventListener(COOKIE_NOTICE_REOPEN_EVENT, () => { reopened = true; }, { once: true });

    render(<TranslationProvider><Footer name="John" /></TranslationProvider>);
    fireEvent.click(screen.getByRole('button', { name: 'Cookie settings' }));

    expect(reopened).toBe(true);
    expect(localStorage.getItem(CONSENT_STORAGE_KEY)).toBeNull();
  });
});
