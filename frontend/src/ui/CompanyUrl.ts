const companyUrlMap: Record<string, string> = {
  ryanair: 'https://www.ryanair.com',
  docplanner: 'https://www.docplanner.com',
  'metrohm dropsens': 'https://www.dropsens.com',
  metrohm: 'https://www.metrohm.com',
  dropsens: 'https://www.dropsens.com',
  'plain concepts': 'https://www.plainconcepts.com',
  'roche diagnostics': 'https://www.roche.com',
  roche: 'https://www.roche.com',
  altran: 'https://www.altran.com',
  capgemini: 'https://www.capgemini.com',
  'hemini plc': 'https://www.hemini.com',
  hemini: 'https://www.hemini.com',
  'imed hospitals': 'https://www.imedhospitales.com',
  imed: 'https://www.imedhospitales.com',
};

const keysByLength = Object.keys(companyUrlMap).sort((a, b) => b.length - a.length);

function findBestMatch(input: string): string | null {
  const normalized = input.toLowerCase().trim();

  const exact = companyUrlMap[normalized];
  if (exact) return exact;

  for (const key of keysByLength) {
    if (normalized.startsWith(key) || key.startsWith(normalized)) {
      return companyUrlMap[key];
    }
  }

  return null;
}

export function getCompanyUrl(company: string): string | null {
  return findBestMatch(company);
}
