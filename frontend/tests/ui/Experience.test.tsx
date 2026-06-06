import { describe, it, expect } from 'vitest';
import { render, screen, fireEvent } from '@testing-library/react';
import { Experience } from '../../src/ui/components/Experience';
import { TranslationProvider } from '../../src/ui/contexts/TranslationContext';
import type { ReactNode } from 'react';
import type { Experience as ExpType } from '../../src/domain/entities/CV';

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

describe('Experience', () => {
  it('renders section title', () => {
    render(<Experience experiences={twoExps} />, { wrapper: Wrapper });
    expect(screen.getByText('Career')).toBeInTheDocument();
  });

  it('renders experience cards', () => {
    render(<Experience experiences={twoExps} />, { wrapper: Wrapper });
    expect(screen.getByText('Senior Dev')).toBeInTheDocument();
    expect(screen.getByText('Junior Dev')).toBeInTheDocument();
  });

  it('shows company name', () => {
    render(<Experience experiences={twoExps} />, { wrapper: Wrapper });
    expect(screen.getByText('Acme')).toBeInTheDocument();
    expect(screen.getByText('Beta')).toBeInTheDocument();
  });

  it('shows period', () => {
    render(<Experience experiences={twoExps} />, { wrapper: Wrapper });
    expect(screen.getByText('2024')).toBeInTheDocument();
  });

  it('shows description', () => {
    render(<Experience experiences={twoExps} />, { wrapper: Wrapper });
    expect(screen.getByText('Did work')).toBeInTheDocument();
  });

  it('does not show pagination for <= 2 items', () => {
    render(<Experience experiences={twoExps} />, { wrapper: Wrapper });
    expect(screen.queryByText('← Previous')).not.toBeInTheDocument();
    expect(screen.queryByText('Next →')).not.toBeInTheDocument();
  });

  it('shows pagination for > 2 items', () => {
    render(<Experience experiences={fiveExps} />, { wrapper: Wrapper });
    expect(screen.getByText('← Previous')).toBeInTheDocument();
    expect(screen.getByText('Next →')).toBeInTheDocument();
  });

  it('shows first page by default', () => {
    render(<Experience experiences={fiveExps} />, { wrapper: Wrapper });
    expect(screen.getByText('Role A')).toBeInTheDocument();
    expect(screen.getByText('Role B')).toBeInTheDocument();
    expect(screen.queryByText('Role C')).not.toBeInTheDocument();
  });

  it('navigates to next page', () => {
    render(<Experience experiences={fiveExps} />, { wrapper: Wrapper });
    fireEvent.click(screen.getByText('Next →'));
    expect(screen.getByText('Role C')).toBeInTheDocument();
    expect(screen.getByText('Role D')).toBeInTheDocument();
    expect(screen.queryByText('Role A')).not.toBeInTheDocument();
  });

  it('disables Previous on page 1', () => {
    render(<Experience experiences={fiveExps} />, { wrapper: Wrapper });
    expect(screen.getByText('← Previous')).toBeDisabled();
    expect(screen.getByText('Next →')).not.toBeDisabled();
  });

  it('disables Next on last page', () => {
    render(<Experience experiences={fiveExps} />, { wrapper: Wrapper });
    fireEvent.click(screen.getByText('Next →'));
    fireEvent.click(screen.getByText('Next →'));
    expect(screen.getByText('Next →')).toBeDisabled();
    expect(screen.getByText('← Previous')).not.toBeDisabled();
  });

  it('shows page indicator', () => {
    render(<Experience experiences={fiveExps} />, { wrapper: Wrapper });
    expect(screen.getByText('1 / 3')).toBeInTheDocument();
    fireEvent.click(screen.getByText('Next →'));
    expect(screen.getByText('2 / 3')).toBeInTheDocument();
  });
});
