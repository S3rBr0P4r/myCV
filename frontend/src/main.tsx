import React from 'react';
import ReactDOM from 'react-dom/client';
import { ApiCVRepository } from './infrastructure/repositories/ApiCVRepository';
import { GetCVUseCase } from './application/use-cases/GetCVUseCase';
import { ThemeProvider } from './ui/contexts/ThemeContext';
import { TranslationProvider } from './ui/contexts/TranslationContext';
import { CVProvider } from './ui/contexts/CVContext';
import { useTranslation } from './ui/hooks/useTranslation';
import { App } from './ui/App';

const repository = new ApiCVRepository();
const getCVUseCase = new GetCVUseCase(repository);

function Root() {
  const { locale } = useTranslation();
  return (
    <CVProvider getCVUseCase={getCVUseCase} locale={locale}>
      <App />
    </CVProvider>
  );
}

ReactDOM.createRoot(document.getElementById('app')!).render(
  <React.StrictMode>
    <ThemeProvider>
      <TranslationProvider>
        <Root />
      </TranslationProvider>
    </ThemeProvider>
  </React.StrictMode>,
);
