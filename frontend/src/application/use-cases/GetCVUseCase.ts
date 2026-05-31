import { CV } from '../../domain/entities/CV';
import { ICVRepository } from '../../domain/repositories/ICVRepository';

export class GetCVUseCase {
  constructor(private cvRepository: ICVRepository) {}

  async execute(locale?: string): Promise<CV> {
    return await this.cvRepository.getCV(locale);
  }
}
