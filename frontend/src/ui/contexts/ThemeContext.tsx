import { createContext, useCallback, useEffect, useState, type ReactNode } from 'react';

type Theme = 'light' | 'dark';
const STORAGE_KEY = 'cv-theme';

interface ThemeContextValue {
  theme: Theme;
  toggleTheme: () => void;
}

const getPreferred = (): Theme => {
  const stored = localStorage.getItem(STORAGE_KEY);
  if (stored === 'light' || stored === 'dark') return stored;
  return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
};

const apply = (theme: Theme) => {
  document.documentElement.classList.add('no-transition');
  document.documentElement.setAttribute('data-theme', theme);
  localStorage.setItem(STORAGE_KEY, theme);
  requestAnimationFrame(() => requestAnimationFrame(() => {
    document.documentElement.classList.remove('no-transition');
  }));
};

export const ThemeContext = createContext<ThemeContextValue>({
  theme: 'light',
  toggleTheme: () => {},
});

export function ThemeProvider({ children }: { children: ReactNode }) {
  const [theme, setTheme] = useState<Theme>(getPreferred);

  useEffect(() => { apply(theme); }, [theme]);

  const toggleTheme = useCallback(() => {
    setTheme(prev => prev === 'dark' ? 'light' : 'dark');
  }, []);

  return (
    <ThemeContext.Provider value={{ theme, toggleTheme }}>
      {children}
    </ThemeContext.Provider>
  );
}
