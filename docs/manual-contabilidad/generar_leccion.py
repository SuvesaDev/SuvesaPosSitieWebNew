# -*- coding: utf-8 -*-
"""Lección gráfica de Contabilidad. El cuadro no se mueve: entran los datos."""
import asyncio
import subprocess
from pathlib import Path

from PIL import Image, ImageDraw, ImageFont
import edge_tts

BASE = Path("/Users/amartinez/Downloads/Git/SuvesaPosSitieWebNew/docs/manual-contabilidad")
CLIPS = BASE / "video" / "leccion"
CLIPS.mkdir(parents=True, exist_ok=True)
FONT = "/System/Library/Fonts/Supplemental/Arial.ttf"
FONTB = "/System/Library/Fonts/Supplemental/Arial Bold.ttf"
VOZ = "es-CR-MariaNeural"
FPS = 24
W, H = 1920, 1080

FONDO = (246, 244, 239)
AZUL = (16, 114, 169)
VERDE = (27, 122, 72)
VERDE_SUAVE = (232, 245, 236)
TIERRA = (74, 58, 42)
TEXTO = (32, 40, 48)
GRIS = (98, 108, 118)
LINEA = (226, 220, 210)
BLANCO = (255, 255, 255)
ROJO = (166, 62, 52)

f_marca = ImageFont.truetype(FONT, 22)
f_h = ImageFont.truetype(FONTB, 42)
f_sub = ImageFont.truetype(FONT, 24)
f_lbl = ImageFont.truetype(FONT, 20)
f_num = ImageFont.truetype(FONTB, 26)
f_big = ImageFont.truetype(FONTB, 32)
f_pie = ImageFont.truetype(FONT, 26)
f_chip = ImageFont.truetype(FONTB, 18)


def ease(t):
    t = 0 if t < 0 else 1 if t > 1 else t
    return 1 - (1 - t) ** 3


def colones(n):
    entero, frac = f"{n:,.2f}".split(".")
    return "₡" + entero.replace(",", ".") + "," + frac


def lienzo(paso, titulo):
    im = Image.new("RGB", (W, H), FONDO)
    d = ImageDraw.Draw(im)
    d.rectangle((0, 0, W, 8), fill=VERDE)
    d.rectangle((0, 8, W, 86), fill=TIERRA)
    d.text((64, 36), "SeePOS   ·   Contabilidad", font=f_marca, fill=(236, 228, 214))
    d.text((W - 220, 34), paso, font=f_chip, fill=(214, 196, 160))
    d.text((64, 118), titulo, font=f_h, fill=TEXTO)
    d.rectangle((0, H - 78, W, H), fill=AZUL)
    return im, d


def tarjeta(d, x, y, w, h):
    d.rounded_rectangle((x, y, x + w, y + h), radius=18, fill=BLANCO, outline=LINEA, width=2)


def pegar(base, capa, alpha, xy):
    if alpha <= 0.01:
        return
    c = capa.copy()
    if alpha < 0.99:
        a = c.split()[-1].point(lambda p: int(p * alpha))
        c.putalpha(a)
    base.paste(c, xy, c)


def texto_rgba(texto, fuente, color, ancho=None):
    dtmp = ImageDraw.Draw(Image.new("RGB", (8, 8)))
    ancho = ancho or int(dtmp.textlength(texto, font=fuente)) + 4
    alto = fuente.size + 10
    im = Image.new("RGBA", (ancho, alto), (0, 0, 0, 0))
    ImageDraw.Draw(im).text((0, 0), texto, font=fuente, fill=color + (255,))
    return im


