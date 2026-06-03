import { type CV } from '../../domain/entities/CV';
import { type ICVRepository } from '../../domain/repositories/ICVRepository';

const FETCH_TIMEOUT_MS = 10_000;
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

function isValidCV(data: unknown): data is CV {
  if (!data || typeof data !== 'object') return false;
  const cv = data as Record<string, unknown>;
  return (
    typeof cv.name === 'string' &&
    typeof cv.lastName === 'string' &&
    typeof cv.title === 'string' &&
    typeof cv.summary === 'string' &&
    Array.isArray(cv.experiences) &&
    Array.isArray(cv.skillCategories) &&
    Array.isArray(cv.education) &&
    Array.isArray(cv.certifications)
  );
}

function buildFallback(): CV {
  return {
    name: 'John',
    lastName: 'Doe (Offline)',
    title: 'Creative Developer',
    summary: 'The backend is not responding, but here is your local data.',
    experiences: [],
    skillCategories: [],
    education: [],
    certifications: [],
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
        const controller = new AbortController();
        const timeoutId = setTimeout(() => controller.abort(), FETCH_TIMEOUT_MS);

        const headers: Record<string, string> = {};
        if (locale) headers['Accept-Language'] = locale;
        const response = await fetch(this.API_URL, { headers, signal: controller.signal });
        clearTimeout(timeoutId);

        if (!response.ok) {
          throw new Error(`HTTP ${response.status}`);
        }
        const parsed: unknown = await response.json();
        if (!isValidCV(parsed)) {
          throw new Error('Invalid CV data received from API');
        }
        cachedCv = parsed;
        onSuccess();
        return parsed;
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
