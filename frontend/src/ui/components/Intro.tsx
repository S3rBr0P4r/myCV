import { useTranslation } from '../hooks/useTranslation';
import { renderFormattedText } from '../format';

interface IntroProps {
  name: string;
  summary: string;
  gitHubUrl?: string;
}

export function Intro({ name, summary, gitHubUrl }: IntroProps) {
  const { t } = useTranslation();
  const avatarUrl = gitHubUrl
    ? `https://github.com/${gitHubUrl.replace(/https?:\/\/github\.com\//, '').replace(/\/.*$/, '')}.png`
    : undefined;

  return (
    <section id="intro" className="intro-section">
      <div className="intro-layout">
        {avatarUrl && (
          <div className="intro-polaroid">
            <img
              className="intro-polaroid-img"
              src={avatarUrl}
              alt=""
              loading="lazy"
              decoding="async"
              onError={e => { (e.target as HTMLImageElement).style.display = 'none'; }}
            />
          </div>
        )}
        <div className="intro-content">
          <h1 className="main-title">{renderFormattedText(t('intro.greeting', { name }))}</h1>
          <p className="intro-description">{renderFormattedText(summary)}</p>
        </div>
      </div>
    </section>
  );
}