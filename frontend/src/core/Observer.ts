export class ScrollObserver {
  private observer: IntersectionObserver;

  constructor(private selector: string, private activeClass: string = 'active') {
    this.observer = new IntersectionObserver(this.handleIntersect.bind(this), {
      threshold: 0.15,
      rootMargin: "0px 0px -100px 0px"
    });
  }

  private handleIntersect(entries: IntersectionObserverEntry[]): void {
    entries.forEach(entry => {
      if (entry.isIntersecting) {
        entry.target.classList.add(this.activeClass);
      }
    });
  }

  public observe(): void {
    const elements = document.querySelectorAll(this.selector);
    elements.forEach(el => this.observer.observe(el));
  }
}
