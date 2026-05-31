import { CV } from '../../domain/entities/CV';
import { t } from '../../core/TranslationService';

export function renderSkills(cv: CV): string {
  return `
    <section id="skills" class="reveal">
      <h2 class="section-title">${t('skills.title')}</h2>
      <div class="skills-grid">
        ${cv.skills.map(skill => `
          <div class="skill-item stagger-item">${skill}</div>
        `).join('')}
      </div>
    </section>
  `;
}
