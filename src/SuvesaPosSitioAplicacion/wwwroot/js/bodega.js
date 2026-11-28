// Firma de recibido (dibujo 100 % en el navegador: Blazor Server no aguanta
// un roundtrip por cada trazo) y avisos de la cola de piso.

export function conectarFirma(canvas) {
    if (!(canvas instanceof HTMLCanvasElement) || canvas.dataset.seeposFirma === '1') return;
    const ctx = canvas.getContext('2d');
    if (!ctx) return;
    const dpr = window.devicePixelRatio || 1;
    const rect = canvas.getBoundingClientRect();
    canvas.width = Math.max(1, Math.floor(rect.width * dpr));
    canvas.height = Math.max(1, Math.floor(rect.height * dpr));
    ctx.scale(dpr, dpr);
    ctx.lineWidth = 2.2;
    ctx.lineCap = 'round';
    ctx.lineJoin = 'round';
    ctx.strokeStyle = getComputedStyle(canvas).color || '#111';
    let dibujando = false;

    const pos = (ev) => {
        const r = canvas.getBoundingClientRect();
        const t = ev.touches ? ev.touches[0] : ev;
        return { x: t.clientX - r.left, y: t.clientY - r.top };
    };
    const start = (ev) => {
        ev.preventDefault();
        dibujando = true;
        const p = pos(ev);
        ctx.beginPath();
        ctx.moveTo(p.x, p.y);
        canvas.dataset.seeposTinta = '1';
    };
    const move = (ev) => {
        if (!dibujando) return;
        ev.preventDefault();
        const p = pos(ev);
        ctx.lineTo(p.x, p.y);
        ctx.stroke();
    };
    const end = () => { dibujando = false; };

    canvas.addEventListener('pointerdown', start);
    canvas.addEventListener('pointermove', move);
    canvas.addEventListener('pointerup', end);
    canvas.addEventListener('pointerleave', end);
    canvas.addEventListener('touchstart', start, { passive: false });
    canvas.addEventListener('touchmove', move, { passive: false });
    canvas.addEventListener('touchend', end);
    canvas.dataset.seeposFirma = '1';
}

export function limpiarFirma(canvas) {
    if (!(canvas instanceof HTMLCanvasElement)) return;
    const ctx = canvas.getContext('2d');
    if (!ctx) return;
    ctx.save();
    ctx.setTransform(1, 0, 0, 1, 0, 0);
    ctx.clearRect(0, 0, canvas.width, canvas.height);
    ctx.restore();
    delete canvas.dataset.seeposTinta;
}

export function pngFirma(canvas) {
    if (!(canvas instanceof HTMLCanvasElement) || canvas.dataset.seeposTinta !== '1') return null;
    return canvas.toDataURL('image/png');
}

export function avisarNuevo() {
    try {
        const ac = new AudioContext();
        const o = ac.createOscillator();
        const g = ac.createGain();
        o.type = 'sine';
        o.frequency.value = 880;
        g.gain.value = 0.07;
        o.connect(g);
        g.connect(ac.destination);
        o.start();
        o.stop(ac.currentTime + 0.16);
    } catch {
        // Sin audio no se corta el aviso visual.
    }
}

export function abrir(url) {
    window.open(url, '_blank', 'noopener');
}
