#!/usr/bin/env python3
"""
Anota Class/MenuSeePos.cs con `Codigo = "..."` en cada nodo, usando EXACTAMENTE el
mismo algoritmo de slug que la semilla del API
(DevSuvesaPosWeb/ApiSuvesaPos/tools/generar_semilla_seguridad.py). Asi el menu del
sitio y el catalogo de seguridad casan por codigo y no por rotulo.

  python3 tools/anotar_codigos_menu.py [ruta/a/MenuSeePos.cs]

Idempotente: quita los `Codigo = "..."` que hubiera y los vuelve a poner.
"""

import os
import re
import sys
import unicodedata


def slug(texto):
    d = unicodedata.normalize("NFKD", texto)
    d = "".join(c for c in d if not unicodedata.combining(c))
    d = re.sub(r"[^A-Za-z0-9]+", "_", d)
    return d.strip("_").upper()


# --- parser minimo (igual estructura que el del API) ----------------------
_WS = re.compile(r"\s*")
_KEY = re.compile(r"(\w+)\s*=\s*")
_STR = re.compile(r'"((?:[^"\\]|\\.)*)"')
_ARR = re.compile(r"new\s+ItemMenu\s*\[\s*\]\s*")
_NEW = re.compile(r"new\s+ItemMenu\b")


def _strip_comments(s):
    s = re.sub(r"/\*.*?\*/", "", s, flags=re.S)
    return re.sub(r"//[^\n]*", "", s)


def _parse_list(s, i):
    items = []
    i += 1
    while True:
        i = _WS.match(s, i).end()
        if s[i] == "}":
            return items, i + 1
        m = _NEW.match(s, i)
        if not m:
            raise ValueError("esperaba 'new ItemMenu':\n" + s[i:i + 60])
        i = _WS.match(s, m.end()).end()
        node, i = _parse_node(s, i)
        items.append(node)
        i = _WS.match(s, i).end()
        if s[i] == ",":
            i += 1


def _parse_node(s, i):
    if s[i] != "{":
        raise ValueError("esperaba '{':\n" + s[i:i + 60])
    i += 1
    node = {"titulo": None, "hijos": []}
    while True:
        i = _WS.match(s, i).end()
        if s[i] == "}":
            return node, i + 1
        km = _KEY.match(s, i)
        if not km:
            raise ValueError("esperaba 'Clave =':\n" + s[i:i + 60])
        key = km.group(1)
        i = km.end()
        if key == "Hijos":
            am = _ARR.match(s, i)
            i = am.end()
            node["hijos"], i = _parse_list(s, i)
        else:
            sm = _STR.match(s, i)
            if key == "Titulo":
                node["titulo"] = sm.group(1)
            i = sm.end()
        i = _WS.match(s, i).end()
        if s[i] == ",":
            i += 1


def preorden_codigos(raices):
    """Lista de codigos en el mismo orden que aparecen los `Titulo =` en el fichero."""
    codigos = []

    def visita(nodo, mcod, cadena):
        if cadena is None:  # es una raiz
            mcod = slug(nodo["titulo"])
            # Raiz-hoja (Modulo Inventario/Reportes/Farmacia): usa la funcion "espejo"
            # <MODULO>.<MODULO>, que es la grantable en el catalogo del API. Una raiz
            # con hijos solo agrupa: su codigo es el del modulo (uso decorativo).
            codigos.append(mcod + "." + mcod if not nodo["hijos"] else mcod)
            for h in nodo["hijos"]:
                visita(h, mcod, [h["titulo"]])
        else:
            codigos.append(mcod + "." + ".".join(slug(t) for t in cadena))
            for h in nodo["hijos"]:
                visita(h, mcod, cadena + [h["titulo"]])

    for r in raices:
        visita(r, None, None)
    return codigos


def main():
    aqui = os.path.dirname(os.path.abspath(__file__))
    defecto = os.path.join(aqui, "..", "src", "SuvesaPosSitioAplicacion", "Class", "MenuSeePos.cs")
    ruta = sys.argv[1] if len(sys.argv) > 1 else defecto
    if not os.path.exists(ruta):
        print("ERROR: no existe " + ruta, file=sys.stderr)
        sys.exit(2)

    texto = open(ruta, encoding="utf-8").read()

    # 1) quitar Codigo previos (para ser idempotente)
    texto = re.sub(r"^[ \t]*Codigo = \"[^\"]*\",\r?\n", "", texto, flags=re.M)

    # 2) parsear estructura (sin comentarios) y calcular codigos en pre-orden
    limpio = _strip_comments(texto)
    m = re.search(r"Items\s*=\s*new\s+ItemMenu\s*\[\s*\]\s*", limpio)
    inicio = limpio.index("{", m.end())
    raices, _ = _parse_list(limpio, inicio)
    codigos = preorden_codigos(raices)

    # 3) insertar `Codigo = "..."` tras cada linea `Titulo = "...",`
    titulo_re = re.compile(r'^([ \t]*)Titulo = "((?:[^"\\]|\\.)*)",[ \t]*\r?$', re.M)
    matches = list(titulo_re.finditer(texto))
    if len(matches) != len(codigos):
        print("ERROR: {} lineas Titulo vs {} codigos".format(len(matches), len(codigos)), file=sys.stderr)
        sys.exit(1)

    salida = []
    pos = 0
    for m, cod in zip(matches, codigos):
        salida.append(texto[pos:m.end()])
        salida.append("\n{}Codigo = \"{}\",".format(m.group(1), cod))
        pos = m.end()
    salida.append(texto[pos:])
    nuevo = "".join(salida)

    with open(ruta, "w", encoding="utf-8") as fh:
        fh.write(nuevo)

    dups = {c for c in codigos if codigos.count(c) > 1}
    if dups:
        print("AVISO: codigos duplicados: " + ", ".join(sorted(dups)), file=sys.stderr)
    print("OK  {} nodos anotados en {}".format(len(codigos), os.path.relpath(ruta, os.path.join(aqui, ".."))))


if __name__ == "__main__":
    main()
