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
  'intro.ctaJourney': 'The Journey',
  'intro.ctaContact': 'Contact',

  /* Experience section */
  'experience.title': 'Career',

  /* Skills section */
  'skills.title': 'Mastery',

  /* Contact section */
  'contact.heading': "Let's Talk?",
  'contact.email': 'Email',
  'contact.phone': 'Phone',
  'contact.location': 'Location',
  'contact.travel': 'Willingness to travel',

  /* Education & Certifications */
  'education.title': 'Education',
  'education.certifications': 'Certifications',
  'education.notes': 'Notes',

  /* Footer */
  'footer.copyright': '\u00A9 {year} {name}.',

  /* Scroll progress */
  'nav.dotIntro': 'Home',
  'nav.dotExperience': 'Career',
  'nav.dotSkills': 'Mastery',
  'nav.dotContact': 'Contact',
  'nav.dotAria': 'Quick navigation',

  /* Offline fallback */
  'offline.surnameSuffix': ' (Offline)',
  'offline.summary': 'The backend is not responding, but here is your local data.',
  'offline.connectionError': 'Connection error',
};
