import type { ContactInfo } from '../../domain/entities/CV';
import { useTranslation } from '../hooks/useTranslation';
import { renderFormattedText } from '../format';

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
      </footer>
    </>
  );
}
