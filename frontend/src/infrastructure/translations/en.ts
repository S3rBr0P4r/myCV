export type TranslationMap = Record<string, string | ((...args: unknown[]) => string)>;

export const en: Record<string, string> = {
  /* Navbar */
  'nav.logo': 'GHIBLI',
  'nav.logoSuffix': 'CV',
  'nav.themeLabel': 'Switch theme',
  'nav.localeLabel': 'Switch language',
  'nav.localeEn': 'English',
  'nav.localeEs': 'Spanish',

  /* Hero */
  'hero.subtitle': 'Creative Developer & Architect',
  'hero.ctaJourney': 'The Journey',
  'hero.ctaContact': 'Contact',

  /* Experience section */
  'experience.title': 'Career',

  /* Skills section */
  'skills.title': 'Mastery',

  /* Contact section */
  'contact.heading': "Let's Talk?",

  /* Footer */
  'footer.copyright': '\u00A9 {year} {name} {lastName}. Clean Architecture Edition.',

  /* Scroll progress */
  'nav.dotHero': 'Home',
  'nav.dotExperience': 'Career',
  'nav.dotSkills': 'Mastery',
  'nav.dotContact': 'Contact',
  'nav.dotAria': 'Quick navigation',

  /* CV content (English pass-through) */
  '2024 - PRESENT': '2024 - PRESENT',
  '2021 - 2023': '2021 - 2023',
  'Senior Developer': 'Senior Developer',
  'Full Stack Engineer': 'Full Stack Engineer',
  'Redefining the web with handcrafted, fluid architecture.': 'Redefining the web with handcrafted, fluid architecture.',
  'Creating immersive worlds with attention to detail.': 'Creating immersive worlds with attention to detail.',
  'Building digital experiences with the softness of a sunset and the precision of a craftsman.': 'Building digital experiences with the softness of a sunset and the precision of a craftsman.',
  'Creative Developer & Architect': 'Creative Developer & Architect',

  /* Offline fallback */
  'offline.surnameSuffix': ' (Offline)',
  'offline.summary': 'The backend is not responding, but here is your local data.',
  'offline.connectionError': 'Connection error',
};
