import { useEffect } from 'react';
import { useCV } from './hooks/useCV';
import { useTranslation } from './hooks/useTranslation';
import { useScrollReveal } from './hooks/useScrollReveal';
import { useParallax } from './hooks/useParallax';
import type { CV } from '../domain/entities/CV';
import { Header } from './components/Header';
import { Intro } from './components/Intro';
import { Experience } from './components/Experience';
import { Skills } from './components/Skills';
import { Education } from './components/Education';
import { Footer } from './components/Footer';

function useSectionTracker(cv: CV | null) {
  useEffect(() => {
    const dots = document.querySelectorAll<HTMLAnchorElement>('.scroll-progress a');
    const sections = document.querySelectorAll<HTMLElement>('section[id]');

    if (sections.length === 0) return;

    const observer = new IntersectionObserver(entries => {
      for (const entry of entries) {
        if (entry.isIntersecting) {
          dots.forEach(d => d.classList.toggle('active', d.dataset.section === entry.target.id));
        }
      }
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

function buildSections(cv: { skillCategories: unknown[]; education: unknown[]; certifications: unknown[] }, t: (k: string) => string) {
  const sections: { id: string; label: string }[] = [
    { id: 'intro', label: t('nav.dotIntro') },
    { id: 'experience', label: t('nav.dotExperience') },
  ];
  if (cv.skillCategories.length > 0) {
    sections.push({ id: 'skills', label: t('nav.dotSkills') });
  }
  if (cv.education.length > 0 || cv.certifications.length > 0) {
    sections.push({ id: 'education', label: 'Education' });
  }
  sections.push({ id: 'contact', label: t('nav.dotContact') });
  return sections;
}

export function App() {
  const { cv, loading, error, refetch } = useCV();
  const { t, locale } = useTranslation();

  useScrollReveal('.reveal', 'active', 0.15, [cv]);
  useParallax('bg', 0.15);
  useSectionTracker(cv);
  useLocaleDropdown();

  useEffect(() => {
    if (cv) refetch();
  }, [locale]); // eslint-disable-line react-hooks/exhaustive-deps

  if (loading) {
    return (
      <>
        <Header />
        <div className="painted-bg" id="bg" />
        <section className="intro-section" style={{ display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
          <p style={{ opacity: 0.5 }}>Loading CV...</p>
        </section>
      </>
    );
  }

  if (error || !cv) {
    return (
      <>
        <Header />
        <div className="painted-bg" id="bg" />
        <section className="intro-section" style={{ display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
          <p style={{ color: 'var(--accent)' }}>{error || 'Failed to load CV'}</p>
        </section>
      </>
    );
  }

  const sections = buildSections(cv, t);

  return (
    <>
      <Header />
      <div className="painted-bg" id="bg" />
      <Intro summary={cv.summary} />
      <Experience experiences={cv.experiences} />
      <Skills skillCategories={cv.skillCategories} />
      <Education education={cv.education} certifications={cv.certifications} />
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
    </>
  );
}
