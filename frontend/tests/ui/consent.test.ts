import { describe, it, expect, beforeEach } from 'vitest';
import {
  CONSENT_STORAGE_KEY,
  NOTICE_VERSION,
  clearConsent,
  getConsent,
  setConsent,
} from '../../src/ui/consent';

describe('consent', () => {
  beforeEach(() => {
    localStorage.clear();
  });

  it('returns null when nothing is stored', () => {
    expect(getConsent()).toBeNull();
  });

  it('round-trips a record through setConsent and getConsent', () => {
    setConsent('acknowledged');
    const record = getConsent();

    expect(record).not.toBeNull();
    expect(record?.version).toBe(NOTICE_VERSION);
    expect(record?.status).toBe('acknowledged');
    expect(Number.isNaN(Date.parse(record!.decidedAt))).toBe(false);
  });

  it('returns null for corrupted JSON', () => {
    localStorage.setItem(CONSENT_STORAGE_KEY, 'not-json{');
    expect(getConsent()).toBeNull();
  });

  it('returns null for an outdated record version', () => {
    localStorage.setItem(
      CONSENT_STORAGE_KEY,
      JSON.stringify({ version: NOTICE_VERSION - 1, status: 'acknowledged', decidedAt: 'x' }),
    );
    expect(getConsent()).toBeNull();
  });

  it('returns null for an invalid status', () => {
    localStorage.setItem(
      CONSENT_STORAGE_KEY,
      JSON.stringify({ version: NOTICE_VERSION, status: 'maybe', decidedAt: 'x' }),
    );
    expect(getConsent()).toBeNull();
  });

  it('returns null when decidedAt is missing', () => {
    localStorage.setItem(
      CONSENT_STORAGE_KEY,
      JSON.stringify({ version: NOTICE_VERSION, status: 'acknowledged' }),
    );
    expect(getConsent()).toBeNull();
  });

  it('clearConsent removes the stored record', () => {
    setConsent('acknowledged');
    clearConsent();
    expect(localStorage.getItem(CONSENT_STORAGE_KEY)).toBeNull();
    expect(getConsent()).toBeNull();
  });
});
