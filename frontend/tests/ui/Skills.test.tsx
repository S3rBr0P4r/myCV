import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { Skills } from '../../src/ui/components/Skills';
import type { SkillCategory } from '../../src/domain/entities/CV';

const categories: SkillCategory[] = [
  {
    name: 'Languages',
    subCategories: [
      { name: 'Proficient', items: ['C#', 'TypeScript'] },
    ],
  },
];

const generalCategories: SkillCategory[] = [
  {
    name: 'Agile',
    subCategories: [
      { name: 'General', items: ['Professional Scrum Master I | Scrum.org'] },
    ],
  },
];

const languageCategories: SkillCategory[] = [
  {
    name: 'Languages',
    subCategories: [
      {
        name: 'General',
        items: [
          'English: C1 - Advanced',
          'German: A2 - Elementary (Over 700h of study at Goethe-Institut)',
          'Spanish: Native',
          'Catalan: Native',
        ],
      },
    ],
  },
];

describe('Skills', () => {
  it('renders skills section', () => {
    render(<Skills skillCategories={categories} />);
    expect(screen.getByText('Languages')).toBeInTheDocument();
  });

  it('renders subcategories', () => {
    render(<Skills skillCategories={categories} />);
    expect(screen.getByText('Proficient')).toBeInTheDocument();
  });

  it('renders skill items', () => {
    render(<Skills skillCategories={categories} />);
    expect(screen.getByText('C#')).toBeInTheDocument();
    expect(screen.getByText('TypeScript')).toBeInTheDocument();
  });

  it('hides General subcategory title', () => {
    render(<Skills skillCategories={generalCategories} />);
    expect(screen.getByText('Agile')).toBeInTheDocument();
    expect(screen.queryByText('General')).not.toBeInTheDocument();
    expect(screen.getByText('Professional Scrum Master I | Scrum.org')).toBeInTheDocument();
  });

  it('renders language flags for language items', () => {
    render(<Skills skillCategories={languageCategories} />);
    const flags = screen.getAllByTestId('language-flag');
    expect(flags).toHaveLength(4);
    expect((flags[0] as HTMLImageElement).src).toContain('/flags/gb.svg');
    expect((flags[1] as HTMLImageElement).src).toContain('/flags/de.svg');
    expect((flags[2] as HTMLImageElement).src).toContain('/flags/es.svg');
    expect((flags[3] as HTMLImageElement).src).toContain('/flags/es-ct.svg');
  });

  it('renders language name and proficiency on one item', () => {
    render(<Skills skillCategories={languageCategories} />);
    expect(screen.getByText('English: C1 - Advanced')).toBeInTheDocument();
    expect(screen.getByText('Spanish: Native')).toBeInTheDocument();
    expect(screen.getByText('Catalan: Native')).toBeInTheDocument();
  });

  it('returns null for empty categories', () => {
    const { container } = render(<Skills skillCategories={[]} />);
    expect(container.innerHTML).toBe('');
  });
});
