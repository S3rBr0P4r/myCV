import { createElement, type ReactNode } from 'react';

export function renderFormattedText(text: string): ReactNode {
  const parts: ReactNode[] = [];
  const regex = /\*\*(.+?)\*\*/g;
  let last = 0;
  let match: RegExpExecArray | null;

  while ((match = regex.exec(text)) !== null) {
    if (match.index > last) {
      parts.push(text.slice(last, match.index));
    }
    parts.push(createElement('strong', { key: parts.length }, match[1]));
    last = regex.lastIndex;
  }

  if (last < text.length) {
    parts.push(text.slice(last));
  }

  if (parts.length === 0) return null;
  if (parts.length === 1) return parts[0];
  return parts;
}
