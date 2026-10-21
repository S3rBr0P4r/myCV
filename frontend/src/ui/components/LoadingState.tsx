import { useTheme } from '../hooks/useTheme';
import { useTranslation } from '../hooks/useTranslation';

const SPRITE_WIDTH = 603;
const SPRITE_HEIGHT = 726;

export function LoadingState() {
  const { theme } = useTheme();
  const { locale, t } = useTranslation();

  return (
    <section className="loading-state">
      <img
        className="loading-state-sprite"
        src={`/loading/${locale}-${theme}.webp`}
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
