import { CV } from '../../domain/entities/CV';
import { t } from '../../core/TranslationService';
import { renderFormattedText } from '../format';

function appendFormatted(parent: HTMLElement, text: string): void {
  for (const node of renderFormattedText(text)) {
    parent.appendChild(node);
  }
}

export function renderContact(cv: CV): HTMLElement {
  const section = document.createElement('section');
  section.id = 'contact';
  section.className = 'reveal';
  section.style.textAlign = 'center';

  const card = document.createElement('div');
  card.className = 'contact-card';

  const heading = document.createElement('h2');
  heading.appendChild(document.createTextNode(t('contact.heading')));
  card.appendChild(heading);

  if (cv.contactInfo) {
    const info = cv.contactInfo;

    if (info.email) {
      const emailLink = document.createElement('a');
      emailLink.className = 'email-link';
      emailLink.href = `mailto:${info.email}`;
      emailLink.appendChild(document.createTextNode(info.email));
      card.appendChild(emailLink);
    }

    if (info.phone) {
      const p = document.createElement('p');
      p.className = 'contact-detail';
      p.appendChild(document.createTextNode(`${t('contact.phone')}: `));
      appendFormatted(p, info.phone);
      card.appendChild(p);
    }

    if (info.location) {
      const p = document.createElement('p');
      p.className = 'contact-detail';
      p.appendChild(document.createTextNode(`${t('contact.location')}: `));
      appendFormatted(p, info.location);
      card.appendChild(p);
    }

    if (info.willingnessToTravel) {
      const p = document.createElement('p');
      p.className = 'contact-detail';
      p.appendChild(document.createTextNode(`${t('contact.travel')}: `));
      appendFormatted(p, info.willingnessToTravel);
      card.appendChild(p);
    }
  }

  section.appendChild(card);
  return section;
}

export function renderFooter(cvName: string): HTMLElement {
  const footer = document.createElement('footer');

  const p = document.createElement('p');
  for (const node of renderFormattedText(
    t('footer.copyright', { year: 2026, name: cvName }),
  )) {
    p.appendChild(node);
  }
  footer.appendChild(p);

  return footer;
}
