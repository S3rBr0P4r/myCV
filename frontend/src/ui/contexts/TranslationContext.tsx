import { createContext, useCallback, useEffect, useRef, useState, type ReactNode } from 'react';
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

const FADE_OUT = 500;
const HOLD = 300;
const FADE_IN = 400;

interface TranslationContextValue {
  locale: Locale;
  t: (key: string, params?: Record<string, string | number>) => string;
  setLocale: (locale: Locale) => void;
  startTransition: (locale: Locale) => void;
  isTransitioning: boolean;
}

export const TranslationContext = createContext<TranslationContextValue>({
  locale: 'en',
  t: (key: string) => key,
  setLocale: () => {},
  startTransition: () => {},
  isTransitioning: false,
});

export function TranslationProvider({ children }: { children: ReactNode }) {
  const [locale, setLocaleState] = useState<Locale>(loadStored);
  const [isTransitioning, setIsTransitioning] = useState(false);
  const timeoutRef = useRef<ReturnType<typeof setTimeout>>();

  useEffect(() => {
    return () => clearTimeout(timeoutRef.current);
  }, []);

  const setLocale = useCallback((newLocale: Locale) => {
    if (newLocale === locale) return;
    setLocaleState(newLocale);
    localStorage.setItem(STORAGE_KEY, newLocale);
    document.documentElement.setAttribute('data-locale', newLocale);
  }, [locale]);

  const startTransition = useCallback((newLocale: Locale) => {
    if (newLocale === locale || isTransitioning) return;

    setIsTransitioning(true);
    document.documentElement.classList.add('lang-transitioning');

    timeoutRef.current = setTimeout(() => {
      setLocaleState(newLocale);
      localStorage.setItem(STORAGE_KEY, newLocale);
      document.documentElement.setAttribute('data-locale', newLocale);

      document.documentElement.classList.remove('lang-transitioning');
      document.documentElement.classList.add('lang-entering');

      timeoutRef.current = setTimeout(() => {
        setIsTransitioning(false);
        document.documentElement.classList.remove('lang-entering');
      }, FADE_IN);
    }, FADE_OUT + HOLD);
  }, [locale, isTransitioning]);

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
    <TranslationContext.Provider value={{ locale, t, setLocale, startTransition, isTransitioning }}>
      {children}
    </TranslationContext.Provider>
  );
}
