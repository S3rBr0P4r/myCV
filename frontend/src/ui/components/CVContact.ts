import { t } from '../../core/TranslationService';

export function renderContact(): HTMLElement {
  const section = document.createElement('section');
  section.id = 'contact';
  section.className = 'reveal';
  section.style.textAlign = 'center';

  const card = document.createElement('div');
  card.className = 'contact-card';

  const heading = document.createElement('h2');
  heading.appendChild(document.createTextNode(t('contact.heading')));
  card.appendChild(heading);

  const link = document.createElement('a');
  link.className = 'email-link';
  link.href = 'mailto:hello@example.com';
  link.appendChild(document.createTextNode('hello@example.com'));
  card.appendChild(link);

  section.appendChild(card);
  return section;
}

export function renderFooter(cvName: string, cvLastName: string): HTMLElement {
  const footer = document.createElement('footer');

  const p = document.createElement('p');
  p.appendChild(document.createTextNode(
    t('footer.copyright', { year: 2026, name: cvName, lastName: cvLastName }),
  ));
  footer.appendChild(p);

  return footer;
}
