import { CV, Experience } from '../../domain/entities/CV';
import { t } from '../../core/TranslationService';
import { renderFormattedText } from '../format';
import { loadCompanyImage } from '../CompanyImage';
import { getCompanyUrl } from '../CompanyUrl';
import { loadCompanyLogo } from '../CompanyLogoFile';

const PER_PAGE = 3;

function companyFallbackBg(company: string): string {
  let hash = 0;
  for (let i = 0; i < company.length; i++) {
    hash = ((hash << 5) - hash) + company.charCodeAt(i);
  }
  const hue = Math.abs(hash % 50) + 105;
  return [
    `radial-gradient(ellipse at 25% 30%, hsla(${hue}, 45%, 60%, 0.15) 0%, transparent 50%)`,
    `radial-gradient(ellipse at 70% 60%, hsla(${hue + 30}, 40%, 55%, 0.10) 0%, transparent 40%)`,
    `radial-gradient(ellipse at 50% 85%, hsla(${hue + 60}, 35%, 50%, 0.08) 0%, transparent 30%)`,
  ].join(', ');
}

function initialsLogo(company: string): string {
  const words = company.trim().split(/\s+/);
  const initials = words
    .slice(0, 2)
    .map(w => w[0])
    .join('')
    .toUpperCase();

  let hash = 0;
  for (let i = 0; i < company.length; i++) {
    hash = ((hash << 5) - hash) + company.charCodeAt(i);
  }
  const hue = Math.abs(hash % 360);
  const bg = `hsl(${hue}, 35%, 55%)`;

  return `data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='44' height='44' viewBox='0 0 44 44'%3E%3Crect width='44' height='44' rx='8' fill='${encodeURIComponent(bg)}'/%3E%3Ctext x='22' y='22' dominant-baseline='central' text-anchor='middle' font-family='Figtree,sans-serif' font-size='16' font-weight='700' fill='white'%3E${initials}%3C/text%3E%3C/svg%3E`;
}

function createCard(exp: Experience): HTMLElement {
  const card = document.createElement('div');
  card.className = 'experience-card stagger-item';

  const bgLayer = document.createElement('div');
  bgLayer.className = 'exp-bg-layer';
  bgLayer.style.background = companyFallbackBg(exp.company);
  card.appendChild(bgLayer);

  loadCompanyImage(exp.company).then(url => {
    if (url) {
      bgLayer.style.background = '';
      bgLayer.style.backgroundImage = `url(${url})`;
      bgLayer.style.backgroundSize = 'cover';
      bgLayer.style.backgroundPosition = 'center';
    }
  });

  const expContent = document.createElement('div');
  expContent.className = 'exp-content';

  const header = document.createElement('div');
  header.className = 'exp-header';

  const effectiveUrl = exp.companyUrl || getCompanyUrl(exp.company) || '';
  const logo = document.createElement('img');
  logo.className = 'exp-company-logo';
  logo.alt = `${exp.company} logo`;
  logo.src = initialsLogo(exp.company);

  loadCompanyLogo(exp.company).then(url => {
    if (url) {
      const preload = new Image();
      preload.onload = () => {
        if (preload.naturalWidth > 1) {
          logo.src = url;
        }
      };
      preload.src = url;
    }
  });

  header.appendChild(logo);

  const companyInfo = document.createElement('div');
  companyInfo.className = 'exp-company-info';

  if (effectiveUrl) {
    const link = document.createElement('a');
    link.className = 'exp-company-name';
    link.href = effectiveUrl;
    link.target = '_blank';
    link.rel = 'noopener noreferrer';
    link.textContent = exp.company;
    companyInfo.appendChild(link);
  } else {
    const name = document.createElement('span');
    name.className = 'exp-company-name';
    name.textContent = exp.company;
    companyInfo.appendChild(name);
  }

  const meta = document.createElement('div');
  meta.className = 'exp-meta';

  if (exp.location) {
    const loc = document.createElement('span');
    loc.className = 'exp-location';
    loc.textContent = exp.location;
    meta.appendChild(loc);
  }

  if (exp.workMode) {
    const badge = document.createElement('span');
    badge.className = `exp-workmode exp-workmode--${exp.workMode.toLowerCase()}`;
    badge.textContent = exp.workMode;
    meta.appendChild(badge);
  }

  companyInfo.appendChild(meta);
  header.appendChild(companyInfo);
  expContent.appendChild(header);

  const date = document.createElement('span');
  date.className = 'date';
  date.textContent = exp.period;
  expContent.appendChild(date);

  const role = document.createElement('h3');
  role.textContent = exp.role;
  expContent.appendChild(role);

  const descList = document.createElement('ul');
  descList.className = 'exp-description';

  const bullets = exp.description.split('\n').filter(b => b.trim().length > 0);
  for (const bullet of bullets) {
    const li = document.createElement('li');
    const fragments = renderFormattedText(bullet);
    for (const frag of fragments) {
      li.appendChild(frag);
    }
    descList.appendChild(li);
  }

  expContent.appendChild(descList);
  card.appendChild(expContent);
  return card;
}

