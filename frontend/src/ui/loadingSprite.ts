export function loadingSpritePath(locale: string, theme: string): string {
  return `/loading/${locale}-${theme}.webp`;
}

const preloaded: HTMLImageElement[] = [];

export function preloadLoadingSprite(locale: string, theme: string): void {
  const image = new Image();
  image.src = loadingSpritePath(locale, theme);
  preloaded.push(image);
  if (preloaded.length > 4) preloaded.shift();
}
