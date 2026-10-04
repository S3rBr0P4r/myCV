import type { SkillCategory } from '../../domain/entities/CV';
import { useTranslation } from '../hooks/useTranslation';
import { renderFormattedText } from '../format';

const languageFlagMap: Record<string, string> = {
  english: 'gb',
  german: 'de',
  spanish: 'es',
  catalan: 'es-ct',
};

function LanguageFlag({ language }: { language: string }) {
  const code = languageFlagMap[language.toLowerCase()];
  if (!code) return null;
  return (
    <img
      src={`/flags/${code}.svg`}
      alt=""
      width="16"
      height="16"
      className="language-flag"
      loading="lazy"
      data-testid="language-flag"
    />
  );
}

function parseLanguageItem(item: string): { language: string; proficiency: string } | null {
  const colonIdx = item.indexOf(': ');
  if (colonIdx <= 0) return null;
  const language = item.substring(0, colonIdx).trim();
  if (!languageFlagMap[language.toLowerCase()]) return null;
  return { language, proficiency: item.substring(colonIdx + 2).trim() };
}

function SkillItem({ item }: { item: string }) {
  const parsed = parseLanguageItem(item);
  if (parsed) {
    return (
      <li className="skill-item language-item">
        <LanguageFlag language={parsed.language} />
        {renderFormattedText(`${parsed.language}: ${parsed.proficiency}`)}
      </li>
    );
  }
  return <li className="skill-item">{renderFormattedText(item)}</li>;
}

function SkillCat({ category }: { category: SkillCategory }) {
  return (
    <div className="skill-category stagger-item">
      <h3 className="skill-category-title">{renderFormattedText(category.name)}</h3>
      {category.subCategories.map((sub, i) => (
        <div key={i} className="skill-subcategory">
          {sub.name.toLowerCase() !== 'general' && (
            <h4 className="skill-subcategory-title">{renderFormattedText(sub.name)}</h4>
          )}
          <ul className="skill-items">
            {sub.items.map((item, j) => (
              <SkillItem key={j} item={item} />
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
