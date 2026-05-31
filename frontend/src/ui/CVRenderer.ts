import { CV } from '../domain/entities/CV';
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

    const sections = [
      { id: 'hero', label: t('nav.dotHero') },
      { id: 'experience', label: t('nav.dotExperience') },
      { id: 'skills', label: t('nav.dotSkills') },
      { id: 'contact', label: t('nav.dotContact') },
    ];

    appRoot.innerHTML = `
      ${renderNavbar()}

      <div class="painted-bg" id="bg"></div>

      ${renderHero(cv)}
      ${renderExperience(cv)}
      ${renderSkills(cv)}
      ${renderContact()}

      ${renderFooter(cv.name, cv.lastName)}

      <nav class="scroll-progress" aria-label="${t('nav.dotAria')}">
        ${sections.map(s => `
          <a href="#${s.id}" aria-label="${s.label}" data-section="${s.id}"></a>
        `).join('')}
      </nav>
    `;
  }
}
