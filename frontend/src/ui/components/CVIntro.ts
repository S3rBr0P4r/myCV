import { CV } from '../../domain/entities/CV';
import { renderFormattedText } from '../format';

export function renderIntro(cv: CV): HTMLElement {
  const section = document.createElement('section');
  section.id = 'intro';
  section.className = 'intro-section';

  const content = document.createElement('div');
  content.className = 'intro-content';

  const description = document.createElement('p');
  description.className = 'intro-description';
  for (const node of renderFormattedText(cv.summary)) {
    description.appendChild(node);
  }
  content.appendChild(description);

  section.appendChild(content);
  return section;
}
