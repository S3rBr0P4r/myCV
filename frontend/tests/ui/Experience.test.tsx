import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { Experience } from '../../src/ui/components/Experience';
import { TranslationProvider } from '../../src/ui/contexts/TranslationContext';
import type { ReactNode } from 'react';
import type { Experience as ExpType } from '../../src/domain/entities/CV';

const oneExp: ExpType[] = [
  { period: '2024', role: 'Senior Dev', company: 'Acme', description: 'Did work' },
];

const twoExps: ExpType[] = [
  { period: '2024', role: 'Senior Dev', company: 'Acme', description: 'Did work' },
  { period: '2023', role: 'Junior Dev', company: 'Beta', description: 'Learned' },
];

const fiveExps: ExpType[] = [
  { period: '2024', role: 'Role A', company: 'Company A', description: 'First' },
  { period: '2023', role: 'Role B', company: 'Company B', description: 'Second' },
  { period: '2022', role: 'Role C', company: 'Company C', description: 'Third' },
  { period: '2021', role: 'Role D', company: 'Company D', description: 'Fourth' },
  { period: '2020', role: 'Role E', company: 'Company E', description: 'Fifth' },
];

function Wrapper({ children }: { children: ReactNode }) {
  return <TranslationProvider>{children}</TranslationProvider>;
}

function getPrevBtn() {
  return screen.getAllByLabelText('Previous')[0];
}

function getNextBtn() {
  return screen.getAllByLabelText('Next')[0];
}

describe('Experience', () => {
  it('renders section title', () => {
    render(<Experience experiences={oneExp} />, { wrapper: Wrapper });
    expect(screen.getByText('Career')).toBeInTheDocument();
  });

  it('renders experience cards', () => {
    render(<Experience experiences={oneExp} />, { wrapper: Wrapper });
    expect(screen.getByText('Senior Dev')).toBeInTheDocument();
  });

  it('shows company name', () => {
    render(<Experience experiences={oneExp} />, { wrapper: Wrapper });
    expect(screen.getByText('Acme')).toBeInTheDocument();
  });

  it('shows period', () => {
    render(<Experience experiences={oneExp} />, { wrapper: Wrapper });
    expect(screen.getByText('2024')).toBeInTheDocument();
  });

  it('shows description', () => {
    render(<Experience experiences={oneExp} />, { wrapper: Wrapper });
    expect(screen.getByText('Did work')).toBeInTheDocument();
  });

  it('shows carousel controls for > 1 item', () => {
    render(<Experience experiences={fiveExps} />, { wrapper: Wrapper });
    expect(getPrevBtn()).toBeInTheDocument();
    expect(getNextBtn()).toBeInTheDocument();
  });

  it('hides carousel controls for 1 item', () => {
    render(<Experience experiences={oneExp} />, { wrapper: Wrapper });
    expect(screen.queryByLabelText('Previous')).not.toBeInTheDocument();
    expect(screen.queryByLabelText('Next')).not.toBeInTheDocument();
  });

  it('renders all items with scroll-snap pages', () => {
    render(<Experience experiences={fiveExps} />, { wrapper: Wrapper });
    expect(screen.getByText('Role A')).toBeInTheDocument();
    expect(screen.getByText('Role E')).toBeInTheDocument();
  });

  it('renders correct number of pages', () => {
    const { container } = render(<Experience experiences={fiveExps} />, { wrapper: Wrapper });
    const pages = container.querySelectorAll('.exp-carousel-page');
    expect(pages.length).toBe(5);
  });

  it('disables previous on first page', () => {
    render(<Experience experiences={fiveExps} />, { wrapper: Wrapper });
    expect(getPrevBtn()).toBeDisabled();
    expect(getNextBtn()).not.toBeDisabled();
  });

  it('disables next on last page', () => {
    render(<Experience experiences={fiveExps} />, { wrapper: Wrapper });
    expect(getNextBtn()).not.toBeDisabled();
  });
});
