import { createElement, type ReactNode } from 'react';

function renderBold(text: string): ReactNode {
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

export function renderFormattedText(text: string): ReactNode {
  const parts: ReactNode[] = [];
  const regex = /\[([^\]]*)\]\(([^)]*)\)/g;
  let last = 0;
  let match: RegExpExecArray | null;

  while ((match = regex.exec(text)) !== null) {
    if (match.index > last) {
      const boldPart = renderBold(text.slice(last, match.index));
      if (boldPart !== null) {
        if (Array.isArray(boldPart)) {
          parts.push(...boldPart);
        } else {
          parts.push(boldPart);
        }
      }
    }
    const linkText = match[1];
    const linkUrl = match[2];
    const linkContent = renderBold(linkText);
    parts.push(
      createElement('a', { key: parts.length, href: linkUrl, rel: 'noopener noreferrer', target: '_blank' }, linkContent),
    );
    last = regex.lastIndex;
  }

  if (last < text.length) {
    const boldPart = renderBold(text.slice(last));
    if (boldPart !== null) {
      if (Array.isArray(boldPart)) {
        parts.push(...boldPart);
      } else {
        parts.push(boldPart);
      }
    }
  }

  if (parts.length === 0) return null;
  if (parts.length === 1) return parts[0];
  return parts;
}
