import { CV } from '../../domain/entities/CV';
import { t } from '../../core/TranslationService';

export function renderExperience(cv: CV): string {
  return `
    <section id="experience" class="reveal">
      <h2 class="section-title">${t('experience.title')}</h2>
      <div class="timeline">
        ${cv.experiences.map((exp, i) => `
          <div class="experience-card stagger-item ${exp.background || `bg-placeholder-${(i % 4) + 1}`}">
            <div class="exp-bg-layer"></div>
            <div class="exp-content">
              <span class="date">${t(exp.period)}</span>
              <h3>${t(exp.role)}</h3>
              <p style="color: var(--primary); font-weight: 700;">${exp.company}</p>
              <p>${t(exp.description)}</p>
            </div>
          </div>
        `).join('')}
      </div>
    </section>
  `;
}
