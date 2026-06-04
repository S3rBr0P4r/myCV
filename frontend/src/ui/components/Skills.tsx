import { useState, useCallback } from 'react';
import type { SkillCategory } from '../../domain/entities/CV';
import { useTranslation } from '../hooks/useTranslation';
import { renderFormattedText } from '../format';

const PER_PAGE = 2;

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
  const [page, setPage] = useState(1);
  const totalPages = Math.ceil(skillCategories.length / PER_PAGE);
  const start = (page - 1) * PER_PAGE;
  const pageItems = skillCategories.slice(start, start + PER_PAGE);

  const goToPage = useCallback((p: number) => {
    setPage(p);
    requestAnimationFrame(() => {
      document.querySelectorAll('.skill-category.stagger-item').forEach(el => {
        el.classList.remove('active');
        void (el as HTMLElement).offsetWidth;
        el.classList.add('active');
      });
    });
  }, []);

  if (skillCategories.length === 0) return null;

  return (
    <section id="skills" className="reveal">
      <h2 className="section-title">{t('skills.title')}</h2>
      <div className="skills-grid">
        {pageItems.map((cat, i) => (
          <SkillCat key={i} category={cat} />
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
