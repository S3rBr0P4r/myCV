import { t, getLocale } from '../../core/TranslationService';

const ukFlag = '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 60 40" width="18" height="12"><rect width="60" height="40" fill="#012169" stroke="#fff" stroke-width="0.5"/><path d="M0,0L60,40M60,0L0,40" stroke="#FFF" stroke-width="6"/><path d="M0,0L60,40M60,0L0,40" stroke="#C8102E" stroke-width="3"/><path d="M0,20L60,20M30,0L30,40" stroke="#FFF" stroke-width="6"/><path d="M0,20L60,20M30,0L30,40" stroke="#C8102E" stroke-width="3"/></svg>';
const esFlag = '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 60 40" width="18" height="12"><rect width="60" height="40" fill="#C60B1E" stroke="#fff" stroke-width="0.5"/><rect y="10" width="60" height="20" fill="#FFC400"/></svg>';

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
  const sun = document.createElement('span');
  sun.className = 'icon-sun';
  sun.ariaHidden = 'true';
  sun.appendChild(document.createTextNode('☀️'));
  const moon = document.createElement('span');
  moon.className = 'icon-moon';
  moon.ariaHidden = 'true';
  moon.appendChild(document.createTextNode('🌙'));
  themeBtn.appendChild(sun);
  themeBtn.appendChild(moon);
  actions.appendChild(themeBtn);

  nav.appendChild(actions);
  return nav;
}
