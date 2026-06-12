import { useEffect } from 'react';
import { useCV } from './hooks/useCV';
import { useTranslation } from './hooks/useTranslation';
import { useScrollReveal } from './hooks/useScrollReveal';
import { useParallax } from './hooks/useParallax';
import type { CV } from '../domain/entities/CV';
import { Header } from './components/Header';
import { Intro } from './components/Intro';
import { Experience } from './components/Experience';
import { AnimatedBackground } from './components/AnimatedBackground';
import { Skills } from './components/Skills';
import { Footer } from './components/Footer';
import { Feedback } from './components/Feedback';

function useSectionTracker(cv: CV | null) {
  useEffect(() => {
    const dots = document.querySelectorAll<HTMLAnchorElement>('.scroll-progress a');
    const sections = document.querySelectorAll<HTMLElement>('section[id]');

    if (sections.length === 0) return;

    const visible = new Set<string>();

    const observer = new IntersectionObserver(entries => {
      for (const entry of entries) {
        if (entry.isIntersecting) {
          visible.add(entry.target.id);
        } else {
          visible.delete(entry.target.id);
        }
      }
      const ids = Array.from(dots).map(d => d.dataset.section).filter(Boolean) as string[];
      const active = ids.findLast(id => visible.has(id)) ?? ids[0];
      dots.forEach(d => d.classList.toggle('active', d.dataset.section === active));
    }, { threshold: 0.3, rootMargin: '0px 0px -30% 0px' });

    sections.forEach(s => observer.observe(s));
    return () => observer.disconnect();
  }, [cv]);
}

function useLocaleDropdown() {
  useEffect(() => {
    const handler = (e: MouseEvent) => {
      const open = document.querySelector('.locale-dropdown.locale-menu--open');
      if (open && !open.contains(e.target as Node)) {
        open.classList.remove('locale-menu--open');
      }
    };
    const keyHandler = (e: KeyboardEvent) => {
      if (e.key === 'Escape') {
        document.querySelector('.locale-dropdown.locale-menu--open')?.classList.remove('locale-menu--open');
      }
    };
    document.addEventListener('click', handler);
    document.addEventListener('keydown', keyHandler);
    return () => {
      document.removeEventListener('click', handler);
      document.removeEventListener('keydown', keyHandler);
    };
  }, []);
}

function buildSections(cv: { skillCategories: unknown[] }, t: (k: string) => string) {
  const sections: { id: string; label: string }[] = [
    { id: 'intro', label: t('nav.dotIntro') },
    { id: 'experience', label: t('nav.dotExperience') },
  ];
  if (Array.isArray(cv.skillCategories) && cv.skillCategories.length > 0) {
    sections.push({ id: 'skills', label: t('nav.dotSkills') });
  }
  sections.push({ id: 'contact', label: t('nav.dotContact') });
  return sections;
}

export function App() {
  const { cv, loading, error } = useCV();
  const { t } = useTranslation();

  useScrollReveal('.reveal', 'active', 0.15, [cv]);
  useParallax('bg', 0.15);
  useParallax('code-dust', 0.05);
  useSectionTracker(cv);
  useLocaleDropdown();

  if (loading) {
    return (
      <>
        <Header />
        <AnimatedBackground />
        <div className="painted-bg" id="bg" />
        <section className="intro-section" style={{ display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
          <p style={{ opacity: 0.5 }}>{t('loading.message')}</p>
        </section>
        <Feedback />
      </>
    );
  }

  if (error || !cv) {
    return (
      <>
        <Header />
        <AnimatedBackground />
        <div className="painted-bg" id="bg" />
        <section style={{
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          flexDirection: 'column',
          gap: '2rem',
          textAlign: 'center',
          padding: 'min(20vh, 160px) 5% 60px',
        }}>
          <img src="/errors/backend_not_responding.webp" alt="" style={{ width: 'min(85vw, 600px)', height: 'auto', display: 'block' }} />
          <div style={{ display: 'flex', flexDirection: 'column', gap: '0.6rem' }}>
            <h2 style={{ color: 'var(--text)', fontSize: '1.6rem', margin: 0 }}>{t('offline.title')}</h2>
            <p style={{ color: 'var(--text-secondary)', margin: 0, maxWidth: '440px' }}>{t('offline.message')}</p>
          </div>
        </section>
        <Feedback />
      </>
    );
  }

  const sections = buildSections(cv, t);

  return (
    <>
      <Header />
      <AnimatedBackground />
      <div className="painted-bg" id="bg" />
      <Intro name={cv.name} summary={cv.summary} gitHubUrl={cv.gitHubUrl} />
      <Experience experiences={cv.experiences} />
      <Skills skillCategories={cv.skillCategories} />
      <Footer name={cv.name} contactInfo={cv.contactInfo} />

      <nav className="scroll-progress" aria-label={t('nav.dotAria')}>
        {sections.map(s => (
          <a
            key={s.id}
            href={`#${s.id}`}
            aria-label={s.label}
            data-section={s.id}
          />
        ))}
      </nav>
      <Feedback />
    </>
  );
}
