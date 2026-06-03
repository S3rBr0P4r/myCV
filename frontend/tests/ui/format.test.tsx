import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { renderFormattedText } from '../../src/ui/format';

describe('renderFormattedText', () => {
  it('returns plain text without bold markers', () => {
    const result = renderFormattedText('hello world');
    render(<div>{result}</div>);
    expect(screen.getByText('hello world')).toBeInTheDocument();
  });

  it('wraps **text** in strong elements', () => {
    const result = renderFormattedText('hello **world**');
    render(<div>{result}</div>);
    const strong = screen.getByText('world');
    expect(strong.tagName).toBe('STRONG');
  });

  it('renders [text](url) as anchor element', () => {
    const result = renderFormattedText('visit [Noa](https://noa.ai/) today');
    render(<div>{result}</div>);
    const link = screen.getByText('Noa');
    expect(link.tagName).toBe('A');
    expect(link).toHaveAttribute('href', 'https://noa.ai/');
    expect(link).toHaveAttribute('target', '_blank');
    expect(link).toHaveAttribute('rel', 'noopener noreferrer');
  });

  it('renders bold inside link text', () => {
    const { container } = render(<div>{renderFormattedText('[**world**](https://example.com)')}</div>);
    const link = container.querySelector('a');
    expect(link).toBeTruthy();
    expect(link).toHaveAttribute('href', 'https://example.com');
    const strong = link!.querySelector('strong');
    expect(strong).toBeTruthy();
    expect(strong!.textContent).toBe('world');
  });

  it('returns null for empty string', () => {
    expect(renderFormattedText('')).toBeNull();
  });
});
