import { useState, useRef, useEffect, useCallback } from 'react';
import type { SkillCategory } from '../../domain/entities/CV';
import { useTranslation } from '../hooks/useTranslation';
import { renderFormattedText } from '../format';

const PER_PAGE = 1;

function SkillCat({ category }: { category: SkillCategory }) {
  return (
    <div className="skill-category stagger-item">
      <h3 className="skill-category-title">{renderFormattedText(category.name)}</h3>
      {category.subCategories.map((sub, i) => (
        <div key={i} className="skill-subcategory">
          <h4 className="skill-subcategory-title">{renderFormattedText(sub.name)}</h4>
          <ul className="skill-items">
            {sub.items.map((item, j) => (
              <li key={j} className="skill-item">
                {renderFormattedText(item)}
              </li>
            ))}
          </ul>
        </div>
      ))}
    </div>
  );
}

interface SkillsProps {
  skillCategories: SkillCategory[];
}

export function Skills({ skillCategories }: SkillsProps) {
  const { t } = useTranslation();
  const trackRef = useRef<HTMLDivElement>(null);
  const [currentPage, setCurrentPage] = useState(0);
  const totalPages = Math.ceil(skillCategories.length / PER_PAGE);

  useEffect(() => {
    setCurrentPage(0);
    if (trackRef.current) {
      trackRef.current.scrollLeft = 0;
    }
  }, [skillCategories.length]);

  const handleScroll = useCallback(() => {
    const el = trackRef.current;
    if (!el) return;
    const page = Math.round(el.scrollLeft / el.clientWidth);
    setCurrentPage(page);
  }, []);

  const goNext = () => {
    trackRef.current?.scrollBy({ left: trackRef.current.clientWidth, behavior: 'smooth' });
  };

  const goPrev = () => {
    trackRef.current?.scrollBy({ left: -trackRef.current.clientWidth, behavior: 'smooth' });
  };

  if (skillCategories.length === 0) return null;

  const pages: SkillCategory[][] = [];
  for (let i = 0; i < skillCategories.length; i += PER_PAGE) {
    pages.push(skillCategories.slice(i, i + PER_PAGE));
  }

  return (
    <section id="skills" className="reveal">
      <h2 className="section-title">{t('skills.title')}</h2>
      <div className="exp-carousel">
        <div className="exp-carousel-track" ref={trackRef} onScroll={handleScroll}>
          {pages.map((pageItems, pi) => (
            <div className="exp-carousel-page" key={pi}>
              {pageItems.map((cat, i) => (
                <SkillCat key={i} category={cat} />
              ))}
            </div>
          ))}
        </div>
        {totalPages > 1 && (
          <>
            <div className="exp-carousel-overlay">
              <button
                className="exp-carousel-overlay-btn"
                onClick={goPrev}
                disabled={currentPage === 0}
                aria-label={t('exp.prev')}
              >
                <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round">
                  <polyline points="15 18 9 12 15 6" />
                </svg>
              </button>
              <button
                className="exp-carousel-overlay-btn"
                onClick={goNext}
                disabled={currentPage >= totalPages - 1}
                aria-label={t('exp.next')}
              >
                <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round">
                  <polyline points="9 18 15 12 9 6" />
                </svg>
              </button>
            </div>
            <div className="exp-carousel-controls">
              <button
                className="exp-carousel-btn"
                onClick={goPrev}
                disabled={currentPage === 0}
                aria-label={t('exp.prev')}
              >
                <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round">
                  <polyline points="15 18 9 12 15 6" />
                </svg>
              </button>
              <div className="exp-carousel-dots">
                {Array.from({ length: totalPages }, (_, i) => (
                  <span
                    key={i}
                    className={`exp-carousel-dot${i === currentPage ? ' active' : ''}`}
                    aria-hidden="true"
                  />
                ))}
              </div>
              <button
                className="exp-carousel-btn"
                onClick={goNext}
                disabled={currentPage >= totalPages - 1}
                aria-label={t('exp.next')}
              >
                <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round">
                  <polyline points="9 18 15 12 9 6" />
                </svg>
              </button>
            </div>
          </>
        )}
      </div>
    </section>
  );
}
