import { type CV } from '../domain/entities/CV';
import { t } from '../core/TranslationService';
import { renderNavbar } from './components/CVNavbar';
import { renderIntro } from './components/CVIntro';
import { renderExperience } from './components/CVExperience';
import { renderSkills } from './components/CVSkills';
import { renderContact, renderFooter } from './components/CVContact';
import { renderEducation } from './components/CVEducation';

export class CVRenderer {
  public static render(cv: CV): void {
    const appRoot = document.getElementById('app');
    if (!appRoot) return;

    appRoot.textContent = '';

    const sections = [
      { id: 'intro', label: t('nav.dotIntro') },
      { id: 'experience', label: t('nav.dotExperience') },
    ];

    if (cv.skillCategories.length > 0) {
      sections.push({ id: 'skills', label: t('nav.dotSkills') });
    }

    if (cv.education.length > 0 || cv.certifications.length > 0) {
      sections.push({ id: 'education', label: 'Education' });
    }

    sections.push({ id: 'contact', label: t('nav.dotContact') });

    appRoot.appendChild(renderNavbar(cv));

    const bg = document.createElement('div');
    bg.className = 'painted-bg';
    bg.id = 'bg';
    appRoot.appendChild(bg);

    appRoot.appendChild(renderIntro(cv));
    appRoot.appendChild(renderExperience(cv));

    if (cv.skillCategories.length > 0) {
      appRoot.appendChild(renderSkills(cv));
    }

    if (cv.education.length > 0 || cv.certifications.length > 0) {
      appRoot.appendChild(renderEducation(cv));
    }

    appRoot.appendChild(renderContact(cv));
    appRoot.appendChild(renderFooter(cv.name));

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
