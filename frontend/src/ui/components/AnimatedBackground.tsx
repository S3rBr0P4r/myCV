import { useEffect, useRef } from 'react';

const CHARS = ['<', '>', '{', '}', '/', '.', '_', '[', ']', '|', ':', '+', '=', '#', '@', '?', '*'];

const LIGHT_COLORS = ['#c49a3c', '#3d4a5c', '#8a8f97', '#a5aab2', '#6b7078'];
const DARK_COLORS = ['#d4a54b', '#7a8fa8', '#6b7078', '#92979f', '#b0b4bc'];

interface Particle {
  x: number;
  y: number;
  char: string;
  size: number;
  speed: number;
  driftAmp: number;
  driftFreq: number;
  phase: number;
  opacity: number;
  color: string;
}

function colorsFor(theme: string): string[] {
  return theme === 'dark' ? DARK_COLORS : LIGHT_COLORS;
}

function createParticle(w: number, h: number, theme: string): Particle {
  const palette = colorsFor(theme);
  return {
    x: Math.random() * w,
    y: Math.random() * h,
    char: CHARS[Math.floor(Math.random() * CHARS.length)],
    size: 9 + Math.random() * 2,
    speed: 0.08 + Math.random() * 0.17,
    driftAmp: 0.2 + Math.random() * 0.4,
    driftFreq: 0.001 + Math.random() * 0.003,
    phase: Math.random() * Math.PI * 2,
    opacity: 0.04 + Math.random() * 0.16,
    color: palette[Math.floor(Math.random() * palette.length)],
  };
}

const COUNT = 45;

export function AnimatedBackground() {
  const ref = useRef<HTMLCanvasElement>(null);
  const particles = useRef<Particle[]>([]);
  const raf = useRef(0);

  useEffect(() => {
    const canvas = ref.current;
    if (!canvas) return;
    const ctx = canvas.getContext('2d');
    if (!ctx) return;

    const reduced = window.matchMedia('(prefers-reduced-motion: reduce)').matches;

    const resize = () => {
      canvas.width = window.innerWidth;
      canvas.height = window.innerHeight;
    };
    resize();
    window.addEventListener('resize', resize);

    const theme = document.documentElement.getAttribute('data-theme') || 'light';
    particles.current = Array.from({ length: COUNT }, () =>
      createParticle(canvas.width, canvas.height, theme)
    );

    const observer = new MutationObserver(() => {
      const t = document.documentElement.getAttribute('data-theme') || 'light';
      const palette = colorsFor(t);
      for (const p of particles.current) {
        p.color = palette[Math.floor(Math.random() * palette.length)];
      }
    });
    observer.observe(document.documentElement, {
      attributes: true,
      attributeFilter: ['data-theme'],
    });

    if (reduced) {
      for (const p of particles.current) {
        ctx.font = `${p.size}px "JetBrains Mono", monospace`;
        ctx.fillStyle = p.color;
        ctx.globalAlpha = p.opacity;
        ctx.fillText(p.char, p.x, p.y);
      }
      ctx.globalAlpha = 1;
      return () => {
        window.removeEventListener('resize', resize);
        observer.disconnect();
      };
    }

    const draw = (ts: number) => {
      ctx.clearRect(0, 0, canvas.width, canvas.height);

      for (const p of particles.current) {
        p.y -= p.speed;
        p.x += Math.sin(ts * p.driftFreq + p.phase) * p.driftAmp;

        if (p.y < -20) {
          const t = document.documentElement.getAttribute('data-theme') || 'light';
          const palette = colorsFor(t);
          p.y = canvas.height + 10;
          p.x = Math.random() * canvas.width;
          p.char = CHARS[Math.floor(Math.random() * CHARS.length)];
          p.color = palette[Math.floor(Math.random() * palette.length)];
          p.opacity = 0.04 + Math.random() * 0.16;
        }

        ctx.font = `${p.size}px "JetBrains Mono", monospace`;
        ctx.fillStyle = p.color;
        ctx.globalAlpha = p.opacity;
        ctx.fillText(p.char, p.x, p.y);
      }

      ctx.globalAlpha = 1;
      raf.current = requestAnimationFrame(draw);
    };

    raf.current = requestAnimationFrame(draw);

    return () => {
      cancelAnimationFrame(raf.current);
      window.removeEventListener('resize', resize);
      observer.disconnect();
    };
  }, []);

  return (
    <canvas
      ref={ref}
      id="code-dust"
      style={{
        position: 'fixed',
        inset: 0,
        width: '100vw',
        height: '100vh',
        pointerEvents: 'none',
        zIndex: 0,
      }}
    />
  );
}
