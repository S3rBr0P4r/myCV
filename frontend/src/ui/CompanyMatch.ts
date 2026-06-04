export function normalizeCompanyName(name: string): string {
  return name.toLowerCase().trim().normalize('NFD').replace(/[\u0300-\u036f]/g, '');
}

export function findBestMatch<T>(
  input: string,
  map: Record<string, T>,
  keysByLength: string[],
): T | null {
  const normalized = normalizeCompanyName(input);

  const exact = map[normalized];
  if (exact) return exact;

  for (const key of keysByLength) {
    if (normalized.startsWith(key) || key.startsWith(normalized)) {
      return map[key];
    }
  }

  return null;
}

export function createLookup<T>(map: Record<string, T>): (input: string) => T | null {
  const keysByLength = Object.keys(map).sort((a, b) => b.length - a.length);
  return (input: string) => findBestMatch(input, map, keysByLength);
}

export function createCachedLookup<T>(map: Record<string, T>): (input: string) => Promise<T | null> {
  const lookup = createLookup(map);
  const cache = new Map<string, T | null>();
  return (input: string) => {
    const cached = cache.get(input);
    if (cached !== undefined) return Promise.resolve(cached);
    const result = lookup(input);
    cache.set(input, result);
    return Promise.resolve(result);
  };
}
