import type { ContactInfo } from '../../domain/entities/CV';
import { useTranslation } from '../hooks/useTranslation';
import { renderFormattedText } from '../format';

const GITHUB_REPO = 'https://github.com/s3rbr0p4r/mycv';

interface FooterProps {
  name: string;
  contactInfo?: ContactInfo | null;
}

export function Footer({ name, contactInfo }: FooterProps) {
  const { t } = useTranslation();

  return (
    <>
      <section id="contact" className="reveal" style={{ textAlign: 'center' }}>
        <div className="contact-card">
          <h2>{t('contact.heading')}</h2>
          {contactInfo?.email && (
            <a className="email-link" href={`mailto:${contactInfo.email}`}>
              {contactInfo.email}
            </a>
          )}
        </div>
      </section>
      <footer>
        <p>{renderFormattedText(t('footer.copyright', { year: 2026, name }))}</p>
        <p className="footer-attribution">
          Crafted with <span className="heart">{'\u2764'}</span> in React &bull; TypeScript &bull; .NET &bull;{' '}
          <a href={GITHUB_REPO} target="_blank" rel="noopener noreferrer">View source on GitHub</a>
        </p>
      </footer>
    </>
  );
}
