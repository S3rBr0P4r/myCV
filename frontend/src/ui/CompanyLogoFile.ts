const cache = new Map<string, string | null>();

const companyLogoMap: Record<string, string> = {
  ryanair: '/logos/ryanair.webp',
  docplanner: '/logos/docplanner.webp',
  'metrohm dropsens': '/logos/metrohm.webp',
  metrohm: '/logos/metrohm.webp',
  'plain concepts': '/logos/plainconcepts.webp',
  'roche diagnostics': '/logos/roche.webp',
  roche: '/logos/roche.webp',
  altran: '/logos/altran.webp',
  'hemini plc': '/logos/hemini.webp',
  hemini: '/logos/hemini.webp',
  'imed hospitals': '/logos/imed.webp',
  imed: '/logos/imed.webp',
};

const keysByLength = Object.keys(companyLogoMap).sort(
  (a, b) => b.length - a.length,
);

function findBestMatch(input: string): string | null {
  const normalized = input.toLowerCase().trim();

  const exact = companyLogoMap[normalized];
  if (exact) return exact;

  for (const key of keysByLength) {
    if (normalized.startsWith(key) || key.startsWith(normalized)) {
      return companyLogoMap[key];
    }
  }

  return null;
}

export function loadCompanyLogo(company: string): Promise<string | null> {
  const cached = cache.get(company);
  if (cached !== undefined) return Promise.resolve(cached);

  const url = findBestMatch(company);
  cache.set(company, url);
  return Promise.resolve(url);
}
