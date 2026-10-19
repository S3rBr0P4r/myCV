import { useCallback, useEffect, useRef, useState } from 'react';
import { useTranslation } from '../hooks/useTranslation';
import {
  COOKIE_NOTICE_REOPEN_EVENT,
  getConsent,
  setConsent,
} from '../consent';

const HEIGHT_VAR = '--cookie-h';

export function CookieNotice() {
  const { t } = useTranslation();
  const [visible, setVisible] = useState<boolean>(() => getConsent() === null);
  const barRef = useRef<HTMLDivElement | null>(null);

  useEffect(() => {
    const reopen = () => setVisible(getConsent() === null);
    window.addEventListener(COOKIE_NOTICE_REOPEN_EVENT, reopen);
    return () => window.removeEventListener(COOKIE_NOTICE_REOPEN_EVENT, reopen);
  }, []);

  useEffect(() => {
    const bar = barRef.current;
    if (!visible || !bar) return;
    const publishHeight = () => {
      document.documentElement.style.setProperty(HEIGHT_VAR, `${bar.offsetHeight}px`);
    };
    publishHeight();
    const observer =
      typeof ResizeObserver !== 'undefined' ? new ResizeObserver(publishHeight) : null;
    observer?.observe(bar);
    return () => {
      observer?.disconnect();
      document.documentElement.style.removeProperty(HEIGHT_VAR);
    };
  }, [visible]);

  const dismiss = useCallback(() => {
    setConsent('acknowledged');
    setVisible(false);
  }, []);

  if (!visible) return null;

  return (
    <div className="site-note" role="region" aria-label={t('cookie.ariaLabel')} ref={barRef}>
      <div className="site-note-inner">
        <div className="site-note-text">
          <p className="site-note-title">{t('cookie.title')}</p>
          <p className="site-note-message">{t('cookie.message')}</p>
          <details className="site-note-details">
            <summary>{t('cookie.detailsSummary')}</summary>
            <p className="site-note-details-body">{t('cookie.detailsBody')}</p>
          </details>
        </div>
        <button type="button" className="site-note-dismiss" onClick={dismiss}>
          {t('cookie.dismiss')}
        </button>
      </div>
    </div>
  );
}
