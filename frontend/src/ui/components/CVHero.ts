import { CV } from '../../domain/entities/CV';
import { t } from '../../core/TranslationService';

export function renderHero(cv: CV): HTMLElement {
  const section = document.createElement('section');
  section.id = 'hero';
  section.className = 'hero';

  const content = document.createElement('div');
  content.className = 'hero-content';

  const subtitle = document.createElement('h3');
  subtitle.className = 'sub-title';
  subtitle.appendChild(document.createTextNode(t('hero.subtitle')));
  content.appendChild(subtitle);

  const title = document.createElement('h1');
  title.className = 'main-title';
  title.appendChild(document.createTextNode(cv.name + ' '));

  const gradient = document.createElement('span');
  gradient.className = 'gradient-text';
  gradient.appendChild(document.createTextNode(cv.lastName));
  title.appendChild(gradient);
  content.appendChild(title);

  const description = document.createElement('p');
  description.className = 'hero-description';
  description.appendChild(document.createTextNode(t(cv.summary)));
  content.appendChild(description);

  const cta = document.createElement('div');
  cta.className = 'cta-buttons';

  const btn1 = document.createElement('a');
  btn1.className = 'btn primary';
  btn1.href = '#experience';
  btn1.appendChild(document.createTextNode(t('hero.ctaJourney')));
  cta.appendChild(btn1);

  const btn2 = document.createElement('a');
  btn2.className = 'btn secondary';
  btn2.href = '#contact';
  btn2.appendChild(document.createTextNode(t('hero.ctaContact')));
  cta.appendChild(btn2);

  content.appendChild(cta);
  section.appendChild(content);
  return section;
}
