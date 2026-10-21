import { describe, it, expect } from 'vitest';
import { render } from '@testing-library/react';
import { RefreshProgress } from '../../src/ui/components/RefreshProgress';

describe('RefreshProgress', () => {
  it('renders an indeterminate progress bar', () => {
    const { container } = render(<RefreshProgress />);
    const bar = container.querySelector('.loading-progress');
    expect(bar).toBeInTheDocument();
  });

  it('is hidden from assistive technology', () => {
    const { container } = render(<RefreshProgress />);
    expect(container.querySelector('.loading-progress')).toHaveAttribute('aria-hidden', 'true');
  });
});
