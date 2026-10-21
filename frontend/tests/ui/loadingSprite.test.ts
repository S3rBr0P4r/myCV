import { describe, it, expect, vi, afterEach } from 'vitest';

class FakeImage {
  src = '';
  constructor() {
    instances.push(this);
  }
}

const instances: FakeImage[] = [];

async function freshModule() {
  vi.resetModules();
  return await import('../../src/ui/loadingSprite');
}

describe('loadingSprite', () => {
  afterEach(() => {
    vi.unstubAllGlobals();
    instances.length = 0;
  });

  it('builds the sprite path from locale and theme', async () => {
    const { loadingSpritePath } = await freshModule();
    expect(loadingSpritePath('en', 'light')).toBe('/loading/en-light.webp');
    expect(loadingSpritePath('es', 'dark')).toBe('/loading/es-dark.webp');
  });

  it('preloads the sprite as an image request', async () => {
    vi.stubGlobal('Image', FakeImage);
    const { preloadLoadingSprite } = await freshModule();
    preloadLoadingSprite('en', 'light');
    expect(instances).toHaveLength(1);
    expect(instances[0].src).toBe('/loading/en-light.webp');
  });

  it('requests the sprite again when asked for a different theme', async () => {
    vi.stubGlobal('Image', FakeImage);
    const { preloadLoadingSprite } = await freshModule();
    preloadLoadingSprite('es', 'dark');
    preloadLoadingSprite('es', 'light');
    expect(instances.map((i) => i.src)).toEqual([
      '/loading/es-dark.webp',
      '/loading/es-light.webp',
    ]);
  });
});
