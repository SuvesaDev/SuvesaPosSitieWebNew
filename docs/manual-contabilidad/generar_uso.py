# -*- coding: utf-8 -*-
"""Capacitación de uso: el ciclo de Contabilidad en la interfaz. El cuadro no se mueve."""
import asyncio
import subprocess
import sys
from pathlib import Path

BASE = Path("/Users/amartinez/Downloads/Git/SuvesaPosSitieWebNew/docs/manual-contabilidad")
sys.path.insert(0, str(BASE))
import generar_leccion as ui

ui.CLIPS = BASE / "video" / "uso"
ui.CLIPS.mkdir(parents=True, exist_ok=True)

from PIL import Image, ImageDraw

AZUL, VERDE, VERDE_SUAVE = ui.AZUL, ui.VERDE, ui.VERDE_SUAVE
TEXTO, GRIS, LINEA, BLANCO, TIERRA = ui.TEXTO, ui.GRIS, ui.LINEA, ui.BLANCO, ui.TIERRA
ROJO = ui.ROJO


def chip(numero, titulo, detalle, ancho=860):
    im = Image.new("RGBA", (ancho, 96), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    d.rounded_rectangle((0, 0, ancho - 16, 84), radius=16, fill=BLANCO + (255,), outline=LINEA + (255,), width=2)
    d.ellipse((18, 16, 70, 68), fill=AZUL + (255,))
    d.text((numero < 10 and 34 or 28, 26), str(numero), font=ui.f_num, fill=BLANCO + (255,))
    d.text((92, 12), titulo, font=ui.f_num, fill=TEXTO + (255,))
    d.text((92, 48), detalle, font=ui.f_lbl, fill=GRIS + (255,))
    return im


def boton(texto, ancho, alto, fondo):
    im = Image.new("RGBA", (ancho, alto), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    d.rounded_rectangle((0, 0, ancho - 1, alto - 1), radius=12, fill=fondo + (255,))
    tw = d.textlength(texto, font=ui.f_big)
    d.text(((ancho - tw) / 2, 16), texto, font=ui.f_big, fill=BLANCO + (255,))
    return im


def insignia(texto, fondo):
    im = Image.new("RGBA", (280, 52), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    d.rounded_rectangle((0, 0, 250, 48), radius=22, fill=fondo + (255,))
    d.text((28, 8), texto, font=ui.f_num, fill=BLANCO + (255,))
    return im


def campo(etiqueta, valor, ancho=520):
    im = Image.new("RGBA", (ancho, 100), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    d.text((0, 0), etiqueta, font=ui.f_lbl, fill=GRIS + (255,))
    d.rounded_rectangle((0, 32, ancho - 8, 96), radius=12, outline=LINEA + (255,), width=2, fill=(250, 248, 244, 255))
    d.text((18, 48), valor, font=ui.f_num, fill=TEXTO + (255,))
    return im


def escena_ciclo(t):
    im, d = ui.lienzo("Cómo se usa", "El ciclo, en el orden de las pantallas")
    pasos = [
        (1, "Configuración emisor", "Encender la sucursal y crear el libro"),
        (2, "Catálogo de cuentas", "Revisar, editar o agregar cuentas"),
        (3, "Cuentas de uso", "Retención, dólares, tránsito y comisiones"),
        (4, "Ficha del proveedor", "El porcentaje de retención, si aplica"),
        (5, "Cierre", "Abrir el mes de trabajo"),
        (6, "Plantillas", "Revisar y activar cada receta"),
        (7, "Operación del día", "Vender, cobrar, comprar y pagar"),
        (8, "Bandeja y consultas", "Revisar, y al final cerrar el mes"),
    ]
    for i, (n, titulo, detalle) in enumerate(pasos):
        col, fila = i % 2, i // 2
        ui.pegar(im, chip(n, titulo, detalle), ui.ease((t - i * 0.28) / 0.35), (70 + col * 920, 210 + fila * 150))
    d = ImageDraw.Draw(im)
    d.text((64, ui.H - 52), "Menú Contabilidad. Mientras la sucursal está apagada, solo se ve Configuración emisor.", font=ui.f_pie, fill=BLANCO)
    return im


def escena_emisor(t):
    im, d = ui.lienzo("1  ·  Configuración emisor", "La sucursal se enciende y el emisor recibe su libro")
    ui.tarjeta(d, 70, 200, 1780, 680)
    ui.pegar(im, campo("Empresa", "Rafael Alberto Martinez Quesada", 560), 1, (110, 240))
    ui.pegar(im, campo("Sucursal", "CostaPets", 420), ui.ease((t - 0.3) / 0.3), (700, 240))
    ui.pegar(im, campo("Emisor", "Rafael Alberto Martinez Quesada", 560), ui.ease((t - 0.5) / 0.3), (1160, 240))
    estados = [("Sucursal", "Encendida"), ("Libro del emisor", "Creado"), ("Relación", "Vigente"), ("Contabilidad efectiva", "Activa")]
    for i, (eti, val) in enumerate(estados):
        x = 110 + i * 420
        capa = Image.new("RGBA", (380, 90), (0, 0, 0, 0))
        cd = ImageDraw.Draw(capa)
        cd.text((0, 0), eti, font=ui.f_lbl, fill=GRIS + (255,))
        cd.rounded_rectangle((0, 32, 220, 80), radius=20, fill=VERDE + (255,))
        cd.text((28, 42), val, font=ui.f_chip, fill=BLANCO + (255,))
        ui.pegar(im, capa, ui.ease((t - 1.0 - i * 0.2) / 0.3), (x, 390))
    ui.pegar(im, boton("Encender sucursal", 420, 72, ui.AZUL), ui.ease((t - 2.1) / 0.3), (110, 540))
    ui.pegar(im, boton("Crear libro del emisor", 520, 72, VERDE), ui.ease((t - 2.5) / 0.3), (560, 540))
    d = ImageDraw.Draw(im)
    d.text((110, 660), "Crear libro también deja vigente la sucursal elegida, si todavía no lo estaba.", font=ui.f_sub, fill=TEXTO)
    d.text((110, 710), "Apagar sucursal detiene los asientos nuevos. Lo ya contabilizado se conserva.", font=ui.f_sub, fill=GRIS)
    d.text((64, ui.H - 52), "Hacen falta los cuatro estados a la vez. Si falta uno, el documento se guarda y no hay póliza.", font=ui.f_pie, fill=BLANCO)
    return im


def escena_catalogo(t):
    im, d = ui.lienzo("2 y 3  ·  Catálogo y cuentas de uso", "El catálogo se puede cambiar. Los usos se guardan aparte")
    ui.tarjeta(d, 70, 210, 860, 640)
    d.text((110, 240), "Catálogo cuentas", font=ui.f_big, fill=TEXTO)
    cuentas = ["1   Activos", "1.1.4.1   Cuentas por cobrar", "1.1.6.1   Inventario", "2.1.1.1   Cuentas por pagar", "4.1.2   Ventas gravadas"]
    for i, c in enumerate(cuentas):
        ui.pegar(im, ui.texto_rgba(c, ui.f_num, TEXTO), ui.ease((t - 0.3 - i * 0.15) / 0.25), (120, 320 + i * 64))
    d.text((110, 680), "Agregar    ·    Editar", font=ui.f_sub, fill=AZUL)
    ui.tarjeta(d, 980, 210, 860, 640)
    d.text((1020, 240), "Cuentas de uso", font=ui.f_big, fill=TEXTO)
    usos = [
        ("Retención a proveedores", "2.1.3.2"),
        ("Banco en dólares", "1.1.2.2"),
        ("Depósitos en tránsito", "1.1.2.3"),
        ("Comisiones por pagar", "2.1.2.5"),
    ]
    for i, (eti, cod) in enumerate(usos):
        ui.pegar(im, campo(eti, cod, 760), ui.ease((t - 0.8 - i * 0.2) / 0.25), (1020, 310 + i * 110))
    d = ImageDraw.Draw(im)
    d.text((64, ui.H - 52), "Al terminar, pulse Guardar cuentas de uso. La retención y las comisiones salen de ahí.", font=ui.f_pie, fill=BLANCO)
    return im


def escena_proveedor(t):
    im, d = ui.lienzo("4  ·  Ficha del proveedor", "El porcentaje se escribe en Compras, Proveedores")
    ui.tarjeta(d, 180, 220, 1560, 600)
    d.text((240, 260), "Proveedor", font=ui.f_lbl, fill=GRIS)
    d.text((240, 296), "Distribuidora Vet", font=ui.f_big, fill=TEXTO)
    ui.pegar(im, campo("Retención (%)", "2", 420), ui.ease((t - 0.5) / 0.35), (240, 390))
    ui.pegar(im, campo("Otro proveedor", "vacío  ·  no aplica", 640), ui.ease((t - 1.1) / 0.35), (720, 390))
    if t > 1.8:
        recuadro = Image.new("RGBA", (1320, 160), (0, 0, 0, 0))
        rd = ImageDraw.Draw(recuadro)
        rd.rounded_rectangle((0, 0, 1280, 150), radius=14, fill=VERDE_SUAVE + (255,))
        rd.text((28, 24), "Pago de ₡100.000,00 con retención del 2 %", font=ui.f_sub, fill=TEXTO + (255,))
        rd.text((28, 78), "Banco  ₡98.000,00      Retención  ₡2.000,00", font=ui.f_big, fill=VERDE + (255,))
        ui.pegar(im, recuadro, ui.ease((t - 1.8) / 0.35), (240, 560))
    d = ImageDraw.Draw(im)
    d.text((64, ui.H - 52), "El porcentaje se toma en el momento del pago. Los pagos ya hechos no se recalculan.", font=ui.f_pie, fill=BLANCO)
    return im


def escena_plantillas(t):
    im, d = ui.lienzo("5 y 6  ·  Mes abierto y plantillas activas", "Sin mes abierto, o con la plantilla en borrador, no hay póliza")
    ui.tarjeta(d, 70, 210, 820, 640)
    d.text((110, 250), "Cierre", font=ui.f_big, fill=TEXTO)
    d.text((110, 320), "Ejercicio 2026     Mes  Septiembre", font=ui.f_sub, fill=TEXTO)
    ui.pegar(im, boton("Abrir período", 380, 70, VERDE), ui.ease((t - 0.4) / 0.3), (110, 390))
    ui.pegar(im, insignia("Abierto", VERDE), ui.ease((t - 1.0) / 0.3), (110, 500))
    ui.tarjeta(d, 940, 210, 910, 640)
    d.text((980, 250), "Plantillas", font=ui.f_big, fill=TEXTO)
    nombres = ["VentaEmitida", "CobroAplicado", "CompraRegistrada", "PagoProveedorAplicado"]
    for i, nombre in enumerate(nombres):
        fila = Image.new("RGBA", (820, 64), (0, 0, 0, 0))
        fd = ImageDraw.Draw(fila)
        fd.text((0, 14), nombre, font=ui.f_num, fill=TEXTO + (255,))
        fd.rounded_rectangle((520, 8, 760, 52), radius=16, fill=(VERDE if t > 1.4 + i * 0.25 else (180, 170, 150)) + (255,))
        fd.text((560, 16), "Activa" if t > 1.4 + i * 0.25 else "Borrador", font=ui.f_chip, fill=BLANCO + (255,))
        ui.pegar(im, fila, ui.ease((t - 0.6 - i * 0.15) / 0.25), (980, 330 + i * 90))
    d = ImageDraw.Draw(im)
    d.text((64, ui.H - 52), "En Versiones se revisan las cuentas y se pulsa Activar. El borrador no contabiliza.", font=ui.f_pie, fill=BLANCO)
    return im


def escena_dia(t):
    im, d = ui.lienzo("7  ·  El día de trabajo", "Cada documento entra en su momento")
    filas = [
        ("Preventa", "No deja asiento. El asiento nace al facturar."),
        ("Tiquete interno", "VentaEmitida, al emitirlo."),
        ("Factura electrónica", "VentaEmitida, cuando Hacienda acepta."),
        ("Cobro confirmado", "CobroAplicado. El cheque pendiente espera."),
        ("Compra sin clave", "CompraRegistrada, al registrarla."),
        ("Compra con clave", "CompraRegistrada, con el mensaje receptor aceptado."),
        ("Pago al proveedor", "PagoProveedorAplicado, con la retención de la ficha."),
    ]
    for i, (titulo, detalle) in enumerate(filas):
        ui.pegar(im, chip(i + 1, titulo, detalle, 1720), ui.ease((t - i * 0.22) / 0.28), (80, 200 + i * 100))
    d = ImageDraw.Draw(im)
    d.text((64, ui.H - 52), "La sucursal de la compra es la de la bodega. La de la venta es la sucursal de la factura.", font=ui.f_pie, fill=BLANCO)
    return im


def escena_bandeja(t):
    im, d = ui.lienzo("8  ·  Bandeja", "Eventos explica. Pólizas muestra lo que ya quedó")
    ui.tarjeta(d, 70, 210, 860, 640)
    d.text((110, 250), "Pestaña Eventos", font=ui.f_big, fill=TEXTO)
    causas = ["Plantilla todavía en borrador", "El mes de la fecha no está abierto", "Sucursal apagada o sin relación", "Hacienda aún no acepta"]
    for i, c in enumerate(causas):
        ui.pegar(im, ui.texto_rgba(c, ui.f_num, TEXTO, 760), ui.ease((t - 0.4 - i * 0.2) / 0.25), (120, 340 + i * 70))
    ui.pegar(im, boton("Reintentar", 320, 68, AZUL), ui.ease((t - 1.6) / 0.3), (110, 680))
    ui.tarjeta(d, 980, 210, 860, 640)
    d.text((1020, 250), "Pestaña Pólizas", font=ui.f_big, fill=TEXTO)
    d.text((1020, 330), "Ahí se lee el asiento ya cuadrado.", font=ui.f_sub, fill=TEXTO)
    d.text((1020, 390), "Detalle muestra las líneas.", font=ui.f_sub, fill=TEXTO)
    d.text((1020, 450), "Reversar deja el asiento contrario.", font=ui.f_sub, fill=TEXTO)
    ui.pegar(im, boton("Nueva póliza manual", 520, 70, VERDE), ui.ease((t - 2.0) / 0.3), (1020, 560))
    d = ImageDraw.Draw(im)
    d.text((64, ui.H - 52), "El documento operativo no se pierde. Se corrige la causa y se reintenta el evento.", font=ui.f_pie, fill=BLANCO)
    return im


def escena_consultas(t):
    im, d = ui.lienzo("Durante el mes", "Dónde se consulta lo que ya se contabilizó")
    items = [
        (1, "Diario", "Las pólizas de un período"),
        (2, "Mayor", "El movimiento de una cuenta"),
        (3, "Auxiliar CxC", "El cliente y Conciliar auxiliar"),
        (4, "Auxiliar CxP", "El proveedor y Conciliar auxiliar"),
        (5, "Balanza", "Debe y haber a una fecha de corte"),
        (6, "Estado de resultados", "Ingresos, gastos y utilidad neta"),
    ]
    for i, (n, titulo, detalle) in enumerate(items):
        col, fila = i % 2, i // 2
        ui.pegar(im, chip(n, titulo, detalle), ui.ease((t - i * 0.22) / 0.3), (70 + col * 920, 220 + fila * 180))
    d = ImageDraw.Draw(im)
    d.text((64, ui.H - 52), "Balanza y estado de resultados se consultan por fecha y se descargan en CSV.", font=ui.f_pie, fill=BLANCO)
    return im


def escena_cierre(t):
    im, d = ui.lienzo("Cerrar el ciclo", "El mes, el mes siguiente y, al final, el año")
    ui.tarjeta(d, 70, 210, 1780, 640)
    hitos = [
        ("1", "Pre-cierre", "En Cierre, con el mes elegido."),
        ("2", "Cerrar período", "Si está listo para cerrar."),
        ("3", "Abrir el mes siguiente", "El mismo día, para el día primero."),
        ("4", "Cierre anual", "Cuando los doce meses ya están cerrados."),
    ]
    for i, (n, titulo, detalle) in enumerate(hitos):
        ui.pegar(im, chip(int(n), titulo, detalle, 1600), ui.ease((t - i * 0.35) / 0.3), (120, 250 + i * 130))
    d = ImageDraw.Draw(im)
    d.text((64, ui.H - 52), "Un mes abierto impide el cierre anual. Un mes bloqueado ya no se reabre.", font=ui.f_pie, fill=BLANCO)
    return im


ESCENAS = [
    ("01", escena_ciclo,
     "Esta capacitación es de uso. Vamos a recorrer el ciclo completo de Contabilidad en SeePOS, pantalla por pantalla. El orden es este: configuración del emisor, catálogo, cuentas de uso, ficha del proveedor, abrir el mes, activar las plantillas, trabajar el día, revisar en la bandeja, y cerrar el mes. Mientras la sucursal está apagada, el menú solo muestra Configuración emisor. Ahí se enciende."),
    ("02", escena_emisor,
     "Entre a Contabilidad, Configuración emisor. Elija la empresa, la sucursal y el emisor. Pulse Encender sucursal. Después, Crear libro del emisor. Si esa sucursal todavía no estaba ligada, ese mismo botón la deja vigente. Cuando termine, tienen que verse cuatro estados: sucursal encendida, libro creado, relación vigente y contabilidad efectiva activa. Para dejar de contabilizar se usa Apagar sucursal. Lo ya hecho se conserva."),
    ("03", escena_catalogo,
     "Siga a Catálogo de cuentas. Si el libro está vacío, cargue el catálogo inicial. Si ya tiene cuentas, use Agregar y Editar. Nada queda cerrado: una cuenta se puede corregir y se pueden crear hijas. Luego vuelva a Configuración emisor, a Cuentas de uso. Elija la retención, el banco en dólares, el tránsito y las comisiones por pagar, y pulse Guardar cuentas de uso. Esas dos últimas, retención y comisiones, son las que usa el asiento."),
    ("04", escena_proveedor,
     "El porcentaje no es igual para todos. En Compras, Proveedores, abra la ficha y escriba Retención, en porcentaje. Con dos, un pago de cien mil deja dos mil en la cuenta de retención y noventa y ocho mil en el banco. Si el campo queda vacío, o en cero, a ese proveedor no se le retiene. El porcentaje se toma al pagar. Un pago ya hecho no cambia si después edita la ficha."),
    ("05", escena_plantillas,
     "Dos pasos antes de operar. En Cierre, indique el año y el mes, y pulse Abrir período. El mes queda abierto. En Plantillas, abra Versiones de cada receta del día: venta, cobro, compra y pago al proveedor. Revise las cuentas. Si la caja o el banco no son los sugeridos, cámbielos en una versión nueva. Pulse Activar. Mientras diga Borrador, el documento se guarda y no hay póliza."),
    ("06", escena_dia,
     "Ya con el mes abierto y las plantillas activas, el día sigue la operación normal. La preventa no se contabiliza. El tiquete interno sí, al emitirlo. La factura electrónica, cuando Hacienda la acepta. El cobro, cuando queda confirmado: un cheque pendiente espera. La compra sin clave se contabiliza al registrarla. La compra con clave, cuando el mensaje receptor queda aceptado. El pago al proveedor usa la retención de su ficha. La sucursal de la compra es la de la bodega."),
    ("07", escena_bandeja,
     "Si un documento se guardó y no ve la póliza, abra Bandeja, pestaña Eventos. El mensaje dice la causa: plantilla en borrador, mes sin abrir, sucursal apagada, o Hacienda todavía no acepta. Corrija eso y pulse Reintentar. En la pestaña Pólizas se ven los asientos ya hechos. Detalle muestra las líneas. Reversar deja el asiento contrario, con la contraseña. Nueva póliza manual sirve para un movimiento que no nace de una factura."),
    ("08", escena_consultas,
     "Durante el mes se consulta en estas pantallas. Diario muestra las pólizas del período. Mayor, el movimiento de una cuenta. Auxiliar de cuentas por cobrar y el de cuentas por pagar muestran el cliente o el proveedor, y Conciliar auxiliar compara el saldo de la operación con el del libro. Balanza da el debe y el haber a una fecha de corte. Estado de resultados da ingresos, gastos y utilidad neta, y se puede descargar en CSV."),
    ("09", escena_cierre,
     "Para cerrar el ciclo del mes, vuelva a Cierre. Elija el período, ejecute el pre-cierre y, si está listo, pulse Cerrar período. Ese mes deja de recibir asientos. El mismo día, abra el mes siguiente, para que el día primero no se quede sin período. El cierre anual se hace en su propia pantalla, cuando los doce meses del año ya están cerrados. Si queda uno abierto, el cierre anual no sigue. Así se recorre el módulo, de la sucursal encendida al cierre del ejercicio."),
]


async def narrar(ruta, texto):
    await ui.edge_tts.Communicate(texto, ui.VOZ, rate="-5%").save(str(ruta))


def duracion(ruta):
    r = subprocess.run(
        ["ffprobe", "-v", "error", "-show_entries", "format=duration", "-of", "csv=p=0", str(ruta)],
        capture_output=True, text=True, check=True)
    return float(r.stdout.strip())


async def main():
    for nombre, pintar, texto in ESCENAS:
        mp3 = ui.CLIPS / f"{nombre}.mp3"
        await narrar(mp3, texto)
        seg = duracion(mp3)
        print(nombre, f"{seg:.1f}s", flush=True)
        ui.render(nombre, mp3, seg, pintar)
    lista = ui.CLIPS / "lista.txt"
    lista.write_text("".join(f"file '{n}.mp4'\n" for n, _, _ in ESCENAS), encoding="utf-8")
    final = BASE / "Capacitacion-Uso-Contabilidad.mp4"
    subprocess.run(
        ["ffmpeg", "-y", "-loglevel", "error", "-f", "concat", "-safe", "0", "-i", str(lista), "-c", "copy", str(final)],
        check=True, cwd=ui.CLIPS)
    print(final)


if __name__ == "__main__":
    asyncio.run(main())
