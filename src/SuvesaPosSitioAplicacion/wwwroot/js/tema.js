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

// Mantiene visible el encabezado que el usuario acaba de desplegar. El menú es
// el único contenedor que se mueve y solo lo hace si el elemento quedó fuera de
// su área visible; no altera el desplazamiento de la pantalla de trabajo.
export function desplazarMenuAVista(elementoOId) {
    const elemento = typeof elementoOId === 'string'
        ? document.getElementById(elementoOId)
        : elementoOId;
    const lista = elemento?.closest?.('.seepos-lista-menu');

    if (!elemento || !lista) {
        return;
    }

    const limite = lista.getBoundingClientRect();
    const item = elemento.getBoundingClientRect();
    const margen = 12;
    let destino = lista.scrollTop;

    if (item.bottom > limite.bottom - margen) {
        destino += item.bottom - limite.bottom + margen;
    } else if (item.top < limite.top + margen) {
        destino -= limite.top - item.top + margen;
    }

    if (destino !== lista.scrollTop) {
        lista.scrollTo({ top: destino, behavior: 'smooth' });
    }
}
