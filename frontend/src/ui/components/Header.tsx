import { useTranslation } from '../hooks/useTranslation';
import { useTheme } from '../hooks/useTheme';
import { renderFormattedText } from '../format';

const sunSvg = '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="5"/><path d="M12 1v2M12 21v2M4.22 4.22l1.42 1.42M18.36 18.36l1.42 1.42M1 12h2M21 12h2M4.22 19.78l1.42-1.42M18.36 5.64l1.42-1.42"/></svg>';
const moonSvg = '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><path d="M21 12.79A9 9 0 1 1 11.21 3 7 7 0 0 0 21 12.79z"/></svg>';

const ukFlag = '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 60 40" width="20" height="14"><rect width="60" height="40" fill="#012169" stroke="#fff" stroke-width="0.5"/><path d="M0,0L60,40M60,0L0,40" stroke="#FFF" stroke-width="6"/><path d="M0,0L60,40M60,0L0,40" stroke="#C8102E" stroke-width="3"/><path d="M0,20L60,20M30,0L30,40" stroke="#FFF" stroke-width="6"/><path d="M0,20L60,20M30,0L30,40" stroke="#C8102E" stroke-width="3"/></svg>';
const esFlag = '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 60 40" width="20" height="14"><rect width="60" height="40" fill="#C60B1E" stroke="#fff" stroke-width="0.5"/><rect y="10" width="60" height="20" fill="#FFC400"/></svg>';

const locales = [
  { code: 'en', flag: ukFlag, labelKey: 'nav.localeEn' },
  { code: 'es', flag: esFlag, labelKey: 'nav.localeEs' },
];

interface HeaderProps {
  name: string;
  title: string;
}

export function Header({ name, title }: HeaderProps) {
  const { t, locale, setLocale } = useTranslation();
  const { toggleTheme } = useTheme();
  const active = locales.find(l => l.code === locale)!;

  return (
    <nav className="navbar">
      <div className="logo">
        <span className="logo-name">{renderFormattedText(name)}</span>
        <span className="logo-title">{renderFormattedText(title)}</span>
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
            dangerouslySetInnerHTML={{ __html: active.flag + '<span class="locale-caret" aria-hidden="true">▾</span>' }}
          />
          <ul className="locale-menu">
            {locales.map(l => (
              <li key={l.code}>
                <button
                  className="locale-option"
                  data-locale={l.code}
                  type="button"
                  aria-label={t(l.labelKey)}
                  title={t(l.labelKey)}
                  onClick={() => setLocale(l.code as 'en' | 'es')}
                  dangerouslySetInnerHTML={{ __html: l.flag + '\u00A0' + t(l.labelKey) }}
                />
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
    </nav>
  );
}
