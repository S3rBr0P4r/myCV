import { CV } from '../../domain/entities/CV';
import { t } from '../../core/TranslationService';

export function renderExperience(cv: CV): HTMLElement {
  const section = document.createElement('section');
  section.id = 'experience';
  section.className = 'reveal';

  const title = document.createElement('h2');
  title.className = 'section-title';
  title.appendChild(document.createTextNode(t('experience.title')));
  section.appendChild(title);

  const timeline = document.createElement('div');
  timeline.className = 'timeline';

  for (let i = 0; i < cv.experiences.length; i++) {
    const exp = cv.experiences[i];

    const card = document.createElement('div');
    card.className = `experience-card stagger-item ${exp.background || `bg-placeholder-${(i % 4) + 1}`}`;

    const bgLayer = document.createElement('div');
    bgLayer.className = 'exp-bg-layer';
    card.appendChild(bgLayer);

    const expContent = document.createElement('div');
    expContent.className = 'exp-content';

    const date = document.createElement('span');
    date.className = 'date';
    date.appendChild(document.createTextNode(t(exp.period)));
    expContent.appendChild(date);

    const role = document.createElement('h3');
    role.appendChild(document.createTextNode(t(exp.role)));
    expContent.appendChild(role);

    const company = document.createElement('p');
    company.style.color = 'var(--primary)';
    company.style.fontWeight = '700';
    company.appendChild(document.createTextNode(exp.company));
    expContent.appendChild(company);

    const desc = document.createElement('p');
    desc.appendChild(document.createTextNode(t(exp.description)));
    expContent.appendChild(desc);

    card.appendChild(expContent);
    timeline.appendChild(card);
  }

  section.appendChild(timeline);
  return section;
}
