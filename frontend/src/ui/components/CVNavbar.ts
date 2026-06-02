import { t, getLocale } from '../../core/TranslationService';

const sunSvg = '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="5"/><path d="M12 1v2M12 21v2M4.22 4.22l1.42 1.42M18.36 18.36l1.42 1.42M1 12h2M21 12h2M4.22 19.78l1.42-1.42M18.36 5.64l1.42-1.42"/></svg>';
const moonSvg = '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><path d="M21 12.79A9 9 0 1 1 11.21 3 7 7 0 0 0 21 12.79z"/></svg>';

const ukFlag = '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 60 40" width="20" height="14"><rect width="60" height="40" fill="#012169" stroke="#fff" stroke-width="0.5"/><path d="M0,0L60,40M60,0L0,40" stroke="#FFF" stroke-width="6"/><path d="M0,0L60,40M60,0L0,40" stroke="#C8102E" stroke-width="3"/><path d="M0,20L60,20M30,0L30,40" stroke="#FFF" stroke-width="6"/><path d="M0,20L60,20M30,0L30,40" stroke="#C8102E" stroke-width="3"/></svg>';
const esFlag = '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 60 40" width="20" height="14"><rect width="60" height="40" fill="#C60B1E" stroke="#fff" stroke-width="0.5"/><rect y="10" width="60" height="20" fill="#FFC400"/></svg>';

const locales = [
  { code: 'en', flag: ukFlag, labelKey: 'nav.localeEn' },
  { code: 'es', flag: esFlag, labelKey: 'nav.localeEs' },
];

export function renderNavbar(): HTMLElement {
  const current = getLocale();
  const active = locales.find(l => l.code === current)!;

  const nav = document.createElement('nav');
  nav.className = 'navbar';

  const logo = document.createElement('div');
  logo.className = 'logo';
  logo.appendChild(document.createTextNode(t('nav.logo')));
  const logoSuffix = document.createElement('span');
  logoSuffix.appendChild(document.createTextNode(t('nav.logoSuffix')));
  logo.appendChild(logoSuffix);
  nav.appendChild(logo);

  const actions = document.createElement('div');
  actions.className = 'navbar-actions';

  const dropdown = document.createElement('div');
  dropdown.className = 'locale-dropdown';

  const trigger = document.createElement('button');
  trigger.className = 'locale-trigger';
  trigger.id = 'localeTrigger';
  trigger.type = 'button';
  trigger.ariaLabel = t('nav.localeLabel');
  trigger.title = t('nav.localeLabel');
  trigger.innerHTML = active.flag;
  const caret = document.createElement('span');
  caret.className = 'locale-caret';
  caret.ariaHidden = 'true';
  caret.appendChild(document.createTextNode('▾'));
  trigger.appendChild(caret);
  dropdown.appendChild(trigger);

  const menu = document.createElement('ul');
  menu.className = 'locale-menu';
  menu.id = 'localeMenu';
  for (const l of locales) {
    const li = document.createElement('li');
    const btn = document.createElement('button');
    btn.className = 'locale-option';
    btn.dataset.locale = l.code;
    btn.type = 'button';
    btn.ariaLabel = t(l.labelKey);
    btn.title = t(l.labelKey);
    btn.innerHTML = l.flag;
    btn.appendChild(document.createTextNode('\u00A0' + t(l.labelKey)));
    li.appendChild(btn);
    menu.appendChild(li);
  }
  dropdown.appendChild(menu);
  actions.appendChild(dropdown);

  const themeBtn = document.createElement('button');
  themeBtn.className = 'theme-toggle';
  themeBtn.id = 'themeToggle';
  themeBtn.type = 'button';
  themeBtn.ariaLabel = t('nav.themeLabel');
  themeBtn.title = t('nav.themeLabel');
  themeBtn.innerHTML = sunSvg + moonSvg;
  actions.appendChild(themeBtn);

  nav.appendChild(actions);
  return nav;
}
