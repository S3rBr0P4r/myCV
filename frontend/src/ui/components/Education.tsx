import type { Education as Edu, Certification as Cert } from '../../domain/entities/CV';
import { useTranslation } from '../hooks/useTranslation';
import { renderFormattedText } from '../format';

function EduCard({ edu }: { edu: Edu }) {
  return (
    <div className="edu-card stagger-item">
      <h3 className="edu-degree">{renderFormattedText(edu.degree)}</h3>
      <p className="edu-institution">{renderFormattedText(edu.institution)}</p>
      {edu.notes && <p className="edu-notes">{renderFormattedText(edu.notes)}</p>}
    </div>
  );
}

function CertItem({ cert }: { cert: Cert }) {
  return (
    <li className="cert-item stagger-item">
      <span className="cert-title">{renderFormattedText(cert.title)}</span>
      <span className="cert-issuer">{renderFormattedText(cert.issuer)}</span>
    </li>
  );
}

interface EducationProps {
  education: Edu[];
  certifications: Cert[];
}

export function Education({ education, certifications }: EducationProps) {
  const { t } = useTranslation();

  if (education.length === 0 && certifications.length === 0) return null;

  return (
    <section id="education" className="reveal">
      {education.length > 0 && (
        <>
          <h2 className="section-title">{t('education.title')}</h2>
          <div className="education-grid">
            {education.map((edu, i) => (
              <EduCard key={i} edu={edu} />
            ))}
          </div>
        </>
      )}
      {certifications.length > 0 && (
        <>
          <h2 className="section-title certifications-title">
            {t('education.certifications')}
          </h2>
          <ul className="cert-list">
            {certifications.map((cert, i) => (
              <CertItem key={i} cert={cert} />
            ))}
          </ul>
        </>
      )}
    </section>
  );
}
