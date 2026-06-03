import { describe, it, expect, afterEach } from 'vitest';
import { render, screen, fireEvent } from '@testing-library/react';
import { TranslationProvider, TranslationContext } from '../../src/ui/contexts/TranslationContext';
import { useContext } from 'react';

function TestConsumer() {
  const { locale, t, setLocale } = useContext(TranslationContext);
  return (
    <div>
      <span data-testid="locale">{locale}</span>
      <span data-testid="translated">{t('nav.themeLabel')}</span>
      <button data-testid="set-es" onClick={() => setLocale('es')}>ES</button>
    </div>
  );
}

describe('TranslationContext', () => {
  afterEach(() => {
    localStorage.removeItem('cv-locale');
    document.documentElement.removeAttribute('data-locale');
  });

  it('provides English by default', () => {
    render(
      <TranslationProvider>
        <TestConsumer />
      </TranslationProvider>,
    );
    expect(screen.getByTestId('locale').textContent).toBe('en');
  });

  it('translates keys', () => {
    render(
      <TranslationProvider>
        <TestConsumer />
      </TranslationProvider>,
    );
    expect(screen.getByTestId('translated').textContent).toBe('Switch theme');
  });

  it('switches locale', () => {
    render(
      <TranslationProvider>
        <TestConsumer />
      </TranslationProvider>,
    );
    fireEvent.click(screen.getByTestId('set-es'));
    expect(screen.getByTestId('locale').textContent).toBe('es');
    expect(screen.getByTestId('translated').textContent).toBe('Cambiar tema');
  });
});
