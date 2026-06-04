import { renderFormattedText } from '../format';

interface IntroProps {
  name: string;
  summary: string;
}

export function Intro({ name, summary }: IntroProps) {
  return (
    <section id="intro" className="intro-section">
      <div className="intro-content">
        <h1 className="main-title">{renderFormattedText(`Well met. I'm ${name}.`)}</h1>
        <p className="intro-description">{renderFormattedText(summary)}</p>
      </div>
    </section>
  );
}
