import { useTranslation } from '../hooks/useTranslation';
import { useTheme } from '../hooks/useTheme';
import { useCV } from '../hooks/useCV';

const sunSvg = '<svg class="icon-sun" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="5"/><path d="M12 1v2M12 21v2M4.22 4.22l1.42 1.42M18.36 18.36l1.42 1.42M1 12h2M21 12h2M4.22 19.78l1.42-1.42M18.36 5.64l1.42-1.42"/></svg>';
const moonSvg = '<svg class="icon-moon" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><path d="M21 12.79A9 9 0 1 1 11.21 3 7 7 0 0 0 21 12.79z"/></svg>';

const linkedInSvg = '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="currentColor"><path d="M19 3a2 2 0 0 1 2 2v14a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h14zm-.5 15.5v-5.3a3.26 3.26 0 0 0-3.26-3.26c-.85 0-1.84.52-2.32 1.3v-1.11h-2.79v8.37h2.79v-4.93c0-.77.62-1.4 1.39-1.4a1.4 1.4 0 0 1 1.4 1.4v4.93h2.79zM6.88 8.56a1.68 1.68 0 0 0 1.68-1.68c0-.93-.75-1.69-1.68-1.69a1.69 1.69 0 0 0-1.69 1.69c0 .93.76 1.68 1.69 1.68zm1.39 9.94v-8.37H5.5v8.37h2.77z"/></svg>';
const gitHubSvg = '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="currentColor"><path d="M12 0C5.37 0 0 5.37 0 12c0 5.3 3.438 9.8 8.205 11.387.6.113.82-.258.82-.577 0-.285-.01-1.04-.015-2.04-3.338.724-4.042-1.61-4.042-1.61C4.422 18.07 3.633 17.7 3.633 17.7c-1.087-.744.084-.729.084-.729 1.205.084 1.838 1.236 1.838 1.236 1.07 1.835 2.809 1.305 3.495.998.108-.776.417-1.305.76-1.605-2.665-.3-5.466-1.332-5.466-5.93 0-1.31.465-2.38 1.235-3.22-.135-.303-.54-1.523.105-3.176 0 0 1.005-.322 3.3 1.23.96-.267 1.98-.399 3-.405 1.02.006 2.04.138 3 .405 2.28-1.552 3.285-1.23 3.285-1.23.645 1.653.24 2.873.12 3.176.765.84 1.23 1.91 1.23 3.22 0 4.61-2.805 5.625-5.475 5.92.42.36.81 1.096.81 2.22 0 1.606-.015 2.896-.015 3.286 0 .315.21.69.825.57C20.565 21.795 24 17.295 24 12 24 5.37 18.63 0 12 0z"/></svg>';
const emailSvg = '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><rect x="2" y="4" width="20" height="16" rx="2"/><path d="M22 4l-10 8L2 4"/></svg>';

const locales = [
  { code: 'en' as const, flagSrc: '/flags/gb.svg', label: 'GB', ariaKey: 'nav.localeEn' },
  { code: 'es' as const, flagSrc: '/flags/es.svg', label: 'ES', ariaKey: 'nav.localeEs' },
];

export function Header() {
  const { t, locale, startTransition } = useTranslation();
  const { toggleTheme } = useTheme();
  const { cv } = useCV();
  const active = locales.find(l => l.code === locale)!;

  return (
    <nav className="navbar">
      <div className="navbar-inner">
        <div className="navbar-social">
          {cv?.linkedInUrl && (
            <a
              href={cv.linkedInUrl}
              className="social-link"
              target="_blank"
              rel="noopener noreferrer"
              aria-label="LinkedIn"
              title="LinkedIn"
              dangerouslySetInnerHTML={{ __html: linkedInSvg }}
            />
          )}
          {cv?.gitHubUrl && (
            <a
              href={cv.gitHubUrl}
              className="social-link"
              target="_blank"
              rel="noopener noreferrer"
              aria-label="GitHub"
              title="GitHub"
              dangerouslySetInnerHTML={{ __html: gitHubSvg }}
            />
          )}
          {cv?.contactInfo?.email && (
            <a
              href={`mailto:${cv.contactInfo.email}`}
              className="social-link"
              aria-label="Email"
              title="Email"
              dangerouslySetInnerHTML={{ __html: emailSvg }}
            />
          )}
        </div>

        <div className="navbar-actions">
          <div className="locale-dropdown">
            <button
              className="locale-trigger"
              type="button"
              aria-label={t('nav.localeLabel')}
              title={t('nav.localeLabel')}
              onClick={e => {
                e.stopPropagation();
                const dd = e.currentTarget.closest('.locale-dropdown');
                dd?.classList.toggle('locale-menu--open');
              }}
            >
              <img className="locale-flag" src={active.flagSrc} alt="" width="28" height="28" />
              <span className="locale-label">{active.label}</span>
              <span className="locale-caret" aria-hidden="true">▾</span>
            </button>
            <ul className="locale-menu">
              {locales.map(l => (
                <li key={l.code}>
                  <button
                    className="locale-option"
                    data-locale={l.code}
                    type="button"
                    aria-label={t(l.ariaKey)}
                    title={t(l.ariaKey)}
                    onClick={e => {
                      startTransition(l.code);
                      const dd = e.currentTarget.closest('.locale-dropdown');
                      dd?.classList.remove('locale-menu--open');
                    }}
                  >
                    <img className="locale-flag" src={l.flagSrc} alt="" width="24" height="24" />
                    <span>{l.label}</span>
                  </button>
                </li>
              ))}
            </ul>
          </div>
          <button
            className="theme-toggle"
            id="themeToggle"
            type="button"
            aria-label={t('nav.themeLabel')}
            title={t('nav.themeLabel')}
            onClick={toggleTheme}
            dangerouslySetInnerHTML={{ __html: sunSvg + moonSvg }}
          />
        </div>
      </div>
    </nav>
  );
}
