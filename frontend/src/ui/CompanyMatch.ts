export function normalizeCompanyName(name: string): string {
  return name.toLowerCase().trim().normalize('NFD').replace(/[\u0300-\u036f]/g, '');
}

export function findBestMatch(
  input: string,
  map: Record<string, string>,
  keysByLength: string[],
): string | null {
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
