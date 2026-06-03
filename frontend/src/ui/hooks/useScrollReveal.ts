import { useEffect, useRef } from 'react';

export function useScrollReveal(
  selector: string = '.reveal',
  activeClass: string = 'active',
  threshold: number = 0.15,
  deps: unknown[] = [],
) {
  const observerRef = useRef<IntersectionObserver | null>(null);

  useEffect(() => {
    observerRef.current = new IntersectionObserver(
      entries => {
        for (const entry of entries) {
          if (entry.isIntersecting) {
            entry.target.classList.add(activeClass);
          }
        }
      },
      { threshold, rootMargin: '0px 0px -100px 0px' },
    );

    const elements = document.querySelectorAll(selector);
    for (const el of elements) {
      observerRef.current.observe(el);
    }

    return () => {
      observerRef.current?.disconnect();
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [selector, activeClass, threshold, ...deps]);
}
