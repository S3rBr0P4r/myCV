import { createContext, useCallback, useEffect, useState, type ReactNode } from 'react';
import type { CV } from '../../domain/entities/CV';
import type { GetCVUseCase } from '../../application/use-cases/GetCVUseCase';

interface CVContextValue {
  cv: CV | null;
  loading: boolean;
  error: string | null;
  refetch: () => void;
}

export const CVContext = createContext<CVContextValue>({
  cv: null,
  loading: true,
  error: null,
  refetch: () => {},
});

export function CVProvider({
  getCVUseCase,
  locale,
  children,
}: {
  getCVUseCase: GetCVUseCase;
  locale: string;
  children: ReactNode;
}) {
  const [cv, setCV] = useState<CV | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [refetchKey, setRefetchKey] = useState(0);

  const refetch = useCallback(() => {
    setRefetchKey(k => k + 1);
  }, []);

  useEffect(() => {
    let cancelled = false;
    setLoading(true);
    setError(null);

    getCVUseCase.execute(locale).then(data => {
      if (cancelled) return;
      setCV(data);
      setLoading(false);
    }).catch(err => {
      if (cancelled) return;
      setError(err instanceof Error ? err.message : 'Failed to load CV');
      setLoading(false);
    });

    return () => { cancelled = true; };
  }, [getCVUseCase, locale, refetchKey]);

  return (
    <CVContext.Provider value={{ cv, loading, error, refetch }}>
      {children}
    </CVContext.Provider>
  );
}
