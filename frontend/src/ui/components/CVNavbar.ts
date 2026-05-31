import { t, getLocale } from '../../core/TranslationService';

const ukFlag = '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 60 40" width="18" height="12"><rect width="60" height="40" fill="#012169" stroke="#fff" stroke-width="0.5"/><path d="M0,0L60,40M60,0L0,40" stroke="#FFF" stroke-width="6"/><path d="M0,0L60,40M60,0L0,40" stroke="#C8102E" stroke-width="3"/><path d="M0,20L60,20M30,0L30,40" stroke="#FFF" stroke-width="6"/><path d="M0,20L60,20M30,0L30,40" stroke="#C8102E" stroke-width="3"/></svg>';
const esFlag = '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 60 40" width="18" height="12"><rect width="60" height="40" fill="#C60B1E" stroke="#fff" stroke-width="0.5"/><rect y="10" width="60" height="20" fill="#FFC400"/></svg>';

const locales = [
  { code: 'en', flag: ukFlag, labelKey: 'nav.localeEn' },
  { code: 'es', flag: esFlag, labelKey: 'nav.localeEs' },
];

export function renderNavbar(): string {
  const current = getLocale();
  const active = locales.find(l => l.code === current)!;

  return `
    <nav class="navbar">
      <div class="logo">${t('nav.logo')}<span>${t('nav.logoSuffix')}</span></div>
      <div class="navbar-actions">
        <div class="locale-dropdown">
          <button class="locale-trigger" id="localeTrigger" type="button"
            aria-label="${t('nav.localeLabel')}"
            title="${t('nav.localeLabel')}">
            ${active.flag}
            <span class="locale-caret" aria-hidden="true">▾</span>
          </button>
          <ul class="locale-menu" id="localeMenu">
            ${locales.map(l => `
              <li><button class="locale-option" data-locale="${l.code}" type="button"
                aria-label="${t(l.labelKey)}"
                title="${t(l.labelKey)}">${l.flag} ${t(l.labelKey)}</button></li>
            `).join('')}
          </ul>
        </div>
        <button class="theme-toggle" id="themeToggle" type="button"
          aria-label="${t('nav.themeLabel')}"
          title="${t('nav.themeLabel')}">
          <span class="icon-sun" aria-hidden="true">☀️</span>
          <span class="icon-moon" aria-hidden="true">🌙</span>
        </button>
      </div>
    </nav>
  `;
}
