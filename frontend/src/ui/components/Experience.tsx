import { useState, useCallback, useEffect, useRef } from 'react';
import type { Experience } from '../../domain/entities/CV';
import { useTranslation } from '../hooks/useTranslation';
import { renderFormattedText } from '../format';
import { getCompanyUrl, loadCompanyImage, loadCompanyLogo } from '../CompanyData';

const PER_PAGE = 2;

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
  const initials = words.slice(0, 2).map(w => w[0]).join('').toUpperCase();
  let hash = 0;
  for (let i = 0; i < company.length; i++) {
    hash = ((hash << 5) - hash) + company.charCodeAt(i);
  }
  const hue = Math.abs(hash % 360);
  const bg = `hsl(${hue}, 35%, 55%)`;
  return `data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='44' height='44' viewBox='0 0 44 44'%3E%3Crect width='44' height='44' rx='8' fill='${encodeURIComponent(bg)}'/%3E%3Ctext x='22' y='22' dominant-baseline='central' text-anchor='middle' font-family='Figtree,sans-serif' font-size='16' font-weight='700' fill='white'%3E${initials}%3C/text%3E%3C/svg%3E`;
}

function ExpCard({ exp }: { exp: Experience }) {
  const bgRef = useRef<HTMLDivElement>(null);
  const logoRef = useRef<HTMLImageElement>(null);
  const effectiveUrl = exp.companyUrl || getCompanyUrl(exp.company) || '';

  useEffect(() => {
    const bg = bgRef.current;
    if (!bg) return;
    loadCompanyImage(exp.company).then(url => {
      if (url && bg) {
        bg.style.background = '';
        bg.style.backgroundImage = `url(${url})`;
        bg.style.backgroundSize = 'cover';
        bg.style.backgroundPosition = 'center';
      }
    });
  }, [exp.company]);

  useEffect(() => {
    const img = logoRef.current;
    if (!img) return;
    loadCompanyLogo(exp.company).then(url => {
      if (!url) return;
      const preload = new Image();
      preload.onload = () => {
        if (preload.naturalWidth > 1 && img) {
          img.src = url;
        }
      };
      preload.src = url;
    });
  }, [exp.company]);

  const bullets = exp.description.split('\n').filter(b => b.trim().length > 0);

  return (
    <div className="experience-card stagger-item">
      <div ref={bgRef} className="exp-bg-layer" style={{ background: companyFallbackBg(exp.company) }} />
      <div className="exp-content">
        <div className="exp-header">
          <img
            ref={logoRef}
            className="exp-company-logo"
            alt={`${exp.company} logo`}
            src={initialsLogo(exp.company)}
            loading="lazy"
            decoding="async"
          />
          <div className="exp-company-info">
            {effectiveUrl ? (
              <a className="exp-company-name" href={effectiveUrl} target="_blank" rel="noopener noreferrer">
                {exp.company}
              </a>
            ) : (
              <span className="exp-company-name">{exp.company}</span>
            )}
            <div className="exp-meta">
              {exp.location && (
                <a
                  className="exp-location"
                  href={`https://www.google.com/maps/search/?api=1&query=${encodeURIComponent(exp.location)}`}
                  target="_blank"
                  rel="noopener noreferrer"
                >
                  {exp.location}
                </a>
              )}
              {exp.workMode && (
                <span className={`exp-workmode exp-workmode--${exp.workMode.toLowerCase()}`}>
                  {exp.workMode}
                </span>
              )}
            </div>
          </div>
        </div>
        <span className="date">{exp.period}</span>
        <h3>{exp.role}</h3>
        {bullets.length > 0 && (
          <ul className="exp-description">
            {bullets.map((b, i) => (
              <li key={i}>{renderFormattedText(b)}</li>
            ))}
          </ul>
        )}
      </div>
    </div>
  );
}

interface ExperienceProps {
  experiences: Experience[];
}

export function Experience({ experiences }: ExperienceProps) {
  const { t } = useTranslation();
  const [page, setPage] = useState(1);
  const totalPages = Math.ceil(experiences.length / PER_PAGE);
  const start = (page - 1) * PER_PAGE;
  const pageItems = experiences.slice(start, start + PER_PAGE);

  const goToPage = useCallback((p: number) => {
    setPage(p);
    requestAnimationFrame(() => {
      document.querySelectorAll('.experience-card.stagger-item').forEach(el => {
        el.classList.remove('active');
        void (el as HTMLElement).offsetWidth;
        el.classList.add('active');
      });
    });
  }, []);

  return (
    <section id="experience" className="reveal">
      <h2 className="section-title">{t('experience.title')}</h2>
      <div className="timeline">
        {pageItems.map((exp, i) => (
          <ExpCard key={`${exp.company}-${exp.period}-${i}`} exp={exp} />
        ))}
      </div>
      {totalPages > 1 && (
        <div className="exp-pagination">
          <button
            className="exp-page-btn"
            disabled={page <= 1}
            onClick={() => goToPage(page - 1)}
          >
            ← Previous
          </button>
          <span className="exp-page-indicator">{page} / {totalPages}</span>
          <button
            className="exp-page-btn"
            disabled={page >= totalPages}
            onClick={() => goToPage(page + 1)}
          >
            Next →
          </button>
        </div>
      )}
    </section>
  );
}