function renderPage(
  container: HTMLElement,
  experiences: Experience[],
  page: number,
): void {
  const start = (page - 1) * PER_PAGE;
  const end = Math.min(start + PER_PAGE, experiences.length);
  const pageItems = experiences.slice(start, end);

  container.innerHTML = '';

  for (const exp of pageItems) {
    container.appendChild(createCard(exp));
  }

  setTimeout(() => {
    for (const item of container.querySelectorAll('.stagger-item')) {
      item.classList.remove('active');
      void (item as HTMLElement).offsetWidth;
      item.classList.add('active');
    }
  }, 50);
}

function createPagination(
  total: number,
  current: number,
  onPage: (page: number) => void,
): HTMLElement {
  const totalPages = Math.ceil(total / PER_PAGE);
  if (totalPages <= 1) return document.createElement('div');

  const wrapper = document.createElement('div');
  wrapper.className = 'exp-pagination';

  const prev = document.createElement('button');
  prev.className = 'exp-page-btn';
  prev.textContent = '← Previous';
  prev.disabled = current <= 1;
  prev.addEventListener('click', () => {
    if (current > 1) onPage(current - 1);
  });
  wrapper.appendChild(prev);

  const indicator = document.createElement('span');
  indicator.className = 'exp-page-indicator';
  indicator.textContent = `${current} / ${totalPages}`;
  wrapper.appendChild(indicator);

  const next = document.createElement('button');
  next.className = 'exp-page-btn';
  next.textContent = 'Next →';
  next.disabled = current >= totalPages;
  next.addEventListener('click', () => {
    if (current < totalPages) onPage(current + 1);
  });
  wrapper.appendChild(next);

  return wrapper;
}

export function renderExperience(cv: CV): HTMLElement {
  const section = document.createElement('section');
  section.id = 'experience';
  section.className = 'reveal';

  const title = document.createElement('h2');
  title.className = 'section-title';
  title.textContent = t('experience.title');
  section.appendChild(title);

  const timeline = document.createElement('div');
  timeline.className = 'timeline';

  let currentPage = 1;

  function goToPage(page: number) {
    currentPage = page;
    renderPage(timeline, cv.experiences, currentPage);

    const existingPagination = section.querySelector('.exp-pagination');
    if (existingPagination) {
      existingPagination.replaceWith(
        createPagination(cv.experiences.length, currentPage, goToPage),
      );
    }
  }

  renderPage(timeline, cv.experiences, currentPage);
  section.appendChild(timeline);

  const pagination = createPagination(
    cv.experiences.length,
    currentPage,
    goToPage,
  );
  if (pagination.children.length > 0) {
    section.appendChild(pagination);
  }

  return section;
}
