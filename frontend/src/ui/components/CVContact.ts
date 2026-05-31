import { t } from '../../core/TranslationService';

export function renderContact(): string {
  return `
    <section id="contact" class="reveal" style="text-align: center;">
      <div class="contact-card">
        <h2>${t('contact.heading')}</h2>
        <a href="mailto:hello@example.com" class="email-link">hello@example.com</a>
      </div>
    </section>
  `;
}

export function renderFooter(cvName: string, cvLastName: string): string {
  return `
    <footer>
      <p>${t('footer.copyright', { year: 2026, name: cvName, lastName: cvLastName })}</p>
    </footer>
  `;
}
