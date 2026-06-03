import { useContext } from 'react';
import { TranslationContext } from '../contexts/TranslationContext';

export function useTranslation() {
  return useContext(TranslationContext);
}
