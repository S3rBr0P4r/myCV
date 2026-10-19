export const CONSENT_STORAGE_KEY = 'cv-cookie-consent';
export const NOTICE_VERSION = 1;
export const COOKIE_NOTICE_REOPEN_EVENT = 'cookie-notice:reopen';

export type ConsentStatus = 'acknowledged' | 'accepted' | 'rejected';

export interface ConsentRecord {
  version: number;
  status: ConsentStatus;
  decidedAt: string;
}

const isConsentStatus = (value: unknown): value is ConsentStatus =>
  value === 'acknowledged' || value === 'accepted' || value === 'rejected';

const isValidRecord = (value: unknown): value is ConsentRecord => {
  if (typeof value !== 'object' || value === null) return false;
  const record = value as Partial<ConsentRecord>;
  return (
    record.version === NOTICE_VERSION &&
    isConsentStatus(record.status) &&
    typeof record.decidedAt === 'string'
  );
};

export function getConsent(): ConsentRecord | null {
  try {
    const raw = localStorage.getItem(CONSENT_STORAGE_KEY);
    if (!raw) return null;
    const parsed: unknown = JSON.parse(raw);
    if (!isValidRecord(parsed)) return null;
    return parsed;
  } catch {
    return null;
  }
}

export function setConsent(status: ConsentStatus): void {
  const record: ConsentRecord = {
    version: NOTICE_VERSION,
    status,
    decidedAt: new Date().toISOString(),
  };
  localStorage.setItem(CONSENT_STORAGE_KEY, JSON.stringify(record));
}

export function clearConsent(): void {
  localStorage.removeItem(CONSENT_STORAGE_KEY);
}