def fila_asiento(mov, cuenta, monto, debito):
    im = Image.new("RGBA", (1480, 58), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    color = AZUL if debito else VERDE
    d.rounded_rectangle((0, 8, 130, 48), radius=8, fill=color)
    d.text((18, 16), mov, font=f_chip, fill=BLANCO + (255,))
    d.text((160, 14), cuenta, font=f_num, fill=TEXTO + (255,))
    monto_t = colones(monto)
    tw = d.textlength(monto_t, font=f_num)
    d.text((1480 - tw - 8, 14), monto_t, font=f_num, fill=TEXTO + (255,))
    return im


def render(nombre, audio, duracion, pintar):
    frames = int(duracion * FPS) + int(0.4 * FPS)
    seg = frames / FPS
    cmd = [
        "ffmpeg", "-y", "-loglevel", "error",
        "-f", "rawvideo", "-pix_fmt", "rgb24", "-s", f"{W}x{H}", "-r", str(FPS), "-i", "-",
        "-i", str(audio),
        "-t", f"{seg:.3f}",
        "-vf", f"fade=t=in:st=0:d=0.25,fade=t=out:st={max(seg - 0.3, 0.3):.2f}:d=0.25",
        "-c:v", "libx264", "-pix_fmt", "yuv420p", "-preset", "veryfast",
        "-c:a", "aac", "-b:a", "192k", "-shortest", "-movflags", "+faststart",
        str(CLIPS / f"{nombre}.mp4"),
    ]
    proc = subprocess.Popen(cmd, stdin=subprocess.PIPE)
    for i in range(frames):
        t = i / FPS
        im = pintar(t).convert("RGB")
        proc.stdin.write(im.tobytes())
    proc.stdin.close()
    proc.wait()
    if proc.returncode != 0:
        raise RuntimeError(nombre)


def escena_venta(t):
    im, d = lienzo("Caso 1  ·  Venta", "Una factura y el asiento que deja")
    tarjeta(d, 70, 210, 1780, 250)
    d.text((110, 236), "Cliente", font=f_lbl, fill=GRIS)
    d.text((110, 268), "Mascotas del Valle", font=f_big, fill=TEXTO)
    d.text((720, 236), "Documento", font=f_lbl, fill=GRIS)
    d.text((720, 268), "Factura  ·  15 de septiembre", font=f_big, fill=TEXTO)
    datos = [("Subtotal", 100000), ("IVA 13 %", 13000), ("Total", 113000)]
    for i, (eti, val) in enumerate(datos):
        x = 110 + i * 360
        d.text((x, 340), eti, font=f_lbl, fill=GRIS)
        d.text((x, 368), colones(val), font=f_big, fill=AZUL if i < 2 else VERDE)
    d.text((110, 490), "Asiento", font=f_sub, fill=TIERRA)
    lineas = [
        ("Débito", "1.1.4.1   Cuentas por cobrar", 113000, True),
        ("Crédito", "4.1.2   Ventas gravadas", 100000, False),
        ("Crédito", "2.1.3.1   IVA devengado", 13000, False),
    ]
    for i, (mov, cuenta, monto, deb) in enumerate(lineas):
        pegar(im, fila_asiento(mov, cuenta, monto, deb), ease((t - 0.8 - i * 0.45) / 0.4), (110, 540 + i * 70))
    if t > 2.6:
        pegar(im, texto_rgba("Debe  ₡113.000,00    =    Haber  ₡113.000,00", f_num, VERDE), ease((t - 2.6) / 0.35), (110, 770))
    d = ImageDraw.Draw(im)
    d.text((64, H - 52), "El tiquete interno entra al emitirse. La factura electrónica, cuando Hacienda la acepta.", font=f_pie, fill=BLANCO)
    return im


def escena_compra(t):
    im, d = lienzo("Caso 2  ·  Compra", "Una factura de proveedor y su asiento")
    tarjeta(d, 70, 210, 1780, 250)
    d.text((110, 236), "Proveedor", font=f_lbl, fill=GRIS)
    d.text((110, 268), "Distribuidora Vet", font=f_big, fill=TEXTO)
    d.text((720, 236), "Documento", font=f_lbl, fill=GRIS)
    d.text((720, 268), "Compra  ·  sin clave fiscal", font=f_big, fill=TEXTO)
    datos = [("Inventario", 80000), ("IVA soportado", 10400), ("Total a pagar", 90400)]
    for i, (eti, val) in enumerate(datos):
        x = 110 + i * 420
        d.text((x, 340), eti, font=f_lbl, fill=GRIS)
        d.text((x, 368), colones(val), font=f_big, fill=AZUL if i < 2 else VERDE)
    d.text((110, 490), "Asiento", font=f_sub, fill=TIERRA)
    lineas = [
        ("Débito", "1.1.6.1   Inventario", 80000, True),
        ("Débito", "1.1.7.1   IVA soportado", 10400, True),
        ("Crédito", "2.1.1.1   Cuentas por pagar", 90400, False),
    ]
    for i, (mov, cuenta, monto, deb) in enumerate(lineas):
        pegar(im, fila_asiento(mov, cuenta, monto, deb), ease((t - 0.7 - i * 0.4) / 0.4), (110, 540 + i * 70))
    if t > 2.4:
        pegar(im, texto_rgba("Debe  ₡90.400,00    =    Haber  ₡90.400,00", f_num, VERDE), ease((t - 2.4) / 0.35), (110, 770))
    d = ImageDraw.Draw(im)
    d.text((64, H - 52), "Si la compra trae clave fiscal, el asiento espera el mensaje receptor aceptado.", font=f_pie, fill=BLANCO)
    return im


def escena_manual(t):
    im, d = lienzo("Asiento manual", "Se crea en Bandeja, pestaña Pólizas")
    tarjeta(d, 70, 200, 1780, 640)
    d.text((110, 230), "Nueva póliza manual", font=f_big, fill=TEXTO)
    d.text((110, 290), "Fecha contable", font=f_lbl, fill=GRIS)
    d.rounded_rectangle((110, 324, 460, 384), radius=10, outline=LINEA, width=2, fill=FONDO)
    d.text((130, 338), "30 / 09 / 2026", font=f_num, fill=TEXTO)
    d.text((520, 290), "Sucursal", font=f_lbl, fill=GRIS)
    d.rounded_rectangle((520, 324, 980, 384), radius=10, outline=LINEA, width=2, fill=FONDO)
    d.text((540, 338), "CostaPets", font=f_num, fill=TEXTO)
    d.text((110, 420), "Líneas", font=f_sub, fill=TIERRA)
    encabezado = Image.new("RGBA", (1480, 36), (0, 0, 0, 0))
    ImageDraw.Draw(encabezado).text((0, 0), "Movimiento          Cuenta                                              Monto", font=f_lbl, fill=GRIS + (255,))
    pegar(im, encabezado, 1, (110, 470))
    pegar(im, fila_asiento("Débito", "1.1.2.1   BAC colones", 50000, True), ease((t - 0.6) / 0.4), (110, 520))
    pegar(im, fila_asiento("Crédito", "1.1.1.2   Caja cobro ruta", 50000, False), ease((t - 1.2) / 0.4), (110, 590))
    if t > 2.0:
        sello = Image.new("RGBA", (700, 70), (0, 0, 0, 0))
        sd = ImageDraw.Draw(sello)
        sd.rounded_rectangle((0, 0, 640, 60), radius=12, fill=VERDE_SUAVE + (255,))
        sd.text((24, 14), "Debe igual al haber    ₡50.000,00", font=f_num, fill=VERDE + (255,))
        pegar(im, sello, ease((t - 2.0) / 0.35), (110, 700))
    d = ImageDraw.Draw(im)
    d.text((64, H - 52), "La fecha tiene que caer en un mes abierto. Una cuenta de control pide la contraseña.", font=f_pie, fill=BLANCO)
    return im


def escena_abrir(t):
    im, d = lienzo("Abrir el período", "Cierre  ·  el mes nace abierto")
    tarjeta(d, 360, 230, 1200, 560)
    d.text((420, 270), "Ejercicio", font=f_lbl, fill=GRIS)
    d.rounded_rectangle((420, 310, 760, 380), radius=10, outline=LINEA, width=2, fill=FONDO)
    d.text((450, 326), "2026", font=f_big, fill=TEXTO)
    d.text((820, 270), "Mes", font=f_lbl, fill=GRIS)
    d.rounded_rectangle((820, 310, 1320, 380), radius=10, outline=LINEA, width=2, fill=FONDO)
    d.text((850, 326), "Septiembre", font=f_big, fill=TEXTO)
    abierto = t > 1.6
    btn = VERDE if not abierto else (210, 214, 208)
    d.rounded_rectangle((420, 440, 860, 520), radius=12, fill=btn)
    d.text((470, 460), "Abrir período", font=f_big, fill=BLANCO if not abierto else GRIS)
    etiqueta = "Abierto" if abierto else "Sin abrir"
    fondo_b = VERDE if abierto else (180, 176, 168)
    d.rounded_rectangle((900, 448, 1180, 512), radius=24, fill=fondo_b)
    d.text((960, 464), etiqueta, font=f_num, fill=BLANCO)
    d.text((420, 580), "Los asientos de septiembre solo entran en este mes.", font=f_sub, fill=TEXTO)
    d.text((420, 640), "Octubre se abre aparte, cuando llegue octubre.", font=f_sub, fill=GRIS)
    d = ImageDraw.Draw(im)
    d.text((64, H - 52), "Pantalla Cierre. Elija el año y el mes, y pulse Abrir período.", font=f_pie, fill=BLANCO)
    return im


def escena_cerrar(t):
    im, d = lienzo("Cerrar el período", "Primero el pre-cierre, después el cierre")
    tarjeta(d, 280, 210, 1360, 620)
    d.text((340, 250), "09 / 2026", font=f_big, fill=TEXTO)
    cerrado = t > 2.2
    d.rounded_rectangle((620, 246, 860, 304), radius=22, fill=(180, 140, 40) if not cerrado else (90, 98, 108))
    d.text((660, 258), "Abierto" if not cerrado else "Cerrado", font=f_num, fill=BLANCO)
    listo = t > 1.0
    caja = VERDE_SUAVE if listo else (255, 244, 220)
    tinta = VERDE if listo else (140, 90, 20)
    d.rounded_rectangle((340, 360, 1480, 470), radius=14, fill=caja)
    d.text((380, 390), "Listo para cerrar." if listo else "Ejecutar pre-cierre", font=f_big, fill=tinta)
    d.text((380, 432), "Eventos bloqueantes: 0     Diferencias sin resolver: 0" if listo else "Revise la bandeja antes de cerrar el mes.", font=f_lbl, fill=tinta)
    d.rounded_rectangle((340, 520, 760, 600), radius=12, fill=VERDE if listo and not cerrado else (210, 214, 208))
    d.text((390, 540), "Cerrar período", font=f_big, fill=BLANCO if listo and not cerrado else GRIS)
    d.text((340, 680), "Ese mes deja de recibir asientos. El siguiente hay que abrirlo.", font=f_sub, fill=TEXTO)
    d = ImageDraw.Draw(im)
    d.text((64, H - 52), "Reabrir un mes cerrado pide la contraseña. Un mes bloqueado ya no se reabre.", font=f_pie, fill=BLANCO)
    return im


def tabla_aux(im, t, titulo, filas, pie):
    base, d = lienzo(titulo, "Movimiento del cliente o del proveedor, y la conciliación")
    tarjeta(d, 80, 200, 1760, 620)
    d.text((120, 230), "Fecha", font=f_lbl, fill=GRIS)
    d.text((360, 230), "Origen", font=f_lbl, fill=GRIS)
    d.text((1100, 230), "Débito", font=f_lbl, fill=GRIS)
    d.text((1450, 230), "Crédito", font=f_lbl, fill=GRIS)
    d.line((120, 270, 1760, 270), fill=LINEA, width=2)
    for i, (fecha, origen, deb, cre) in enumerate(filas):
        y = 300 + i * 78
        alpha = ease((t - 0.4 - i * 0.35) / 0.35)
        capa = Image.new("RGBA", (1640, 64), (0, 0, 0, 0))
        cd = ImageDraw.Draw(capa)
        cd.text((0, 12), fecha, font=f_num, fill=TEXTO + (255,))
        cd.text((240, 12), origen, font=f_num, fill=TEXTO + (255,))
        cd.text((980, 12), deb, font=f_num, fill=TEXTO + (255,))
        cd.text((1330, 12), cre, font=f_num, fill=TEXTO + (255,))
        pegar(base, capa, alpha, (120, y))
    if t > 1.8:
        sello = texto_rgba("Conciliar auxiliar   ·   Sin diferencias", f_num, VERDE)
        pegar(base, sello, ease((t - 1.8) / 0.35), (120, 720))
    d = ImageDraw.Draw(base)
    d.text((64, H - 52), pie, font=f_pie, fill=BLANCO)
    return base


def escena_cxc(t):
    return tabla_aux(
        None, t, "Auxiliar de cuentas por cobrar",
        [("15/09/2026", "Ventas  ·  Mascotas del Valle", colones(113000), "—")],
        "Pantalla Auxiliar CxC. El botón Conciliar auxiliar compara el saldo legado con el del libro.",
    )


def escena_cxp(t):
    return tabla_aux(
        None, t, "Auxiliar de cuentas por pagar",
        [("15/09/2026", "Compras  ·  Distribuidora Vet", "—", colones(90400))],
        "Pantalla Auxiliar CxP. Misma idea: el proveedor, el documento y el saldo del libro.",
    )


def escena_resultado(t):
    im, d = lienzo("Estado de resultados", "Ingresos, gastos y utilidad del rango elegido")
    tarjeta(d, 220, 200, 1480, 640)
    d.text((280, 240), "Del 1 al 30 de septiembre de 2026", font=f_sub, fill=GRIS)
    d.text((280, 310), "Ingresos", font=f_big, fill=AZUL)
    d.text((320, 380), "4.1.2    Ventas gravadas", font=f_num, fill=TEXTO)
    d.text((1280, 380), colones(100000), font=f_num, fill=TEXTO)
    d.text((280, 450), "Total ingresos", font=f_num, fill=TEXTO)
    d.text((1240, 450), colones(100000), font=f_big, fill=AZUL)
    d.text((280, 530), "Gastos", font=f_big, fill=ROJO)
    d.text((320, 590), "En este caso la compra quedó en inventario, no en un gasto.", font=f_sub, fill=GRIS)
    d.text((280, 660), "Total gastos", font=f_num, fill=TEXTO)
    d.text((1320, 660), colones(0), font=f_big, fill=ROJO)
    if t > 1.4:
        barra = Image.new("RGBA", (1320, 80), (0, 0, 0, 0))
        bd = ImageDraw.Draw(barra)
        bd.rounded_rectangle((0, 0, 1280, 72), radius=12, fill=(232, 243, 236, 255))
        bd.text((28, 18), "Utilidad neta", font=f_big, fill=VERDE + (255,))
        bd.text((860, 18), colones(100000), font=f_big, fill=VERDE + (255,))
        pegar(im, barra, ease((t - 1.4) / 0.4), (280, 730))
    d = ImageDraw.Draw(im)
    d.text((64, H - 52), "Se consulta por fechas y se puede descargar en CSV.", font=f_pie, fill=BLANCO)
    return im


def escena_balanza(t):
    im, d = lienzo("Balanza de comprobación", "A la fecha de corte, el debe tiene que igualar al haber")
    tarjeta(d, 120, 200, 1680, 680)
    d.text((170, 230), "Fecha de corte    30 / 09 / 2026", font=f_sub, fill=GRIS)
    heads = [(170, "Código"), (420, "Descripción"), (980, "Débito"), (1240, "Crédito"), (1500, "Saldo")]
    for x, htxt in heads:
        d.text((x, 290), htxt, font=f_lbl, fill=GRIS)
    d.line((170, 330, 1700, 330), fill=LINEA, width=2)
    filas = [
        ("1.1.4.1", "Cuentas por cobrar", 113000, 0),
        ("1.1.6.1", "Inventario", 80000, 0),
        ("1.1.7.1", "IVA soportado", 10400, 0),
        ("2.1.1.1", "Cuentas por pagar", 0, 90400),
        ("2.1.3.1", "IVA devengado", 0, 13000),
        ("4.1.2", "Ventas gravadas", 0, 100000),
    ]
    for i, (cod, desc, deb, cre) in enumerate(filas):
        y = 350 + i * 58
        saldo = deb - cre
        capa = Image.new("RGBA", (1560, 48), (0, 0, 0, 0))
        cd = ImageDraw.Draw(capa)
        cd.text((0, 6), cod, font=f_num, fill=TEXTO + (255,))
        cd.text((250, 6), desc, font=f_num, fill=TEXTO + (255,))
        cd.text((800, 6), colones(deb) if deb else "—", font=f_num, fill=TEXTO + (255,))
        cd.text((1060, 6), colones(cre) if cre else "—", font=f_num, fill=TEXTO + (255,))
        cd.text((1320, 6), colones(saldo), font=f_num, fill=TEXTO + (255,))
        pegar(im, capa, ease((t - 0.3 - i * 0.18) / 0.3), (170, y))
    if t > 2.2:
        tot = texto_rgba("Total debe  ₡203.400,00     =     Total haber  ₡203.400,00", f_num, VERDE)
        pegar(im, tot, ease((t - 2.2) / 0.35), (170, 760))
    d = ImageDraw.Draw(im)
    d.text((64, H - 52), "Pantalla Balanza. Si los dos totales no coinciden, revise la bandeja antes de cerrar el mes.", font=f_pie, fill=BLANCO)
    return im


def escena_anual(t):
    im, d = lienzo("Cierre anual", "Los doce meses cerrados, y después el cierre del ejercicio")
    tarjeta(d, 160, 200, 1600, 640)
    d.text((220, 240), "Ejercicio 2026", font=f_big, fill=TEXTO)
    meses = ["Ene", "Feb", "Mar", "Abr", "May", "Jun", "Jul", "Ago", "Sep", "Oct", "Nov", "Dic"]
    for i, mes in enumerate(meses):
        col, row = i % 6, i // 6
        x, y = 220 + col * 240, 330 + row * 120
        visible = ease((t - i * 0.12) / 0.25)
        chip = Image.new("RGBA", (210, 90), (0, 0, 0, 0))
        cd = ImageDraw.Draw(chip)
        cd.rounded_rectangle((0, 0, 200, 82), radius=14, fill=(236, 239, 236, 255), outline=LINEA + (255,))
        cd.text((18, 10), mes, font=f_num, fill=TEXTO + (255,))
        cd.text((18, 46), "Cerrado", font=f_lbl, fill=VERDE + (255,))
        pegar(im, chip, visible, (x, y))
    if t > 2.0:
        boton = Image.new("RGBA", (520, 80), (0, 0, 0, 0))
        bd = ImageDraw.Draw(boton)
        bd.rounded_rectangle((0, 0, 500, 70), radius=12, fill=VERDE + (255,))
        bd.text((70, 16), "Ejecutar cierre anual", font=f_big, fill=BLANCO + (255,))
        pegar(im, boton, ease((t - 2.0) / 0.35), (220, 620))
    d = ImageDraw.Draw(im)
    d.text((64, H - 52), "Si queda un mes abierto, el cierre anual no se ejecuta. Primero se cierra ese mes.", font=f_pie, fill=BLANCO)
    return im


ESCENAS = [
    ("01", escena_venta,
     "Caso de venta. El cliente Mascotas del Valle compra por cien mil colones, más trece mil de IVA. El total de la factura es ciento trece mil. El asiento debita cuentas por cobrar por el total, acredita las ventas por el subtotal y acredita el IVA. El debe y el haber quedan iguales. El tiquete interno se contabiliza al emitirlo. La factura electrónica espera a que Hacienda la acepte."),
    ("02", escena_compra,
     "Caso de compra. Llega la factura de Distribuidora Vet: ochenta mil de inventario y diez mil cuatrocientos de IVA. El total a pagar es noventa mil cuatrocientos. El asiento debita inventario, debita el IVA soportado y acredita cuentas por pagar. También cuadra. Si la compra no trae clave fiscal, el asiento nace al registrarla. Si trae clave, espera a que el mensaje receptor quede aceptado."),
    ("03", escena_manual,
     "Un asiento que no nace de una factura se crea a mano. En Bandeja, pestaña Pólizas, se pulsa Nueva póliza manual. En el ejemplo se pasan cincuenta mil de la caja al banco: débito al banco y crédito a la caja, con la misma cifra. La fecha tiene que estar en un mes abierto y la sucursal tiene que estar encendida. Si la cuenta es de control, el sistema pide la contraseña antes de guardar."),
    ("04", escena_abrir,
     "Para que esos asientos puedan entrar, el mes tiene que estar abierto. En la pantalla Cierre se escribe el ejercicio, se elige el mes y se pulsa Abrir período. Septiembre de dos mil veintiséis queda abierto. Los asientos del quince de septiembre entran ahí. Los de octubre no, hasta que se abra octubre."),
    ("05", escena_cerrar,
     "Al terminar el mes se cierra. Primero, Ejecutar pre-cierre. Si no hay eventos pendientes ni diferencias sin resolver, aparece Listo para cerrar. Entonces se pulsa Cerrar período. Septiembre pasa a cerrado y ya no recibe asientos nuevos. Si hiciera falta uno, se reabre con la contraseña. Bloquear el mes es definitivo."),
    ("06", escena_cxc,
     "El auxiliar de cuentas por cobrar muestra el movimiento de cada cliente. En el caso de Mascotas del Valle se ve la fecha, el origen Ventas, y el débito de ciento trece mil. El botón Conciliar auxiliar compara ese saldo con el del libro. Si coinciden, no hay diferencia. Si no coinciden, se deja constancia con Resolver."),
    ("07", escena_cxp,
     "El auxiliar de cuentas por pagar hace lo mismo con los proveedores. Distribuidora Vet aparece con el crédito de noventa mil cuatrocientos, que es el total de la factura. Conciliar auxiliar revisa que ese saldo sea el mismo en la operación y en el libro."),
    ("08", escena_resultado,
     "El estado de resultados resume ingresos y gastos entre dos fechas. En este ejemplo, ventas gravadas suma cien mil. La compra no aparece como gasto, porque quedó en la cuenta de inventario. Por eso el total de gastos es cero y la utilidad neta es cien mil. El resultado se consulta en su pantalla y se puede descargar en CSV."),
    ("09", escena_balanza,
     "La balanza de comprobación lista cada cuenta a una fecha de corte, con su débito, su crédito y su saldo. Al pie, el total del debe tiene que ser igual al total del haber. Con la venta y la compra de este caso, ambos dan doscientos tres mil cuatrocientos. Si no coinciden, se revisa la bandeja antes de cerrar el mes."),
    ("10", escena_anual,
     "El cierre anual se hace cuando los doce meses del ejercicio ya están cerrados. En la pantalla Cierre anual se consulta el año. Si falta un mes, o si alguno sigue abierto, el cierre no sigue. Cuando los doce dicen cerrado, se pulsa Ejecutar cierre anual. Así queda cerrado el ejercicio y se puede adjuntar la autorización."),
]


async def narrar(ruta, texto):
    await edge_tts.Communicate(texto, VOZ, rate="-5%", pitch="+0Hz").save(str(ruta))


def duracion(ruta):
    r = subprocess.run(
        ["ffprobe", "-v", "error", "-show_entries", "format=duration", "-of", "csv=p=0", str(ruta)],
        capture_output=True, text=True, check=True)
    return float(r.stdout.strip())


async def main():
    for nombre, pintar, texto in ESCENAS:
        mp3 = CLIPS / f"{nombre}.mp3"
        await narrar(mp3, texto)
        seg = duracion(mp3)
        print(nombre, f"{seg:.1f}s", flush=True)
        render(nombre, mp3, seg, pintar)
    lista = CLIPS / "lista.txt"
    lista.write_text("".join(f"file '{n}.mp4'\n" for n, _, _ in ESCENAS), encoding="utf-8")
    final = BASE / "Capacitacion-Contabilidad.mp4"
    subprocess.run(
        ["ffmpeg", "-y", "-loglevel", "error", "-f", "concat", "-safe", "0", "-i", str(lista), "-c", "copy", str(final)],
        check=True, cwd=CLIPS)
    print(final)


if __name__ == "__main__":
    asyncio.run(main())
