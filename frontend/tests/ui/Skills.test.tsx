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

  it('returns null for empty categories', () => {
    const { container } = render(<Skills skillCategories={[]} />);
    expect(container.innerHTML).toBe('');
  });
});
