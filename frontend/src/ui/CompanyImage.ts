import { findBestMatch } from './CompanyMatch';

const cache = new Map<string, string | null>();

const companyImageMap: Record<string, string> = {
  ryanair: '/backgrounds/ryanair.webp',
  docplanner: '/backgrounds/docplanner.webp',
  'metrohm dropsens': '/backgrounds/metrohm.webp',
  metrohm: '/backgrounds/metrohm.webp',
  dropsens: '/backgrounds/metrohm.webp',
  'plain concepts': '/backgrounds/plain-concepts.webp',
  'roche diagnostics': '/backgrounds/roche.webp',
  roche: '/backgrounds/roche.webp',
  altran: '/backgrounds/altran.webp',
  capgemini: '/backgrounds/altran.webp',
  'hemini plc': '/backgrounds/hemini.webp',
  hemini: '/backgrounds/hemini.webp',
  'imed hospitals': '/backgrounds/imed.webp',
  imed: '/backgrounds/imed.webp',
};

const keysByLength = Object.keys(companyImageMap).sort((a, b) => b.length - a.length);

export function loadCompanyImage(company: string): Promise<string | null> {
  const cached = cache.get(company);
  if (cached !== undefined) return Promise.resolve(cached);

  const url = findBestMatch(company, companyImageMap, keysByLength);
  cache.set(company, url);
  return Promise.resolve(url);
}
