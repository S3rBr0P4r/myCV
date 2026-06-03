import { useEffect, useRef } from 'react';

export function useParallax(elementId: string, speed: number = 0.15) {
  const scrollRef = useRef(0);

  useEffect(() => {
    const element = document.getElementById(elementId);
    if (!element) return;

    const onScroll = () => {
      scrollRef.current = window.pageYOffset;
      element.style.transform = `translateY(${scrollRef.current * speed}px)`;
    };

    window.addEventListener('scroll', onScroll, { passive: true });
    return () => window.removeEventListener('scroll', onScroll);
  }, [elementId, speed]);
}
