import type { SkillCategory } from '../../domain/entities/CV';
import { useTranslation } from '../hooks/useTranslation';
import { renderFormattedText } from '../format';

function SkillCat({ category }: { category: SkillCategory }) {
  return (
    <div className="skill-category">
      <h3 className="skill-category-title">{renderFormattedText(category.name)}</h3>
      {category.subCategories.map((sub, i) => (
        <div key={i} className="skill-subcategory">
          <h4 className="skill-subcategory-title">{renderFormattedText(sub.name)}</h4>
          <ul className="skill-items">
            {sub.items.map((item, j) => (
              <li key={j} className="skill-item stagger-item">
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

  if (skillCategories.length === 0) return null;

  return (
    <section id="skills" className="reveal">
      <h2 className="section-title">{t('skills.title')}</h2>
      <div className="skills-grid">
        {skillCategories.map((cat, i) => (
          <SkillCat key={i} category={cat} />
        ))}
      </div>
    </section>
  );
}
