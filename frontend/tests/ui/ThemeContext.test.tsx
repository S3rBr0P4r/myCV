import { describe, it, expect, afterEach } from 'vitest';
import { render, screen, fireEvent } from '@testing-library/react';
import { ThemeProvider, ThemeContext } from '../../src/ui/contexts/ThemeContext';
import { useContext } from 'react';

function TestConsumer() {
  const { theme, toggleTheme } = useContext(ThemeContext);
  return (
    <div>
      <span data-testid="theme">{theme}</span>
      <button data-testid="toggle" onClick={toggleTheme}>Toggle</button>
    </div>
  );
}

describe('ThemeContext', () => {
  afterEach(() => {
    localStorage.removeItem('cv-theme');
    document.documentElement.removeAttribute('data-theme');
    document.documentElement.classList.remove('no-transition');
  });

  it('provides default light theme', () => {
    render(
      <ThemeProvider>
        <TestConsumer />
      </ThemeProvider>,
    );
    expect(screen.getByTestId('theme').textContent).toBe('light');
  });

  it('toggles theme on button click', () => {
    render(
      <ThemeProvider>
        <TestConsumer />
      </ThemeProvider>,
    );
    fireEvent.click(screen.getByTestId('toggle'));
    expect(screen.getByTestId('theme').textContent).toBe('dark');
  });

  it('persists theme to localStorage', () => {
    render(
      <ThemeProvider>
        <TestConsumer />
      </ThemeProvider>,
    );
    fireEvent.click(screen.getByTestId('toggle'));
    expect(localStorage.getItem('cv-theme')).toBe('dark');
  });
});
