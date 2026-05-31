import { en } from '../infrastructure/translations/en';
import { es } from '../infrastructure/translations/es';

const STORAGE_KEY = 'cv-locale';

type Locale = 'en' | 'es';

const translations: Record<Locale, Record<string, string>> = { en, es };

let currentLocale: Locale = loadStored();

function loadStored(): Locale {
  const stored = localStorage.getItem(STORAGE_KEY);
  if (stored === 'en' || stored === 'es') return stored;
  return navigator.language.startsWith('es') ? 'es' : 'en';
}

// Sync <html data-locale> on module load
document.documentElement.setAttribute('data-locale', currentLocale);

export function t(key: string, params?: Record<string, string | number>): string {
  const value = translations[currentLocale]?.[key];
  if (value === undefined) return key;
  if (!params) return value;
  return Object.entries(params).reduce(
    (str, [k, v]) => str.replace(`{${k}}`, String(v)),
    value,
  );
}

export function getLocale(): Locale {
  return currentLocale;
}

export function setLocale(locale: Locale): void {
  if (locale === currentLocale) return;
  currentLocale = locale;
  localStorage.setItem(STORAGE_KEY, locale);
  document.documentElement.setAttribute('data-locale', locale);
}
