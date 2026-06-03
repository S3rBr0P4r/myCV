import { CV, SkillCategory } from '../../domain/entities/CV';
import { t } from '../../core/TranslationService';
import { renderFormattedText } from '../format';

function appendFormatted(parent: HTMLElement, text: string): void {
  for (const node of renderFormattedText(text)) {
    parent.appendChild(node);
  }
}

function renderCategory(category: SkillCategory): HTMLElement {
  const container = document.createElement('div');
  container.className = 'skill-category';

  const catTitle = document.createElement('h3');
  catTitle.className = 'skill-category-title';
  appendFormatted(catTitle, category.name);
  container.appendChild(catTitle);

  for (const sub of category.subCategories) {
    const subContainer = document.createElement('div');
    subContainer.className = 'skill-subcategory';

    const subTitle = document.createElement('h4');
    subTitle.className = 'skill-subcategory-title';
    appendFormatted(subTitle, sub.name);
    subContainer.appendChild(subTitle);

    const list = document.createElement('ul');
    list.className = 'skill-items';

    for (const item of sub.items) {
      const li = document.createElement('li');
      li.className = 'skill-item stagger-item';
      appendFormatted(li, item);
      list.appendChild(li);
    }

    subContainer.appendChild(list);
    container.appendChild(subContainer);
  }

  return container;
}

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

  for (const category of cv.skillCategories) {
    grid.appendChild(renderCategory(category));
  }

  section.appendChild(grid);
  return section;
}
