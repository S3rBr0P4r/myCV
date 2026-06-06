import { describe, it, expect, vi, afterEach } from 'vitest';
import { render, screen, fireEvent } from '@testing-library/react';
import { Header } from '../../src/ui/components/Header';
import { TranslationProvider } from '../../src/ui/contexts/TranslationContext';
import { ThemeProvider } from '../../src/ui/contexts/ThemeContext';
import { CVContext } from '../../src/ui/contexts/CVContext';
import type { ReactNode } from 'react';
import type { CV } from '../../src/domain/entities/CV';

const fullCV: CV = {
  name: 'Sergio',
  lastName: 'Brotons',
  title: 'Developer',
  summary: 'A developer',
  experiences: [],
  skillCategories: [],
  linkedInUrl: 'https://linkedin.com/in/sergio',
  gitHubUrl: 'https://github.com/sergio',
  contactInfo: { email: 'sergio@test.com', phone: '', location: '', willingnessToTravel: '' },
};

const emptyCV: CV = {
  name: 'Sergio',
  lastName: 'Brotons',
  title: 'Developer',
  summary: 'A developer',
  experiences: [],
  skillCategories: [],
};

function createWrapper(cv: CV | null) {
  return function Wrapper({ children }: { children: ReactNode }) {
    return (
      <CVContext.Provider value={{ cv, loading: false, error: null, refetch: () => {} }}>
        <TranslationProvider>
          <ThemeProvider>
            {children}
          </ThemeProvider>
        </TranslationProvider>
      </CVContext.Provider>
    );
  };
}

describe('Header', () => {
  afterEach(() => {
    document.body.innerHTML = '';
    localStorage.removeItem('cv-locale');
    localStorage.removeItem('cv-theme');
    document.documentElement.removeAttribute('data-theme');
    document.documentElement.removeAttribute('data-locale');
  });

  it('renders LinkedIn link when CV has URL', () => {
    const Wrapper = createWrapper(fullCV);
    render(<Header />, { wrapper: Wrapper });
    const link = screen.getByLabelText('LinkedIn');
    expect(link).toBeInTheDocument();
    expect(link).toHaveAttribute('href', 'https://linkedin.com/in/sergio');
  });

  it('renders GitHub link when CV has URL', () => {
    const Wrapper = createWrapper(fullCV);
    render(<Header />, { wrapper: Wrapper });
    const link = screen.getByLabelText('GitHub');
    expect(link).toBeInTheDocument();
    expect(link).toHaveAttribute('href', 'https://github.com/sergio');
  });

  it('renders Email link when CV has contact', () => {
    const Wrapper = createWrapper(fullCV);
    render(<Header />, { wrapper: Wrapper });
    const link = screen.getByLabelText('Email');
    expect(link).toBeInTheDocument();
    expect(link).toHaveAttribute('href', 'mailto:sergio@test.com');
  });

  it('hides social links when CV lacks data', () => {
    const Wrapper = createWrapper(emptyCV);
    render(<Header />, { wrapper: Wrapper });
    expect(screen.queryByLabelText('LinkedIn')).not.toBeInTheDocument();
    expect(screen.queryByLabelText('GitHub')).not.toBeInTheDocument();
    expect(screen.queryByLabelText('Email')).not.toBeInTheDocument();
  });

  it('renders locale dropdown trigger', () => {
    const Wrapper = createWrapper(fullCV);
    render(<Header />, { wrapper: Wrapper });
    expect(screen.getByLabelText('Switch language')).toBeInTheDocument();
  });

  it('opens locale menu on trigger click', () => {
    const Wrapper = createWrapper(fullCV);
    const { container } = render(<Header />, { wrapper: Wrapper });
    const trigger = screen.getByLabelText('Switch language');
    fireEvent.click(trigger);
    const menu = container.querySelector('.locale-menu--open');
    expect(menu).toBeInTheDocument();
  });

  it('renders theme toggle button', () => {
    const Wrapper = createWrapper(fullCV);
    render(<Header />, { wrapper: Wrapper });
    expect(screen.getByLabelText('Switch theme')).toBeInTheDocument();
  });
});
