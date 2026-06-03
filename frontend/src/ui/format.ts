export function renderFormattedText(text: string): (Text | HTMLElement)[] {
  const parts: (Text | HTMLElement)[] = [];
  const regex = /\*\*(.+?)\*\*/g;
  let last = 0;
  let match: RegExpExecArray | null;

  while ((match = regex.exec(text)) !== null) {
    if (match.index > last) {
      parts.push(document.createTextNode(text.slice(last, match.index)));
    }
    const strong = document.createElement('strong');
    strong.appendChild(document.createTextNode(match[1]));
    parts.push(strong);
    last = regex.lastIndex;
  }

  if (last < text.length) {
    parts.push(document.createTextNode(text.slice(last)));
  }

  return parts;
}
