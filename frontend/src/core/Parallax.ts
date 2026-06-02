export class ParallaxEffect {
  constructor(private elementId: string, private speed: number = 0.2) {}

  public init(): void {
    const element = document.getElementById(this.elementId);
    if (!element) return;

    window.addEventListener('scroll', () => {
      const scroll = window.pageYOffset;
      element.style.transform = `translateY(${scroll * this.speed}px)`;
    }, { passive: true });
  }
}
