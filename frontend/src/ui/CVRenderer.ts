import { type CV } from '../domain/entities/CV';
import { t } from '../core/TranslationService';
import { renderNavbar } from './components/CVNavbar';
import { renderHero } from './components/CVHero';
import { renderExperience } from './components/CVExperience';
import { renderSkills } from './components/CVSkills';
import { renderContact, renderFooter } from './components/CVContact';

export class CVRenderer {
  public static render(cv: CV): void {
    const appRoot = document.getElementById('app');
    if (!appRoot) return;

    appRoot.textContent = '';

    const sections = [
      { id: 'hero', label: t('nav.dotHero') },
      { id: 'experience', label: t('nav.dotExperience') },
      { id: 'skills', label: t('nav.dotSkills') },
      { id: 'contact', label: t('nav.dotContact') },
    ];

    appRoot.appendChild(renderNavbar());

    const bg = document.createElement('div');
    bg.className = 'painted-bg';
    bg.id = 'bg';
    appRoot.appendChild(bg);

    appRoot.appendChild(renderHero(cv));
    appRoot.appendChild(renderExperience(cv));
    appRoot.appendChild(renderSkills(cv));
    appRoot.appendChild(renderContact());
    appRoot.appendChild(renderFooter(cv.name, cv.lastName));

    const progressNav = document.createElement('nav');
    progressNav.className = 'scroll-progress';
    progressNav.ariaLabel = t('nav.dotAria');
    for (const s of sections) {
      const link = document.createElement('a');
      link.href = `#${s.id}`;
      link.ariaLabel = s.label;
      link.dataset.section = s.id;
      progressNav.appendChild(link);
    }
    appRoot.appendChild(progressNav);
  }
}
