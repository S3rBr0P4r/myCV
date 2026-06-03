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

function findBestMatch(input: string): string | null {
  const normalized = input.toLowerCase().trim();

  const exact = companyImageMap[normalized];
  if (exact) return exact;

  for (const key of keysByLength) {
    if (normalized.startsWith(key) || key.startsWith(normalized)) {
      return companyImageMap[key];
    }
  }

  return null;
}

export function loadCompanyImage(company: string): Promise<string | null> {
  const cached = cache.get(company);
  if (cached !== undefined) return Promise.resolve(cached);

  const url = findBestMatch(company);
  cache.set(company, url);
  return Promise.resolve(url);
}
