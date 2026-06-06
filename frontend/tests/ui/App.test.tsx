import { describe, it, expect, vi, afterEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import { App } from '../../src/ui/App';
import { CVContext } from '../../src/ui/contexts/CVContext';
import { TranslationProvider } from '../../src/ui/contexts/TranslationContext';
import { ThemeProvider } from '../../src/ui/contexts/ThemeContext';
import type { ReactNode } from 'react';
import type { CV } from '../../src/domain/entities/CV';

const mockCV: CV = {
  name: 'Sergio',
  lastName: 'Brotons',
  title: 'Developer',
  summary: 'A **skilled** developer',
  experiences: [
    { period: '2020-2024', role: 'Senior Dev', company: 'Acme', description: 'Did **work** [here](https://acme.com)' },
    { period: '2018-2020', role: 'Junior Dev', company: 'Beta', description: 'Learned a lot' },
    { period: '2016-2018', role: 'Intern', company: 'Gamma', description: 'Started out' },
    { period: '2014-2016', role: 'Trainee', company: 'Delta', description: 'Early days' },
    { period: '2012-2014', role: 'Apprentice', company: 'Epsilon', description: 'Learning' },
  ],
  skillCategories: [
    { name: 'Languages', subCategories: [{ name: 'Proficient', items: ['C#', 'TS'] }] },
    { name: 'Tools', subCategories: [{ name: 'CI/CD', items: ['GitHub Actions'] }] },
  ],
  linkedInUrl: 'https://linkedin.com/in/sergio',
  gitHubUrl: 'https://github.com/sergio',
};

function TestWrapper({ children }: { children: ReactNode }) {
  return (
    <CVContext.Provider value={{ cv: null, loading: true, error: null, refetch: () => {} }}>
      <TranslationProvider>
        <ThemeProvider>
          {children}
        </ThemeProvider>
      </TranslationProvider>
    </CVContext.Provider>
  );
}

function createWrapper(cv: CV | null, loading: boolean, error: string | null) {
  return function Wrapped({ children }: { children: ReactNode }) {
    return (
      <CVContext.Provider value={{ cv, loading, error, refetch: () => {} }}>
        <TranslationProvider>
          <ThemeProvider>
            {children}
          </ThemeProvider>
        </TranslationProvider>
      </CVContext.Provider>
    );
  };
}

describe('App', () => {
  afterEach(() => {
    document.body.innerHTML = '';
  });

  it('shows loading state', () => {
    render(<App />, { wrapper: TestWrapper });
    expect(screen.getByText('Loading CV...')).toBeInTheDocument();
  });

  it('shows error state', () => {
    const Wrapper = createWrapper(null, false, 'Something went wrong');
    render(<App />, { wrapper: Wrapper });
    expect(screen.getByText('Something went wrong')).toBeInTheDocument();
  });

  it('shows generic error when cv is null without error', () => {
    const Wrapper = createWrapper(null, false, null);
    render(<App />, { wrapper: Wrapper });
    expect(screen.getByText('Failed to load CV')).toBeInTheDocument();
  });

  it('renders main sections when CV loaded', () => {
    const Wrapper = createWrapper(mockCV, false, null);
    render(<App />, { wrapper: Wrapper });

    expect(screen.getByText('Career')).toBeInTheDocument();
    expect(screen.getByText('Capabilities')).toBeInTheDocument();
    expect(screen.getByText("Let's make things happen")).toBeInTheDocument();
  });

  it('renders scroll navigation dots', () => {
    const Wrapper = createWrapper(mockCV, false, null);
    const { container } = render(<App />, { wrapper: Wrapper });

    const nav = container.querySelector('.scroll-progress');
    expect(nav).toBeInTheDocument();
  });
});
