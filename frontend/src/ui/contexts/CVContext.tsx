import { createContext, useCallback, useEffect, useMemo, useState, type ReactNode } from 'react';
import type { CV } from '../../domain/entities/CV';
import type { GetCVUseCase } from '../../application/use-cases/GetCVUseCase';

interface CVContextValue {
  cv: CV | null;
  loading: boolean;
  refreshing: boolean;
  error: string | null;
  refetch: () => void;
}

export const CVContext = createContext<CVContextValue>({
  cv: null,
  loading: true,
  refreshing: false,
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
  const [pending, setPending] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [refetchKey, setRefetchKey] = useState(0);

  const refetch = useCallback(() => {
    setRefetchKey(k => k + 1);
  }, []);

  useEffect(() => {
    let cancelled = false;
    setPending(true);
    setError(null);

    getCVUseCase.execute(locale).then(data => {
      if (cancelled) return;
      setCV(data);
      setPending(false);
    }).catch(err => {
      if (cancelled) return;
      setError(err instanceof Error ? err.message : 'Failed to load CV');
      setPending(false);
    });

    return () => { cancelled = true; };
  }, [getCVUseCase, locale, refetchKey]);

  const value = useMemo<CVContextValue>(() => ({
    cv,
    loading: pending && cv === null,
    refreshing: pending && cv !== null,
    error,
    refetch,
  }), [cv, pending, error, refetch]);

  return (
    <CVContext.Provider value={value}>
      {children}
    </CVContext.Provider>
  );
}
