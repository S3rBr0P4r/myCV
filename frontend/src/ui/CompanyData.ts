import { createLookup, createCachedLookup } from './CompanyMatch';

interface CompanyAssets {
  url: string;
  image: string;
  logo: string;
}

const companyData: Record<string, CompanyAssets> = {
  ryanair: {
    url: 'https://www.ryanair.com',
    image: '/backgrounds/ryanair.webp',
    logo: '/logos/ryanair.webp',
  },
  docplanner: {
    url: 'https://www.docplanner.com',
    image: '/backgrounds/docplanner.webp',
    logo: '/logos/docplanner.webp',
  },
  'metrohm dropsens': {
    url: 'https://www.dropsens.com',
    image: '/backgrounds/metrohm.webp',
    logo: '/logos/metrohm.webp',
  },
  'plain concepts': {
    url: 'https://www.plainconcepts.com',
    image: '/backgrounds/plain-concepts.webp',
    logo: '/logos/plainconcepts.webp',
  },
  'roche diagnostics': {
    url: 'https://www.roche.com',
    image: '/backgrounds/roche.webp',
    logo: '/logos/roche.webp',
  },
  altran: {
    url: 'https://www.altran.com',
    image: '/backgrounds/altran.webp',
    logo: '/logos/altran.webp',
  },
  'hemini plc': {
    url: 'https://www.hemini.com',
    image: '/backgrounds/hemini.webp',
    logo: '/logos/hemini.webp',
  },
  'imed hospitals': {
    url: 'https://www.imedhospitales.com',
    image: '/backgrounds/imed.webp',
    logo: '/logos/imed.webp',
  },
};

const urlMap: Record<string, string> = {};
const imageMap: Record<string, string> = {};
const logoMap: Record<string, string> = {};
for (const [key, a] of Object.entries(companyData)) {
  urlMap[key] = a.url;
  imageMap[key] = a.image;
  logoMap[key] = a.logo;
}

export const getCompanyUrl = createLookup(urlMap);
export const loadCompanyImage = createCachedLookup(imageMap);
export const loadCompanyLogo = createCachedLookup(logoMap);
