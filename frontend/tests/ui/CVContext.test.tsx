import { describe, it, expect, vi, afterEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import { useContext } from 'react';
import { CVContext, CVProvider } from '../../src/ui/contexts/CVContext';
import type { GetCVUseCase } from '../../src/application/use-cases/GetCVUseCase';
import type { CV } from '../../src/domain/entities/CV';

const mockCV: CV = {
  name: 'John',
  lastName: 'Doe',
  title: 'Developer',
  summary: 'A skilled developer',
  experiences: [{ period: '2024', role: 'Dev', company: 'Acme', description: 'Work' }],
  skillCategories: [{ name: 'Languages', subCategories: [{ name: '.NET', items: ['C#'] }] }],
  contactInfo: { email: 'john@test.com', phone: '', location: '', willingnessToTravel: '' },
  linkedInUrl: 'https://linkedin.com/in/john',
  gitHubUrl: 'https://github.com/john',
};

function deferredCV() {
  let resolve!: (v: CV) => void;
  let reject!: (e: unknown) => void;
  const promise = new Promise<CV>((res, rej) => { resolve = res; reject = rej; });
  return { promise, resolve, reject };
}

function TestConsumer() {
  const { cv, loading, error, refetch } = useContext(CVContext);
  return (
    <div>
      <span data-testid="loading">{String(loading)}</span>
      <span data-testid="error">{error ?? ''}</span>
      <span data-testid="name">{cv?.name ?? ''}</span>
      <span data-testid="refetch">{typeof refetch}</span>
    </div>
  );
}

function createUseCase(result: Promise<CV>): GetCVUseCase {
  return { execute: () => result } as GetCVUseCase;
}

describe('CVContext', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('shows loading state initially', async () => {
    const { promise } = deferredCV();
    const useCase = createUseCase(promise);

    render(
      <CVProvider getCVUseCase={useCase} locale="en">
        <TestConsumer />
      </CVProvider>,
    );

    expect(screen.getByTestId('loading').textContent).toBe('true');
  });

  it('renders CV data on success', async () => {
    const useCase = createUseCase(Promise.resolve(mockCV));

    render(
      <CVProvider getCVUseCase={useCase} locale="en">
        <TestConsumer />
      </CVProvider>,
    );

    await waitFor(() => {
      expect(screen.getByTestId('name').textContent).toBe('John');
    });
    expect(screen.getByTestId('loading').textContent).toBe('false');
  });

  it('renders error message on failure', async () => {
    const useCase = createUseCase(Promise.reject(new Error('Network failure')));

    render(
      <CVProvider getCVUseCase={useCase} locale="en">
        <TestConsumer />
      </CVProvider>,
    );

    await waitFor(() => {
      expect(screen.getByTestId('error').textContent).toBe('Network failure');
    });
    expect(screen.getByTestId('loading').textContent).toBe('false');
  });

  it('cancels update on unmount', async () => {
    const { promise, resolve } = deferredCV();
    const useCase = createUseCase(promise);

    const { unmount } = render(
      <CVProvider getCVUseCase={useCase} locale="en">
        <TestConsumer />
      </CVProvider>,
    );

    unmount();
    resolve(mockCV);
  });

  it('provides refetch function', () => {
    const useCase = createUseCase(Promise.resolve(mockCV));

    render(
      <CVProvider getCVUseCase={useCase} locale="en">
        <TestConsumer />
      </CVProvider>,
    );

    expect(screen.getByTestId('refetch').textContent).toBe('function');
  });
});
