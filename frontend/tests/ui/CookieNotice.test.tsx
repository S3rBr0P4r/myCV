import { describe, it, expect, beforeEach } from 'vitest';
import { render, screen, fireEvent, act } from '@testing-library/react';
import { CookieNotice } from '../../src/ui/components/CookieNotice';
import { TranslationProvider } from '../../src/ui/contexts/TranslationContext';
import { ThemeProvider } from '../../src/ui/contexts/ThemeContext';
import {
  CONSENT_STORAGE_KEY,
  COOKIE_NOTICE_REOPEN_EVENT,
  NOTICE_VERSION,
  clearConsent,
  setConsent,
} from '../../src/ui/consent';
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

describe('CookieNotice', () => {
  beforeEach(() => {
    localStorage.clear();
    document.documentElement.style.removeProperty('--cookie-h');
  });

  it('renders notice with title and dismiss button when no record exists', () => {
    render(<CookieNotice />, { wrapper: Wrapper });
    expect(screen.getByRole('region', { name: 'Cookie notice' })).toBeInTheDocument();
    expect(screen.getByText('About cookies')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Got it' })).toBeInTheDocument();
  });

  it('hides after dismiss and persists an acknowledged record', () => {
    render(<CookieNotice />, { wrapper: Wrapper });
    fireEvent.click(screen.getByRole('button', { name: 'Got it' }));

    expect(screen.queryByText('About cookies')).not.toBeInTheDocument();

    const stored = JSON.parse(localStorage.getItem(CONSENT_STORAGE_KEY) ?? 'null');
    expect(stored).toMatchObject({ version: NOTICE_VERSION, status: 'acknowledged' });
    expect(typeof stored.decidedAt).toBe('string');
  });

  it('does not render when a current-version record exists', () => {
    setConsent('acknowledged');
    render(<CookieNotice />, { wrapper: Wrapper });
    expect(screen.queryByText('About cookies')).not.toBeInTheDocument();
  });

  it('reappears after reopen event once the record is cleared', () => {
    setConsent('acknowledged');
    render(<CookieNotice />, { wrapper: Wrapper });
    expect(screen.queryByText('About cookies')).not.toBeInTheDocument();

    clearConsent();
    act(() => {
      window.dispatchEvent(new CustomEvent(COOKIE_NOTICE_REOPEN_EVENT));
    });

    expect(screen.getByText('About cookies')).toBeInTheDocument();
  });

  it('stays hidden when a stale event fires while a record exists', () => {
    setConsent('acknowledged');
    render(<CookieNotice />, { wrapper: Wrapper });
    act(() => {
      window.dispatchEvent(new CustomEvent(COOKIE_NOTICE_REOPEN_EVENT));
    });
    expect(screen.queryByText('About cookies')).not.toBeInTheDocument();
  });

  it('re-shows the notice for an outdated record version', () => {
    localStorage.setItem(
      CONSENT_STORAGE_KEY,
      JSON.stringify({ version: NOTICE_VERSION - 1, status: 'acknowledged', decidedAt: 'x' }),
    );
    render(<CookieNotice />, { wrapper: Wrapper });
    expect(screen.getByText('About cookies')).toBeInTheDocument();
  });

  it('cleans up the height variable when dismissed', () => {
    render(<CookieNotice />, { wrapper: Wrapper });
    fireEvent.click(screen.getByRole('button', { name: 'Got it' }));
    expect(document.documentElement.style.getPropertyValue('--cookie-h')).toBe('');
  });
});
