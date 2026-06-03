import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { ApiCVRepository } from '../../../src/infrastructure/repositories/ApiCVRepository';

const validCV = {
  name: 'John',
  lastName: 'Doe',
  title: 'Developer',
  summary: 'A summary',
  experiences: [],
  skillCategories: [],
  education: [],
  certifications: [],
};

describe('ApiCVRepository', () => {
  beforeEach(() => {
    vi.stubGlobal('fetch', vi.fn());
  });

  afterEach(() => {
    vi.unstubAllGlobals();
    vi.restoreAllMocks();
  });

  it('fetches CV with Accept-Language header', async () => {
    vi.mocked(fetch).mockResolvedValueOnce({
      ok: true,
      json: () => Promise.resolve(validCV),
    } as Response);

    const repo = new ApiCVRepository();
    const result = await repo.getCV('es');

    expect(result.name).toBe('John');
    expect(fetch).toHaveBeenCalledWith(
      expect.any(String),
      expect.objectContaining({
        headers: { 'Accept-Language': 'es' },
      }),
    );
  });

  it('returns fallback on HTTP error', async () => {
    vi.mocked(fetch).mockResolvedValueOnce({
      ok: false,
      status: 500,
    } as Response);

    const repo = new ApiCVRepository();
    const result = await repo.getCV('en');

    expect(result.experiences).toEqual([]);
  });

  it('returns fallback on network error', async () => {
    vi.mocked(fetch).mockRejectedValueOnce(new Error('Network error'));

    const repo = new ApiCVRepository();
    const result = await repo.getCV('en');

    expect(result.experiences).toEqual([]);
  });
});
