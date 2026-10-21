import { describe, it, expect, vi, afterEach } from 'vitest';
import { renderHook, act } from '@testing-library/react';
import { useMinimumVisible } from '../../src/ui/hooks/useMinimumVisible';

describe('useMinimumVisible', () => {
  afterEach(() => {
    vi.useRealTimers();
  });

  it('is not visible when never active', () => {
    const { result } = renderHook(() => useMinimumVisible(false, 500));
    expect(result.current).toBe(false);
  });

  it('is visible while active', () => {
    const { result } = renderHook(() => useMinimumVisible(true, 500));
    expect(result.current).toBe(true);
  });

  it('stays visible for the minimum duration after the activity ends', () => {
    vi.useFakeTimers();
    const { result, rerender } = renderHook(
      ({ active }) => useMinimumVisible(active, 500),
      { initialProps: { active: false } },
    );

    rerender({ active: true });
    expect(result.current).toBe(true);

    rerender({ active: false });
    expect(result.current).toBe(true);

    act(() => { vi.advanceTimersByTime(499); });
    expect(result.current).toBe(true);

    act(() => { vi.advanceTimersByTime(2); });
    expect(result.current).toBe(false);
  });

  it('hides immediately when the activity outlasts the minimum duration', () => {
    vi.useFakeTimers();
    const { result, rerender } = renderHook(
      ({ active }) => useMinimumVisible(active, 500),
      { initialProps: { active: false } },
    );

    rerender({ active: true });
    act(() => { vi.advanceTimersByTime(800); });
    rerender({ active: false });
    expect(result.current).toBe(false);
  });

  it('re-activates cleanly when the activity resumes', () => {
    vi.useFakeTimers();
    const { result, rerender } = renderHook(
      ({ active }) => useMinimumVisible(active, 500),
      { initialProps: { active: false } },
    );

    rerender({ active: true });
    act(() => { vi.advanceTimersByTime(200); });
    rerender({ active: false });
    act(() => { vi.advanceTimersByTime(100); });
    rerender({ active: true });

    expect(result.current).toBe(true);

    act(() => { vi.advanceTimersByTime(500); });
    rerender({ active: false });
    expect(result.current).toBe(false);
  });
});
