import { CV } from '../entities/CV';

export interface ICVRepository {
  getCV(locale?: string): Promise<CV>;
}
