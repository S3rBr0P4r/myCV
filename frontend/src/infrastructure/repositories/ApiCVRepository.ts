import { CV } from '../../domain/entities/CV';
import { ICVRepository } from '../../domain/repositories/ICVRepository';
import { t } from '../../core/TranslationService';

export class ApiCVRepository implements ICVRepository {
  private readonly API_URL = import.meta.env.VITE_API_URL;

  async getCV(locale?: string): Promise<CV> {
    try {
      const headers: Record<string, string> = {};
      if (locale) headers['Accept-Language'] = locale;
      const response = await fetch(this.API_URL, { headers });
      if (!response.ok) {
        throw new Error('Failed to fetch CV data from backend');
      }
      return await response.json();
    } catch (error) {
      console.error('API Error:', error);
      return {
        name: 'John',
        lastName: `Doe${t('offline.surnameSuffix')}`,
        title: 'Creative Developer',
        summary: t('offline.summary'),
        experiences: [],
        skills: [t('offline.connectionError')],
      };
    }
  }
}
