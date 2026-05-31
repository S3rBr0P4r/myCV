const STORAGE_KEY = 'cv-theme';

type Theme = 'light' | 'dark';

export class ThemeManager {
  private toggle: HTMLElement | null = null;

  applyStored(): void {
    this.applyTheme(this.getPreferredTheme());
  }

  init(): void {
    this.toggle = document.getElementById('themeToggle');
    if (this.toggle) {
      this.toggle.addEventListener('click', this.handleToggle);
    }
  }

  private getPreferredTheme(): Theme {
    const stored = localStorage.getItem(STORAGE_KEY);
    if (stored === 'light' || stored === 'dark') return stored;

    if (window.matchMedia('(prefers-color-scheme: dark)').matches) {
      return 'dark';
    }

    return 'light';
  }

  private applyTheme(theme: Theme): void {
    document.documentElement.classList.add('no-transition');
    document.documentElement.setAttribute('data-theme', theme);
    localStorage.setItem(STORAGE_KEY, theme);
    requestAnimationFrame(() => {
      requestAnimationFrame(() => {
        document.documentElement.classList.remove('no-transition');
      });
    });
  }

  private handleToggle = (): void => {
    const current = document.documentElement.getAttribute('data-theme') as Theme | null;
    const next: Theme = current === 'dark' ? 'light' : 'dark';
    this.applyTheme(next);
  };
}
