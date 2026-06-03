import { renderFormattedText } from '../format';

interface IntroProps {
  summary: string;
}

export function Intro({ summary }: IntroProps) {
  return (
    <section id="intro" className="intro-section">
      <div className="intro-content">
        <p className="intro-description">{renderFormattedText(summary)}</p>
      </div>
    </section>
  );
}
