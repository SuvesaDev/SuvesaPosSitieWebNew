// Puente con la SPA React durante la convivencia.
//
// Deliberadamente estrecho: sesion, ocultar el cromo, y avisar de la navegacion.
// Cada cosa que se anada aqui es acoplamiento entre dos mundos que se supone
// temporal, y que despues hay que desmontar. Se borra entero en la Ola 6.
//
// Funciona porque el iframe se sirve desde el MISMO origen a traves de YARP:
// eso permite compartir localStorage y tocar el documento de dentro sin
// postMessage para lo basico.

const OCULTAR_CROMO = `
  /* Barra superior de la SPA */
  nav.navbar.navbar-expand-sm.bg-dark.navbar-dark { display: none !important; }
  /* Menu lateral de la SPA */
  nav.vet_nav-menu { display: none !important; }
  /* Barra de pestanas de la SPA */
  ul:has(> li.Tabs_li) { display: none !important; }
  /* Sin el cromo sobra el margen superior que dejaba */
  body { padding-top: 0 !important; }
`;

/**
 * Deja la sesion donde la SPA la busca. Se llama ANTES de crear el iframe,
 * porque React lee localStorage al arrancar.
 */
export function sembrarSesion(token, idSucursal, centro) {
    try {
        localStorage.setItem('auth', JSON.stringify({ token }));
        if (centro) {
            localStorage.setItem('centro', centro);
        }
        if (idSucursal) {
            localStorage.setItem('idSurcursal', String(idSucursal));
        }
    } catch (e) {
        console.warn('SeePOS: no se pudo sembrar la sesion del legado', e);
    }
}

/** Oculta el cromo de la SPA en cuanto su documento este listo. */
export function prepararMarco(marco) {
    if (!marco) {
        return;
    }

    const aplicar = () => {
        try {
            const doc = marco.contentDocument;
            if (!doc) {
                return;
            }

            if (!doc.getElementById('seepos-oculta-cromo')) {
                const estilo = doc.createElement('style');
                estilo.id = 'seepos-oculta-cromo';
                estilo.textContent = OCULTAR_CROMO;
                doc.head.appendChild(estilo);
            }
        } catch (e) {
            // Distinto origen: el iframe se ve entero, con su propio cromo.
            console.warn('SeePOS: el legado no es del mismo origen; no se pudo ocultar su cromo', e);
        }
    };

    marco.addEventListener('load', aplicar);
    aplicar();
}

/** Limpia lo sembrado. Se llama al cerrar sesion. */
export function limpiarSesion() {
    try {
        localStorage.removeItem('auth');
        localStorage.removeItem('centro');
        localStorage.removeItem('idSurcursal');
        localStorage.removeItem('tabs');
    } catch {
        // Nada que hacer.
    }
}
