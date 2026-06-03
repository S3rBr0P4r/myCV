import { describe, it, expect } from 'vitest';
import { getCompanyUrl } from '../../src/ui/CompanyUrl';

describe('getCompanyUrl', () => {
  it('returns url for exact match', () => {
    expect(getCompanyUrl('ryanair')).toBe('https://www.ryanair.com');
  });

  it('returns url for prefix match', () => {
    expect(getCompanyUrl('Ryanair DAC')).toBe('https://www.ryanair.com');
  });

  it('is case-insensitive', () => {
    expect(getCompanyUrl('RYANAIR')).toBe('https://www.ryanair.com');
  });

  it('returns null for unknown company', () => {
    expect(getCompanyUrl('unknown-corp')).toBeNull();
  });

  it('matches metrohm dropsens before metrohm', () => {
    expect(getCompanyUrl('metrohm dropsens')).toBe('https://www.dropsens.com');
  });

  it('matches hemini PLC to hemini.com', () => {
    expect(getCompanyUrl('Heminí PLC')).toBe('https://www.hemini.com');
  });
});
