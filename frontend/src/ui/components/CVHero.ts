import { CV } from '../../domain/entities/CV';
import { t } from '../../core/TranslationService';

export function renderHero(cv: CV): string {
  return `
    <section id="hero" class="hero">
      <div class="hero-content">
        <h3 class="sub-title">${t('hero.subtitle')}</h3>
        <h1 class="main-title">${cv.name} <span class="gradient-text">${cv.lastName}</span></h1>
        <p class="hero-description">${t(cv.summary)}</p>
        <div class="cta-buttons">
          <a href="#experience" class="btn primary">${t('hero.ctaJourney')}</a>
          <a href="#contact" class="btn secondary">${t('hero.ctaContact')}</a>
        </div>
      </div>
    </section>
  `;
}
