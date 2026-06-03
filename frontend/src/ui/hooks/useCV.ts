import { useContext } from 'react';
import { CVContext } from '../contexts/CVContext';

export function useCV() {
  return useContext(CVContext);
}
