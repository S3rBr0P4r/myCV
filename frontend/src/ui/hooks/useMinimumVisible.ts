import { useEffect, useRef, useState } from 'react';

export const MIN_LOADING_MS = 500;

export function useMinimumVisible(active: boolean, durationMs: number = MIN_LOADING_MS): boolean {
  const [visible, setVisible] = useState(active);
  const startedAt = useRef<number | null>(null);

  useEffect(() => {
    if (active) {
      startedAt.current = Date.now();
      setVisible(true);
      return;
    }

    if (startedAt.current === null) {
      setVisible(false);
      return;
    }

    const remaining = durationMs - (Date.now() - startedAt.current);
    if (remaining <= 0) {
      startedAt.current = null;
      setVisible(false);
      return;
    }

    const timer = setTimeout(() => {
      startedAt.current = null;
      setVisible(false);
    }, remaining);
    return () => clearTimeout(timer);
  }, [active, durationMs]);

  return visible;
}
