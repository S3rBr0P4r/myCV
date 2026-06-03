import { CV, Education, Certification } from '../../domain/entities/CV';
import { t } from '../../core/TranslationService';
import { renderFormattedText } from '../format';

function appendFormatted(parent: HTMLElement, text: string): void {
  for (const node of renderFormattedText(text)) {
    parent.appendChild(node);
  }
}

function renderEducationItem(edu: Education): HTMLElement {
  const card = document.createElement('div');
  card.className = 'edu-card stagger-item';

  const degree = document.createElement('h3');
  degree.className = 'edu-degree';
  appendFormatted(degree, edu.degree);
  card.appendChild(degree);

  const institution = document.createElement('p');
  institution.className = 'edu-institution';
  appendFormatted(institution, edu.institution);
  card.appendChild(institution);

  if (edu.notes) {
    const notes = document.createElement('p');
    notes.className = 'edu-notes';
    appendFormatted(notes, edu.notes);
    card.appendChild(notes);
  }

  return card;
}

function renderCertificationItem(cert: Certification): HTMLElement {
  const li = document.createElement('li');
  li.className = 'cert-item stagger-item';

  const title = document.createElement('span');
  title.className = 'cert-title';
  appendFormatted(title, cert.title);
  li.appendChild(title);

  const issuer = document.createElement('span');
  issuer.className = 'cert-issuer';
  appendFormatted(issuer, cert.issuer);
  li.appendChild(issuer);

  return li;
}

export function renderEducation(cv: CV): HTMLElement {
  const section = document.createElement('section');
  section.id = 'education';
  section.className = 'reveal';

  if (cv.education.length > 0) {
    const title = document.createElement('h2');
    title.className = 'section-title';
    title.appendChild(document.createTextNode(t('education.title')));
    section.appendChild(title);

    const grid = document.createElement('div');
    grid.className = 'education-grid';

    for (const edu of cv.education) {
      grid.appendChild(renderEducationItem(edu));
    }

    section.appendChild(grid);
  }

  if (cv.certifications.length > 0) {
    const certTitle = document.createElement('h2');
    certTitle.className = 'section-title';
    certTitle.style.marginTop = '2rem';
    certTitle.appendChild(document.createTextNode(t('education.certifications')));
    section.appendChild(certTitle);

    const list = document.createElement('ul');
    list.className = 'cert-list';

    for (const cert of cv.certifications) {
      list.appendChild(renderCertificationItem(cert));
    }

    section.appendChild(list);
  }

  return section;
}
