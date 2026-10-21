import { useTheme } from '../hooks/useTheme';
import { useTranslation } from '../hooks/useTranslation';
import { loadingSpritePath } from '../loadingSprite';

const SPRITE_WIDTH = 603;
const SPRITE_HEIGHT = 726;

export type LoadingVariant = 'page' | 'overlay';

export function LoadingState({ variant = 'page' }: { variant?: LoadingVariant }) {
  const { theme } = useTheme();
  const { locale, t } = useTranslation();

  return (
    <section className={`loading-state loading-state--${variant}`}>
      <img
        className="loading-state-sprite"
        src={loadingSpritePath(locale, theme)}
        alt=""
        width={SPRITE_WIDTH}
        height={SPRITE_HEIGHT}
        decoding="async"
      />
      <p className="sr-only" role="status" aria-live="polite">
        {t('loading.message')}
      </p>
    </section>
  );
}
