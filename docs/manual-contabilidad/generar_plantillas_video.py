# -*- coding: utf-8 -*-
"""Capacitación: crear plantillas, versionar, simular y activar."""
import asyncio
import subprocess
import sys
from pathlib import Path

from PIL import Image, ImageDraw

BASE = Path("/Users/amartinez/Downloads/Git/SuvesaPosSitieWebNew/docs/manual-contabilidad")
sys.path.insert(0, str(BASE))
import generar_leccion as ui

ui.CLIPS = BASE / "video" / "plantillas"
ui.CLIPS.mkdir(parents=True, exist_ok=True)

AZUL, VERDE, VERDE_SUAVE = ui.AZUL, ui.VERDE, ui.VERDE_SUAVE
TEXTO, GRIS, LINEA, BLANCO = ui.TEXTO, ui.GRIS, ui.LINEA, ui.BLANCO


def caja(titulo, lineas, ancho=860, alto=None):
    alto = alto or 70 + len(lineas) * 42
    im = Image.new("RGBA", (ancho, alto), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    d.rounded_rectangle((0, 0, ancho - 8, alto - 8), radius=16, fill=BLANCO + (255,), outline=LINEA + (255,), width=2)
    d.text((24, 16), titulo, font=ui.f_big, fill=TEXTO + (255,))
    y = 70
    for texto, color in lineas:
        d.text((24, y), texto, font=ui.f_sub, fill=color + (255,))
        y += 40
    return im


def fila(celdas, anchos, alto=52):
    im = Image.new("RGBA", (sum(anchos), alto), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    x = 0
    for texto, ancho in zip(celdas, anchos):
        d.text((x + 8, 10), texto, font=ui.f_num, fill=TEXTO + (255,))
        x += ancho
    return im


def escena_idea(t):
    im, d = ui.lienzo("Plantillas", "La receta de un asiento, y sus versiones")
    ui.pegar(im, caja("Plantilla", [
        ("Es el tipo de documento.", TEXTO),
        ("Ejemplo: VentaEmitida.", TEXTO),
        ("Se crea una sola vez.", GRIS),
    ], 860, 230), ui.ease((t - 0.2) / 0.35), (80, 230))
    ui.pegar(im, caja("Versión", [
        ("Son las cuentas y las fórmulas.", TEXTO),
        ("Nace en borrador.", TEXTO),
        ("Solo la activa contabiliza.", VERDE),
    ], 860, 230), ui.ease((t - 0.8) / 0.35), (980, 230))
    if t > 1.6:
        nota = ui.texto_rgba("Una plantilla puede tener varias versiones. Una sola queda activa.", ui.f_sub, TEXTO, 1500)
        ui.pegar(im, nota, ui.ease((t - 1.6) / 0.3), (120, 520))
    d = ImageDraw.Draw(im)
    d.text((64, ui.H - 52), "Menú Contabilidad → Plantillas. Elija empresa y emisor.", font=ui.f_pie, fill=BLANCO)
    return im


def escena_crear(t):
    im, d = ui.lienzo("Crear la plantilla", "Botón Nueva plantilla")
    ui.tarjeta(d, 280, 220, 1360, 560)
    d.text((340, 260), "Nueva plantilla", font=ui.f_big, fill=TEXTO)
    ui.pegar(im, caja("Tipo de evento", [("AjusteInventario", TEXTO)], 1180, 130), ui.ease((t - 0.4) / 0.3), (340, 340))
    ui.pegar(im, caja("Descripción", [("Ajuste de inventario por toma física.", TEXTO)], 1180, 130), ui.ease((t - 1.0) / 0.3), (340, 490))
    if t > 1.7:
        ui.pegar(im, ui.texto_rgba("Guardar. La plantilla queda en la lista, todavía sin versión activa.", ui.f_sub, VERDE, 1200), ui.ease((t - 1.7) / 0.3), (340, 660))
    d = ImageDraw.Draw(im)
    d.text((64, ui.H - 52), "El tipo de evento tiene que ser el nombre que usa el sistema, por ejemplo VentaEmitida o AjusteInventario.", font=ui.f_pie, fill=BLANCO)
    return im


def escena_versiones(t):
    im, d = ui.lienzo("Ver las versiones", "Pulse Versiones en la fila de la plantilla")
    ui.tarjeta(d, 120, 210, 1680, 640)
    d.text((170, 240), "Versiones de \"VentaEmitida\"", font=ui.f_big, fill=TEXTO)
    encabezado = ["Versión", "Estado", "Desde", "Acciones"]
    anchos = [220, 320, 280, 700]
    ui.pegar(im, fila(encabezado, anchos), 1, (170, 320))
    filas = [
        (["1", "Reemplazada", "2026-09-01", "Detalle    Simular"], 0.4),
        (["2", "Activa", "2026-09-15", "Detalle    Simular"], 0.8),
        (["3", "Borrador", "2026-09-30", "Detalle    Simular    Activar"], 1.2),
    ]
    for i, (celdas, retardo) in enumerate(filas):
        ui.pegar(im, fila(celdas, anchos), ui.ease((t - retardo) / 0.3), (170, 390 + i * 80))
    if t > 2.0:
        ui.pegar(im, ui.texto_rgba("Nueva versión arma la siguiente. El sistema la numera sola.", ui.f_sub, AZUL, 1400), ui.ease((t - 2.0) / 0.3), (170, 680))
    d = ImageDraw.Draw(im)
    d.text((64, ui.H - 52), "Reemplazada ya no se usa. Activa es la que contabiliza. Borrador espera su revisión.", font=ui.f_pie, fill=BLANCO)
    return im


def escena_lineas(t):
    im, d = ui.lienzo("Armar la versión", "Nueva versión: fechas, cuentas y fórmulas")
    ui.tarjeta(d, 80, 200, 1760, 680)
    d.text((120, 230), "Prioridad  1", font=ui.f_sub, fill=TEXTO)
    d.text((420, 230), "Efectiva desde   30/09/2026", font=ui.f_sub, fill=TEXTO)
    d.text((980, 230), "Efectiva hasta   opcional", font=ui.f_sub, fill=GRIS)
    d.text((120, 300), "Líneas", font=ui.f_big, fill=TEXTO)
    anchos = [140, 200, 620, 520]
    ui.pegar(im, fila(["Orden", "Movimiento", "Cuenta", "Fórmula"], anchos), 1, (120, 370))
    lineas = [
        ["1", "Débito", "1.1.4.1  Cuentas por cobrar", "Total"],
        ["2", "Débito", "4.1.3  Descuentos", "Descuento"],
        ["3", "Crédito", "4.1.2  Ventas gravadas", "SubTotal"],
        ["4", "Crédito", "2.1.3.1  IVA devengado", "ImpVenta"],
    ]
    for i, celdas in enumerate(lineas):
        ui.pegar(im, fila(celdas, anchos), ui.ease((t - 0.5 - i * 0.28) / 0.28), (120, 440 + i * 70))
    if t > 2.2:
        ui.pegar(im, ui.texto_rgba("Cada línea pide cuenta y fórmula. Guardar deja la versión en borrador.", ui.f_sub, VERDE, 1400), ui.ease((t - 2.2) / 0.3), (120, 760))
    d = ImageDraw.Draw(im)
    d.text((64, ui.H - 52), "La fórmula usa los importes del documento: Total, SubTotal, Descuento, ImpVenta.", font=ui.f_pie, fill=BLANCO)
    return im


def escena_simular(t):
    im, d = ui.lienzo("Simular", "Pruebe la versión antes de activarla")
    ui.tarjeta(d, 80, 210, 820, 620)
    d.text((120, 250), "Payload de prueba", font=ui.f_big, fill=TEXTO)
    ejemplo = ["Total        113000", "SubTotal     100000", "Descuento         0", "ImpVenta      13000"]
    for i, linea in enumerate(ejemplo):
        ui.pegar(im, ui.texto_rgba(linea, ui.f_num, TEXTO, 700), ui.ease((t - 0.3 - i * 0.12) / 0.2), (140, 340 + i * 56))
    ui.tarjeta(d, 960, 210, 880, 620)
    d.text((1000, 250), "Resultado", font=ui.f_big, fill=TEXTO)
    resultados = [
        ("Débito   1.1.4.1", "113.000,00"),
        ("Crédito  4.1.2", "100.000,00"),
        ("Crédito  2.1.3.1", "13.000,00"),
    ]
    for i, (izq, der) in enumerate(resultados):
        capa = Image.new("RGBA", (760, 56), (0, 0, 0, 0))
        cd = ImageDraw.Draw(capa)
        cd.text((0, 8), izq, font=ui.f_num, fill=TEXTO + (255,))
        cd.text((480, 8), der, font=ui.f_num, fill=TEXTO + (255,))
        ui.pegar(im, capa, ui.ease((t - 1.1 - i * 0.2) / 0.25), (1020, 340 + i * 70))
    if t > 2.0:
        sello = Image.new("RGBA", (280, 64), (0, 0, 0, 0))
        sd = ImageDraw.Draw(sello)
        sd.rounded_rectangle((0, 0, 240, 56), radius=16, fill=VERDE + (255,))
        sd.text((58, 12), "Cuadra", font=ui.f_big, fill=BLANCO + (255,))
        ui.pegar(im, sello, ui.ease((t - 2.0) / 0.3), (1020, 580))
    d = ImageDraw.Draw(im)
    d.text((64, ui.H - 52), "La línea de descuento da cero y no se escribe. Si no cuadra, corrija la fórmula.", font=ui.f_pie, fill=BLANCO)
    return im


def escena_activar(t):
    im, d = ui.lienzo("Activar", "Solo entonces los documentos usan esta versión")
    pasos = [
        ("1", "La versión está en Borrador."),
        ("2", "Pulse Activar y confirme."),
        ("3", "Pasa a Activa."),
        ("4", "La activa anterior queda Reemplazada."),
    ]
    for i, (n, texto) in enumerate(pasos):
        capa = Image.new("RGBA", (1600, 90), (0, 0, 0, 0))
        cd = ImageDraw.Draw(capa)
        cd.rounded_rectangle((0, 0, 1560, 78), radius=14, fill=BLANCO + (255,), outline=LINEA + (255,), width=2)
        cd.ellipse((16, 14, 64, 62), fill=AZUL + (255,))
        cd.text((32, 22), n, font=ui.f_num, fill=BLANCO + (255,))
        cd.text((90, 22), texto, font=ui.f_num, fill=TEXTO + (255,))
        ui.pegar(im, capa, ui.ease((t - i * 0.35) / 0.3), (140, 230 + i * 120))
    d = ImageDraw.Draw(im)
    d.text((64, ui.H - 52), "Los asientos ya hechos no cambian. Lo nuevo usa la versión activa.", font=ui.f_pie, fill=BLANCO)
    return im


ESCENAS = [
    ("01", escena_idea,
     "En Plantillas se decide cómo se arma cada asiento. Una plantilla es el tipo de documento, por ejemplo Venta emitida. La versión es el detalle: qué cuentas y qué fórmulas. La plantilla se crea una vez. Puede tener varias versiones, y solo una queda activa. Mientras la versión esté en borrador, el documento se guarda y no hay póliza. Entre a Contabilidad, Plantillas, y elija la empresa y el emisor."),
    ("02", escena_crear,
     "Para un tipo nuevo, pulse Nueva plantilla. Escriba el tipo de evento con el nombre que usa el sistema, por ejemplo AjusteInventario, y una descripción. Guarde. La plantilla aparece en la lista. Todavía no contabiliza, porque no tiene una versión activa. Las plantillas del catálogo inicial, como la venta y la compra, ya vienen creadas. No hace falta volver a crearlas."),
    ("03", escena_versiones,
     "En la fila de la plantilla pulse Versiones. Verá el número, el estado y la fecha desde la que rige. Reemplazada es una versión vieja. Activa es la que está contabilizando. Borrador es la que todavía se puede revisar. En cada fila están Detalle, Simular y, si es borrador, Activar. Nueva versión arma la siguiente. El sistema le pone el número: uno, dos, tres."),
    ("04", escena_lineas,
     "Al crear la versión se indica la prioridad, desde cuándo rige y, si quiere, hasta cuándo. Después van las líneas. Cada línea tiene orden, débito o crédito, una cuenta que admita movimiento, y una fórmula. En la venta, el total va a cuentas por cobrar, el subtotal a ventas y el impuesto al IVA. El descuento usa la fórmula Descuento. Cada línea necesita cuenta y fórmula. Al guardar, la versión queda en borrador."),
    ("05", escena_simular,
     "Antes de activar, pulse Simular. Escriba importes de prueba, por ejemplo un total de ciento trece mil, un subtotal de cien mil y un impuesto de trece mil. Simular muestra cada línea con su monto. Si el descuento es cero, esa línea no aparece. El recuadro dice Cuadra cuando el debe iguala al haber. Si no cuadra, corrija la fórmula en una versión nueva. Simular no graba asientos."),
    ("06", escena_activar,
     "Cuando la simulación cuadra, pulse Activar y confirme. La versión pasa de borrador a activa. Si ya había otra activa de la misma plantilla, esa queda reemplazada. A partir de ese momento, los documentos de ese tipo usan estas cuentas. Los asientos que ya se habían hecho no se reescriben. Para cambiar las cuentas más adelante, cree otra versión, simúlela y actívela."),
]


async def main():
    for nombre, pintar, texto in ESCENAS:
        mp3 = ui.CLIPS / f"{nombre}.mp3"
        await ui.edge_tts.Communicate(texto, ui.VOZ, rate="-5%").save(str(mp3))
        r = subprocess.run(
            ["ffprobe", "-v", "error", "-show_entries", "format=duration", "-of", "csv=p=0", str(mp3)],
            capture_output=True, text=True, check=True)
        seg = float(r.stdout.strip())
        print(nombre, f"{seg:.1f}s", flush=True)
        ui.render(nombre, mp3, seg, pintar)
    lista = ui.CLIPS / "lista.txt"
    lista.write_text("".join(f"file '{n}.mp4'\n" for n, _, _ in ESCENAS), encoding="utf-8")
    final = BASE / "Capacitacion-Plantillas.mp4"
    subprocess.run(
        ["ffmpeg", "-y", "-loglevel", "error", "-f", "concat", "-safe", "0", "-i", str(lista), "-c", "copy", str(final)],
        check=True, cwd=ui.CLIPS)
    print(final)


if __name__ == "__main__":
    asyncio.run(main())
