import { ApiCVRepository } from './infrastructure/repositories/ApiCVRepository';
import { GetCVUseCase } from './application/use-cases/GetCVUseCase';
import { ScrollObserver } from './core/Observer';
import { ParallaxEffect } from './core/Parallax';
import { ThemeManager } from './core/ThemeManager';
import { getLocale, setLocale } from './core/TranslationService';
import { CVRenderer } from './ui/CVRenderer';

setCSP();

class App {
  private getCVUseCase: GetCVUseCase;
  private themeManager: ThemeManager;
  private cvData: import('./domain/entities/CV').CV | null = null;

  constructor() {
    const repository = new ApiCVRepository();
    this.getCVUseCase = new GetCVUseCase(repository);

    this.themeManager = new ThemeManager();
    this.themeManager.applyStored();

    this.init();
  }

  private async init(): Promise<void> {
    try {
      this.cvData = await this.getCVUseCase.execute(getLocale());
      this.render();
      this.themeManager.init();
      this.initAnimations();
    } catch (error) {
      console.error('App initialization failed:', error);
    }
  }

  private render(): void {
    if (!this.cvData) return;
    CVRenderer.render(this.cvData);
    this.themeManager.init();
    this.initLocaleToggle();
  }

  private initAnimations(): void {
    new ScrollObserver('.reveal').observe();
    new ParallaxEffect('bg', 0.15).init();

    const sections = document.querySelectorAll<HTMLElement>('section[id]');
    const dots = document.querySelectorAll<HTMLAnchorElement>('.scroll-progress a');

    const sectionObserver = new IntersectionObserver(entries => {
      entries.forEach(entry => {
        if (entry.isIntersecting) {
          dots.forEach(d => d.classList.toggle('active', d.dataset.section === entry.target.id));
        }
      });
    }, { threshold: 0.3, rootMargin: '0px 0px -30% 0px' });

    sections.forEach(s => sectionObserver.observe(s));
  }

  private initLocaleToggle(): void {
    const trigger = document.getElementById('localeTrigger');
    const menu = document.getElementById('localeMenu');
    const dropdown = document.querySelector<HTMLElement>('.locale-dropdown');
    if (!trigger || !menu || !dropdown) return;

    const toggle = () => dropdown.classList.toggle('locale-menu--open');
    const close = () => dropdown.classList.remove('locale-menu--open');

    trigger.onclick = (e) => {
      e.stopPropagation();
      toggle();
    };

    document.querySelectorAll<HTMLButtonElement>('.locale-option').forEach(btn => {
      btn.onclick = () => {
        const locale = btn.dataset.locale;
        if (!locale || locale === getLocale()) { close(); return; }
        setLocale(locale as 'en' | 'es');
        close();
        this.refetchAndRender();
      };
    });

    document.addEventListener('click', (e) => {
      if (!dropdown.contains(e.target as Node)) close();
    });

    document.addEventListener('keydown', (e) => {
      if (e.key === 'Escape') close();
    });
  }

  private async refetchAndRender(): Promise<void> {
    try {
      this.cvData = await this.getCVUseCase.execute(getLocale());
      this.render();
      this.initAnimations();
    } catch (error) {
      console.error('Re-fetch failed:', error);
    }
  }
}

function setCSP(): void {
  const apiUrl = import.meta.env.VITE_API_URL || '';
  let connectSrc = "'self'";
  if (apiUrl) {
    try {
      const origin = new URL(apiUrl, window.location.origin).origin;
      if (origin !== window.location.origin) {
        connectSrc += ` ${origin}`;
      }
    } catch { /* relative URL — same origin */ }
  }
  const meta = document.createElement('meta');
  meta.httpEquiv = 'Content-Security-Policy';
  meta.content = `default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline'; font-src 'self' https://fonts.gstatic.com; img-src 'self' data:; connect-src ${connectSrc}; frame-ancestors 'none'; base-uri 'self'; form-action 'self'`;
  document.head.appendChild(meta);
}

document.addEventListener('DOMContentLoaded', () => new App());
