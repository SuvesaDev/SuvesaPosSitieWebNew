// Conmutador de tema. Bootstrap 5.3 lee data-bs-theme del elemento raiz.
// La eleccion se recuerda por navegador; si no hay ninguna, manda el sistema.

export function alternar() {
    const raiz = document.documentElement;
    const oscuro = raiz.getAttribute('data-bs-theme') !== 'dark';

    raiz.setAttribute('data-bs-theme', oscuro ? 'dark' : 'light');

    try {
        localStorage.setItem('seepos.tema', oscuro ? 'dark' : 'light');
    } catch {
        // Sin almacenamiento, el cambio dura lo que la pagina.
    }

    return oscuro;
}

export function esOscuro() {
    return document.documentElement.getAttribute('data-bs-theme') === 'dark';
}

export function esPantallaPequena() {
    return window.matchMedia('(max-width: 991.98px)').matches;
}
