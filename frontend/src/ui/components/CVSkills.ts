import { CV } from '../../domain/entities/CV';
import { t } from '../../core/TranslationService';

export function renderSkills(cv: CV): HTMLElement {
  const section = document.createElement('section');
  section.id = 'skills';
  section.className = 'reveal';

  const title = document.createElement('h2');
  title.className = 'section-title';
  title.appendChild(document.createTextNode(t('skills.title')));
  section.appendChild(title);

  const grid = document.createElement('div');
  grid.className = 'skills-grid';

  for (const skill of cv.skills) {
    const item = document.createElement('div');
    item.className = 'skill-item stagger-item';
    item.appendChild(document.createTextNode(skill));
    grid.appendChild(item);
  }

  section.appendChild(grid);
  return section;
}
