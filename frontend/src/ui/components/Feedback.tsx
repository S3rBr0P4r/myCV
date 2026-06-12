import { useState, useCallback, useEffect, useRef } from 'react';
import { useTranslation } from '../hooks/useTranslation';

const STARS = [1, 2, 3, 4, 5];
const TOAST_DURATION = 5000;

function getCountry(): string {
  try {
    const regions = new Intl.DisplayNames(navigator.languages, { type: 'region' });
    const parts = new Intl.Locale(navigator.language).maximize();
    return parts.region ? regions.of(parts.region) ?? 'Unknown' : 'Unknown';
  } catch {
    return 'Unknown';
  }
}

export function Feedback() {
  const { t } = useTranslation();
  const [open, setOpen] = useState(false);
  const [name, setName] = useState('');
  const [comment, setComment] = useState('');
  const [rating, setRating] = useState(0);
  const [hover, setHover] = useState(0);
  const [toast, setToast] = useState('');
  const [error, setError] = useState('');
  const toastTimer = useRef<ReturnType<typeof setTimeout>>();

  const country = getCountry();

  useEffect(() => {
    return () => clearTimeout(toastTimer.current);
  }, []);

  const showToast = useCallback((message: string) => {
    setToast(message);
    clearTimeout(toastTimer.current);
    toastTimer.current = setTimeout(() => setToast(''), TOAST_DURATION);
  }, []);

  const handleSubmit = useCallback(async () => {
    const trimmed = name.trim();
    if (!trimmed) {
      setError(t('feedback.requiredName'));
      return;
    }
    if (rating < 1 || rating > 5) {
      setError(t('feedback.requiredRating'));
      return;
    }
    setError('');

    try {
      const origin = window.location.origin;
      const resp = await fetch('/api/v1/feedback', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json', Origin: origin },
        body: JSON.stringify({ country, rating, name: trimmed, comment: comment.trim() }),
      });
      if (!resp.ok) throw new Error();
      setOpen(false);
      setName('');
      setComment('');
      setRating(0);
      setHover(0);
      showToast(t('feedback.thankYou'));
    } catch {
      setError(t('feedback.error'));
    }
  }, [name, comment, rating, country, t, showToast]);

  const reset = useCallback(() => {
    setOpen(false);
    setName('');
    setComment('');
    setRating(0);
    setHover(0);
    setError('');
  }, []);

  return (
    <>
      {open && (
        <div className="feedback-overlay" onClick={reset}>
          <div className="feedback-modal" onClick={e => e.stopPropagation()}>
            <h2>{t('feedback.title')}</h2>
            <p>{t('feedback.subtitle')}</p>

            <div className="feedback-form">
              <div className="feedback-field">
                <label>{t('feedback.nameLabel')}</label>
                <input
                  type="text"
                  value={name}
                  onChange={e => setName(e.target.value)}
                />
              </div>

              <div className="feedback-field">
                <label>{t('feedback.ratingLabel')}</label>
                <div className="feedback-stars">
                  {STARS.map(s => (
                    <button
                      key={s}
                      type="button"
                      className={`feedback-star${s <= (hover || rating) ? ' active' : ''}`}
                      onClick={() => setRating(s)}
                      onMouseEnter={() => setHover(s)}
                      onMouseLeave={() => setHover(0)}
                      aria-label={`${s} star${s > 1 ? 's' : ''}`}
                    >
                      {'\u2605'}
                    </button>
                  ))}
                </div>
              </div>

              <div className="feedback-field">
                <label>{t('feedback.commentLabel')}</label>
                <textarea
                  className="feedback-textarea"
                  value={comment}
                  onChange={e => setComment(e.target.value)}
                  rows={3}
                />
              </div>

              {error && <p className="feedback-error">{error}</p>}

              <button className="feedback-submit" onClick={handleSubmit}>
                {t('feedback.send')}
              </button>
            </div>
          </div>
        </div>
      )}

      {!open && (
        <button
          className="feedback-fab"
          onClick={() => setOpen(true)}
          aria-label={t('feedback.fab')}
        >
          <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
            <path d="M21 15a2 2 0 0 1-2 2H7l-4 4V5a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2z" />
            <line x1="8" y1="10" x2="16" y2="10" />
            <line x1="12" y1="7" x2="12" y2="13" />
          </svg>
          <span className="feedback-fab-tooltip">{t('feedback.fab')}</span>
        </button>
      )}

      {toast && (
        <div className="feedback-toast" role="status">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round">
            <polyline points="20 6 9 17 4 12" />
          </svg>
          {toast}
        </div>
      )}
    </>
  );
}
