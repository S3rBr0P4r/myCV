import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { renderFormattedText } from '../../src/ui/format';
import { TranslationProvider } from '../../src/ui/contexts/TranslationContext';

describe('renderFormattedText', () => {
  it('returns plain text without bold markers', () => {
    const result = renderFormattedText('hello world');
    render(<TranslationProvider><div>{result}</div></TranslationProvider>);
    expect(screen.getByText('hello world')).toBeInTheDocument();
  });

  it('wraps **text** in strong elements', () => {
    const result = renderFormattedText('hello **world**');
    render(<TranslationProvider><div>{result}</div></TranslationProvider>);
    const strong = screen.getByText('world');
    expect(strong.tagName).toBe('STRONG');
  });

  it('returns null for empty string', () => {
    expect(renderFormattedText('')).toBeNull();
  });
});
