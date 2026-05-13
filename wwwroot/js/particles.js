function initParticles() {
  const container = document.getElementById('particles-container');
  if (!container) return;

  const icons = [
    '<path d="M12 5v14M5 12h14"/>',
    '<path d="M19 14c1.49-1.46 3-3.21 3-5.5A5.5 5.5 0 0 0 16.5 3c-1.76 0-3 .5-4.5 2-1.5-1.5-2.74-2-4.5-2A5.5 5.5 0 0 0 2 8.5c0 2.3 1.5 4.05 3 5.5l7 7Z"/>',
    '<rect x="2" y="7" width="20" height="10" rx="5" ry="5"/>',
    '<path d="M4.5 16.5c1.5 1.5 3 1.5 4.5 0s3-1.5 4.5 0 3 1.5 4.5 0"/>',
  ];

  function createParticle() {
    const particle = document.createElement('div');
    particle.className = 'particle';
    const size = Math.random() * 30 + 15;
    const left = Math.random() * 100;
    const duration = Math.random() * 12 + 8;
    const delay = Math.random() * 10;
    const icon = icons[Math.floor(Math.random() * icons.length)];
    particle.style.width = `${size}px`;
    particle.style.height = `${size}px`;
    particle.style.left = `${left}%`;
    particle.style.animationDuration = `${duration}s`;
    particle.style.animationDelay = `-${delay}s`;
    particle.innerHTML = `
      <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
        ${icon}
      </svg>`;
    container.appendChild(particle);
    setTimeout(() => {
      particle.remove();
      createParticle();
    }, (duration + delay) * 1000);
  }
  for (let i = 0; i < 15; i++) {
    setTimeout(createParticle, i * 300);
  }
}

document.addEventListener('DOMContentLoaded', initParticles);
