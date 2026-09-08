#!/usr/bin/env python3
"""Sanea el OpenAPI que emite el API de SeePOS antes de generar el cliente.

Problema conocido: las propiedades decimales llegan con
`maximum: 1.7976931348623157e+308` (double.MaxValue), que no cabe en un
System.Decimal y hace fallar al generador. Se eliminan esos limites, que
ademas no aportan nada: son el rango del tipo, no una regla de negocio.
"""
import json
import sys

DECIMAL_MAX = 79228162514264337593543950335
LIMITES = ("maximum", "minimum", "exclusiveMaximum", "exclusiveMinimum")

quitados = 0


def limpiar(nodo):
    global quitados
    if isinstance(nodo, dict):
        for clave in LIMITES:
            valor = nodo.get(clave)
            if isinstance(valor, (int, float)) and not isinstance(valor, bool):
                if abs(valor) > DECIMAL_MAX:
                    del nodo[clave]
                    quitados += 1
        for valor in nodo.values():
            limpiar(valor)
    elif isinstance(nodo, list):
        for valor in nodo:
            limpiar(valor)


def main():
    origen, destino = sys.argv[1], sys.argv[2]
    with open(origen, encoding="utf-8") as f:
        doc = json.load(f)
    limpiar(doc)
    with open(destino, "w", encoding="utf-8") as f:
        json.dump(doc, f, ensure_ascii=False, indent=1)
    print(f"  limites fuera de rango eliminados: {quitados}")


if __name__ == "__main__":
    main()
