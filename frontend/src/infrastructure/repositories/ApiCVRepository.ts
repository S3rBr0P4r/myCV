import { type CV } from '../../domain/entities/CV';
import { type ICVRepository } from '../../domain/repositories/ICVRepository';
import { t } from '../../core/TranslationService';

const THRESHOLD = 3;
const RESET_TIMEOUT_MS = 30_000;
const MAX_RETRIES = 2;
const BASE_DELAY_MS = 1000;

let failures = 0;
let lastFailureTime = 0;
let isOpen = false;
let cachedCv: CV | null = null;

function isCircuitOpen(): boolean {
  if (!isOpen) return false;
  if (Date.now() - lastFailureTime > RESET_TIMEOUT_MS) {
    isOpen = false;
    return false;
  }
  return true;
}

function onSuccess(): void {
  failures = 0;
  isOpen = false;
}

function onFailure(): void {
  failures++;
  lastFailureTime = Date.now();
  if (failures >= THRESHOLD) {
    isOpen = true;
  }
}

function buildFallback(): CV {
  return {
    name: 'John',
    lastName: `Doe${t('offline.surnameSuffix')}`,
    title: 'Creative Developer',
    summary: t('offline.summary'),
    experiences: [],
    skills: [t('offline.connectionError')],
  };
}

function delay(ms: number): Promise<void> {
  return new Promise(resolve => setTimeout(resolve, ms));
}

export class ApiCVRepository implements ICVRepository {
  private readonly API_URL = import.meta.env.VITE_API_URL;

  async getCV(locale?: string): Promise<CV> {
    if (cachedCv) {
      return cachedCv;
    }

    if (isCircuitOpen()) {
      return buildFallback();
    }

    for (let attempt = 0; attempt <= MAX_RETRIES; attempt++) {
      try {
        const headers: Record<string, string> = {};
        if (locale) headers['Accept-Language'] = locale;
        const response = await fetch(this.API_URL, { headers });
        if (!response.ok) {
          throw new Error(`HTTP ${response.status}`);
        }
        const cv: CV = await response.json();
        cachedCv = cv;
        onSuccess();
        return cv;
      } catch (error) {
        console.error('API Error:', error);
        if (attempt < MAX_RETRIES) {
          await delay(BASE_DELAY_MS * Math.pow(2, attempt));
        }
      }
    }

    onFailure();
    return buildFallback();
  }
}
