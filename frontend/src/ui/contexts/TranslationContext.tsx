import { createContext, useCallback, useState, type ReactNode } from 'react';
import { en } from '../../infrastructure/translations/en';
import { es } from '../../infrastructure/translations/es';

type Locale = 'en' | 'es';
const STORAGE_KEY = 'cv-locale';

const translations: Record<Locale, Record<string, string>> = { en, es };

function loadStored(): Locale {
  const stored = localStorage.getItem(STORAGE_KEY);
  if (stored === 'en' || stored === 'es') return stored;
  return 'en';
}

interface TranslationContextValue {
  locale: Locale;
  t: (key: string, params?: Record<string, string | number>) => string;
  setLocale: (locale: Locale) => void;
}

export const TranslationContext = createContext<TranslationContextValue>({
  locale: 'en',
  t: (key: string) => key,
  setLocale: () => {},
});

export function TranslationProvider({ children }: { children: ReactNode }) {
  const [locale, setLocaleState] = useState<Locale>(loadStored);

  const setLocale = useCallback((newLocale: Locale) => {
    if (newLocale === locale) return;
    setLocaleState(newLocale);
    localStorage.setItem(STORAGE_KEY, newLocale);
    document.documentElement.setAttribute('data-locale', newLocale);
  }, [locale]);

  const t = useCallback(
    (key: string, params?: Record<string, string | number>): string => {
      const value = translations[locale]?.[key];
      if (value === undefined) return key;
      if (!params) return value;
      return Object.entries(params).reduce(
        (str, [k, v]) => str.replace(`{${k}}`, String(v)),
        value,
      );
    },
    [locale],
  );

  return (
    <TranslationContext.Provider value={{ locale, t, setLocale }}>
      {children}
    </TranslationContext.Provider>
  );
}
