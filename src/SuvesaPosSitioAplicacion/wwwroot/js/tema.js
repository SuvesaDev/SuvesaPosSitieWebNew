// Conmutador de tema. Bootstrap 5.3 lee data-bs-theme del elemento raiz.
// La eleccion se recuerda por navegador; si no hay ninguna, manda el sistema.

// Selecciona todo el contenido de un campo numerico al enfocarlo. Varias
// pantallas de alta (existencias, cantidades) arrancan esos campos en "0"
// real, no vacio; sin esto, el usuario tiene que borrar el cero a mano antes
// de escribir el valor de verdad. Delegado en document: sigue funcionando
// aunque Blazor reemplace el DOM, sin tener que reengancharlo por pantalla.
document.addEventListener('focusin', (evento) => {
    if (evento.target instanceof HTMLInputElement && evento.target.type === 'number') {
        evento.target.select();
    }
});

// Copia texto al portapapeles (claves fiscales, codigos largos de leer a mano).
// Devuelve si funciono para que quien llama decida si avisar del error.
export async function copiarTexto(texto) {
    try {
        await navigator.clipboard.writeText(texto);
        return true;
    } catch {
        return false;
    }
}

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
        // Sin animacion: un scroll "smooth" que queda a mitad de camino cuando
        // Blazor vuelve a parchear el DOM justo despues (p. ej. al alternar otra
        // rama enseguida) deja al navegador pintando un cuadro en blanco aunque
        // el DOM ya sea correcto - visto tanto en Chrome real como en Playwright.
        lista.scrollTo({ top: destino, behavior: 'instant' });
    }
}
