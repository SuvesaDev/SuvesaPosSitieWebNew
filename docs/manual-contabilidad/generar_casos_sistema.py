# -*- coding: utf-8 -*-
"""Capacitación del módulo completo, con casos reales sobre pantallas del sistema."""
import asyncio
import subprocess
import sys
from pathlib import Path

from PIL import Image, ImageDraw

BASE = Path("/Users/amartinez/Downloads/Git/SuvesaPosSitieWebNew/docs/manual-contabilidad")
DOCS = BASE.parent
sys.path.insert(0, str(BASE))
import generar_leccion as ui

ui.CLIPS = BASE / "video" / "casos-sistema"
ui.CLIPS.mkdir(parents=True, exist_ok=True)

CAP = BASE / "video" / "capturas"
PAN = BASE / "pantallas"
FAC = DOCS / "manual-empaque"
BOD = DOCS / "manual-bodega"

AZUL, VERDE, VERDE_SUAVE = ui.AZUL, ui.VERDE, ui.VERDE_SUAVE
TEXTO, GRIS, LINEA, BLANCO, TIERRA = ui.TEXTO, ui.GRIS, ui.LINEA, ui.BLANCO, ui.TIERRA
ROJO = ui.ROJO
CREMA = (250, 248, 244)
W, H = ui.W, ui.H
X0 = 428
EMPRESA = "Rafael Alberto Martinez Quesada"


def pantalla(ruta):
    src = Image.open(ruta).convert("RGB")
    w, h = src.size
    destino = 1920 / 1080
    if w / h > destino:
        nw = int(h * destino)
        x = (w - nw) // 2
        src = src.crop((x, 0, x + nw, h))
    else:
        nh = int(w / destino)
        src = src.crop((0, 0, w, nh))
    return src.resize((W, H), Image.Resampling.LANCZOS)


def escena_base(ruta, pie):
    im = pantalla(ruta)
    d = ImageDraw.Draw(im)
    d.rectangle((0, H - 78, W, H), fill=AZUL)
    d.text((64, H - 52), pie, font=ui.f_pie, fill=BLANCO)
    return im, d


def chip_caso(texto):
    tw = ImageDraw.Draw(Image.new("RGB", (8, 8))).textlength(texto, font=ui.f_chip) + 48
    im = Image.new("RGBA", (int(tw), 48), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    d.rounded_rectangle((0, 0, int(tw) - 1, 44), radius=22, fill=TIERRA + (255,))
    d.text((22, 10), texto, font=ui.f_chip, fill=(236, 228, 214, 255))
    return im


def panel(ancho, alto):
    im = Image.new("RGBA", (ancho, alto), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    d.rounded_rectangle((0, 0, ancho - 1, alto - 1), radius=18, fill=(255, 255, 255, 242), outline=LINEA + (255,), width=2)
    return im, d


def fila_corta(mov, cuenta, monto, debito, ancho=720):
    im = Image.new("RGBA", (ancho, 52), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    color = AZUL if debito else VERDE
    d.rounded_rectangle((0, 6, 118, 44), radius=8, fill=color + (255,))
    d.text((14, 14), mov, font=ui.f_chip, fill=BLANCO + (255,))
    d.text((136, 12), cuenta, font=ui.f_lbl, fill=TEXTO + (255,))
    monto_t = ui.colones(monto)
    tw = d.textlength(monto_t, font=ui.f_num)
    d.text((ancho - tw - 8, 10), monto_t, font=ui.f_num, fill=TEXTO + (255,))
    return im


def nota(texto, fondo, tinta, ancho=720):
    im = Image.new("RGBA", (ancho, 64), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    d.rounded_rectangle((0, 0, ancho - 8, 56), radius=12, fill=fondo + (255,))
    d.text((20, 14), texto, font=ui.f_num, fill=tinta + (255,))
    return im


def chrome_modulo(titulo, pie):
    im = pantalla(CAP / "catalogo.png")
    d = ImageDraw.Draw(im)
    d.rectangle((X0 - 8, 86, W, H - 78), fill=CREMA)
    d.rectangle((0, H - 78, W, H), fill=AZUL)
    d.text((64, H - 52), pie, font=ui.f_pie, fill=BLANCO)
    d.text((X0 + 24, 100), "Inicio", font=ui.f_lbl, fill=GRIS)
    d.text((X0 + 88, 100), ">", font=ui.f_lbl, fill=GRIS)
    d.text((X0 + 112, 100), "Contabilidad", font=ui.f_lbl, fill=GRIS)
    d.text((X0 + 268, 100), ">", font=ui.f_lbl, fill=GRIS)
    d.text((X0 + 292, 100), titulo, font=ui.f_lbl, fill=TEXTO)
    d.rounded_rectangle((X0 + 24, 146, X0 + 92, 214), radius=14, fill=VERDE_SUAVE)
    d.text((X0 + 112, 158), titulo, font=ui.f_h, fill=TEXTO)
    d.rounded_rectangle((X0 + 24, 250, 1860, 370), radius=18, fill=BLANCO, outline=LINEA, width=2)
    d.text((X0 + 48, 268), "Empresa", font=ui.f_lbl, fill=GRIS)
    d.rounded_rectangle((X0 + 48, 298, 1200, 350), radius=12, outline=LINEA, width=2, fill=CREMA)
    d.text((X0 + 64, 310), EMPRESA, font=ui.f_num, fill=TEXTO)
    return im, d


def fila_tabla(celdas, anchos, negrita=False):
    fuente = ui.f_num if negrita else ui.f_lbl
    im = Image.new("RGBA", (sum(anchos), 44), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    x = 0
    for texto, ancho in zip(celdas, anchos):
        d.text((x + 8, 10), texto, font=fuente, fill=TEXTO + (255,))
        x += ancho
    return im


def escena_config(t):
    im, d = escena_base(CAP / "config.png", "Pantalla Configuración emisor. Los cuatro estados tienen que estar en verde.")
    ui.pegar(im, chip_caso("1  ·  el libro"), ui.ease((t - 0.15) / 0.3), (1560, 96))
    p, pd = panel(780, 220)
    pd.text((28, 22), "CostaPets ya contabiliza", font=ui.f_num, fill=TEXTO + (255,))
    pd.text((28, 72), "Encendida, Creado, Vigente, Activa.", font=ui.f_lbl, fill=GRIS + (255,))
    pd.text((28, 118), "La retención se elige aquí. El porcentaje", font=ui.f_lbl, fill=TEXTO + (255,))
    pd.text((28, 158), "vive en la ficha de cada proveedor.", font=ui.f_lbl, fill=TEXTO + (255,))
    ui.pegar(im, p, ui.ease((t - 0.5) / 0.35), (1080, 720))
    return im


def escena_catalogo(t):
    im, d = escena_base(CAP / "catalogo.png", "Pantalla Catálogo cuentas. El árbol se edita. Nada queda cerrado.")
    ui.pegar(im, chip_caso("2  ·  el catálogo"), ui.ease((t - 0.15) / 0.3), (1520, 96))
    p, pd = panel(820, 180)
    pd.text((28, 22), "Incluye costo de ventas y gastos.", font=ui.f_num, fill=TEXTO + (255,))
    pd.text((28, 72), "El asiento usa la cuenta hija. 6.1.6.8 es merma.", font=ui.f_lbl, fill=TEXTO + (255,))
    pd.text((28, 112), "Si el libro ya tenía cuentas, se crea a mano.", font=ui.f_lbl, fill=GRIS + (255,))
    ui.pegar(im, p, ui.ease((t - 0.5) / 0.35), (1040, 760))
    return im


def escena_plantillas(t):
    im, d = escena_base(CAP / "plantillas.png", "Pantalla Plantillas. Versiones, simular y activar. El borrador no contabiliza.")
    ui.pegar(im, chip_caso("3  ·  las recetas"), ui.ease((t - 0.15) / 0.3), (1520, 96))
    p, pd = panel(860, 240)
    pd.text((28, 18), "Del día: venta, cobro, compra y pago.", font=ui.f_num, fill=TEXTO + (255,))
    pd.text((28, 64), "También nota de crédito y devolución de compra.", font=ui.f_lbl, fill=TEXTO + (255,))
    pd.text((28, 108), "Costo de ventas y AjusteInventario:", font=ui.f_lbl, fill=TEXTO + (255,))
    pd.text((28, 148), "borrador, hasta que el contador los pida.", font=ui.f_lbl, fill=GRIS + (255,))
    ui.pegar(im, p, ui.ease((t - 0.45) / 0.35), (1000, 700))
    return im


def escena_cierre_abrir(t):
    im, d = escena_base(PAN / "cierre-abrir.png", "Pantalla Cierre. Ejercicio y mes, y luego Abrir período.")
    ui.pegar(im, chip_caso("4  ·  abrir el mes"), ui.ease((t - 0.15) / 0.3), (1480, 96))
    ui.pegar(im, nota("2026  ·  Septiembre  ·  pulse Abrir período", VERDE_SUAVE, VERDE, 720), ui.ease((t - 0.5) / 0.3), (1080, 780))
    return im


def escena_cierre(t):
    im, d = escena_base(PAN / "cierre-abierto.png", "Septiembre quedó abierto. Al terminar el mes: pre-cierre y Cerrar período.")
    ui.pegar(im, chip_caso("4  ·  el mes"), ui.ease((t - 0.15) / 0.3), (1560, 96))
    ui.pegar(im, nota("09 / 2026   ·   Abierto", VERDE_SUAVE, VERDE, 480), ui.ease((t - 0.5) / 0.3), (1340, 780))
    return im


def escena_factura_activa(t):
    im, d = escena_base(FAC / "11-factura-activada.png", "Pantalla Facturación. La caja está activa. Aquí nace el caso de venta.")
    ui.pegar(im, chip_caso("Caso 1  ·  venta"), ui.ease((t - 0.1) / 0.3), (1520, 96))
    p, pd = panel(780, 160)
    pd.text((28, 22), "Cajero Costa PetsAdmin  ·  Apertura 7.", font=ui.f_num, fill=TEXTO + (255,))
    pd.text((28, 72), "Emisor: Rafael Alberto Martinez Quesada.", font=ui.f_lbl, fill=GRIS + (255,))
    pd.text((28, 112), "La preventa no se contabiliza. Esta sí, al facturar.", font=ui.f_lbl, fill=TEXTO + (255,))
    ui.pegar(im, p, ui.ease((t - 0.45) / 0.35), (1080, 760))
    return im


def escena_cliente(t):
    im, d = escena_base(FAC / "12-factura-cliente.png", "El cliente de este caso: Adrián José Chaves Hernández.")
    ui.pegar(im, chip_caso("Caso 1  ·  el cliente"), ui.ease((t - 0.1) / 0.3), (1480, 96))
    p, pd = panel(720, 150)
    pd.text((28, 20), "Cédula 1-1599-0250.", font=ui.f_num, fill=TEXTO + (255,))
    pd.text((28, 68), "Contado, colones, bodega Facturacion.", font=ui.f_lbl, fill=TEXTO + (255,))
    pd.text((28, 108), "El asiento usará este cliente en CxC.", font=ui.f_lbl, fill=GRIS + (255,))
    ui.pegar(im, p, ui.ease((t - 0.4) / 0.35), (1140, 760))
    return im


def escena_buscador(t):
    im, d = escena_base(FAC / "13-buscador-articulos.png", "Se buscan los artículos de la factura. Los precios salen de aquí.")
    ui.pegar(im, chip_caso("Caso 1  ·  el detalle"), ui.ease((t - 0.1) / 0.3), (1480, 96))
    p, pd = panel(640, 140)
    pd.text((24, 18), "MAN-UND   ₡113,00", font=ui.f_num, fill=TEXTO + (255,))
    pd.text((24, 62), "MAN-CAJ   ₡1.017,00", font=ui.f_num, fill=TEXTO + (255,))
    pd.text((24, 102), "Luego el IVA del 13 %.", font=ui.f_lbl, fill=GRIS + (255,))
    ui.pegar(im, p, ui.ease((t - 0.4) / 0.35), (1220, 760))
    return im


def escena_venta(t):
    im, d = escena_base(
        FAC / "14-factura-detalle.png",
        "Factura lista. Subtotal ₡1.130,00  ·  IVA ₡146,90  ·  Total ₡1.276,90",
    )
    ui.pegar(im, chip_caso("Caso 1  ·  el asiento"), ui.ease((t - 0.1) / 0.3), (1480, 96))
    p, pd = panel(760, 340)
    pd.text((24, 16), "Lo que deja en el libro", font=ui.f_num, fill=TIERRA + (255,))
    lineas = [
        ("Débito", "1.1.4.1  Por cobrar", 1276.90, True),
        ("Crédito", "4.1.2  Ventas", 1130.00, False),
        ("Crédito", "2.1.3.1  IVA", 146.90, False),
    ]
    for i, args in enumerate(lineas):
        ui.pegar(p, fila_corta(*args, 710), ui.ease((t - 0.7 - i * 0.28) / 0.28), (20, 62 + i * 58))
    if t > 2.0:
        ui.pegar(p, nota("Debe = Haber   ₡1.276,90", VERDE_SUAVE, VERDE, 710), ui.ease((t - 2.0) / 0.3), (20, 250))
    ui.pegar(im, p, ui.ease((t - 0.35) / 0.35), (1120, 620))
    return im


def escena_bandeja_venta(t):
    im, d = escena_base(
        CAP / "bandeja.png",
        "Bandeja. Electrónica: el evento espera Aceptado o Aceptado parcial.",
    )
    ui.pegar(im, chip_caso("Caso 1  ·  Bandeja"), ui.ease((t - 0.1) / 0.3), (1500, 96))
    p, pd = panel(1000, 300)
    pd.text((28, 16), "Eventos y Pólizas, en esta pantalla.", font=ui.f_num, fill=TEXTO + (255,))
    pd.text((28, 60), "VentaEmitida  ·  Adrián  ·  ₡1.276,90", font=ui.f_lbl, fill=TEXTO + (255,))
    pd.text((28, 100), "Tiquete interno: al emitir. Electrónica: Hacienda.", font=ui.f_lbl, fill=GRIS + (255,))
    pd.text((28, 144), "Reintentar si la plantilla sigue en borrador.", font=ui.f_lbl, fill=TEXTO + (255,))
    pd.text((28, 184), "Nueva póliza manual está en la pestaña Pólizas.", font=ui.f_lbl, fill=GRIS + (255,))
    pd.text((28, 228), "Reversar pide la contraseña.", font=ui.f_lbl, fill=GRIS + (255,))
    ui.pegar(im, p, ui.ease((t - 0.35) / 0.35), (480, 620))
    return im


def escena_dia(t):
    im, d = escena_base(
        CAP / "bandeja.png",
        "El día entero deja rastro aquí: venta, cobro, compra, pago y nota de crédito.",
    )
    ui.pegar(im, chip_caso("El día  ·  Bandeja"), ui.ease((t - 0.1) / 0.3), (1480, 96))
    filas = [
        ("VentaEmitida", "La factura de Adrián, cuando Hacienda acepta."),
        ("CobroAplicado", "Al confirmar el cobro. El cheque pendiente espera."),
        ("CompraRegistrada", "Al registrar, o al aceptar el mensaje receptor."),
        ("PagoProveedorAplicado", "Con la retención de la ficha del proveedor."),
        ("NotaCreditoEmitida", "NC de mostrador o de un faltante de Bodega."),
        ("AjusteInventario", "Toma o merma. En borrador hasta activarla."),
    ]
    p, pd = panel(1120, 420)
    for i, (tipo, detalle) in enumerate(filas):
        capa = Image.new("RGBA", (1080, 58), (0, 0, 0, 0))
        cd = ImageDraw.Draw(capa)
        cd.text((8, 8), tipo, font=ui.f_num, fill=AZUL + (255,))
        cd.text((400, 10), detalle, font=ui.f_lbl, fill=TEXTO + (255,))
        ui.pegar(p, capa, ui.ease((t - 0.25 - i * 0.18) / 0.25), (20, 12 + i * 66))
    ui.pegar(im, p, ui.ease((t - 0.2) / 0.3), (720, 540))
    return im


def escena_cola(t):
    im, d = escena_base(BOD / "07-cola-con-orden.png", "Cola de alistamiento. Factura 6, crédito, pendiente.")
    ui.pegar(im, chip_caso("Caso 2  ·  despacho"), ui.ease((t - 0.1) / 0.3), (1480, 96))
    p, pd = panel(860, 180)
    pd.text((28, 20), "Alistar no duplica la venta.", font=ui.f_num, fill=TEXTO + (255,))
    pd.text((28, 72), "Un solo VentaEmitida: el de la factura.", font=ui.f_lbl, fill=TEXTO + (255,))
    pd.text((28, 112), "Requiere despacho decide si entra a la cola.", font=ui.f_lbl, fill=GRIS + (255,))
    ui.pegar(im, p, ui.ease((t - 0.45) / 0.35), (980, 760))
    return im


def escena_faltante(t):
    im, d = escena_base(BOD / "09-empaque-alistamiento.png", "Empaque. Faltante y No entregado sí dejan póliza.")
    ui.pegar(im, chip_caso("Caso 2  ·  excepción"), ui.ease((t - 0.1) / 0.3), (1460, 96))
    p, pd = panel(900, 250)
    pd.text((28, 16), "Faltante o no entregado → NotaCreditoEmitida", font=ui.f_num, fill=ROJO + (255,))
    pd.text((28, 64), "Tiquete interno, al grabar. Electrónica, Hacienda.", font=ui.f_lbl, fill=TEXTO + (255,))
    pd.text((28, 108), "Merma: AjusteInventario, borrador, sucursal de la bodega.", font=ui.f_lbl, fill=TEXTO + (255,))
    pd.text((28, 152), "Anular empacado = el mismo camino que no entregado.", font=ui.f_lbl, fill=GRIS + (255,))
    ui.pegar(im, p, ui.ease((t - 0.45) / 0.35), (960, 690))
    return im


def escena_ruta(t):
    im, d = escena_base(BOD / "15-empaque-en-ruta.png", "En ruta. Empacar y salir esperan a que Hacienda acepte.")
    ui.pegar(im, chip_caso("Caso 2  ·  en ruta"), ui.ease((t - 0.1) / 0.3), (1500, 96))
    ui.pegar(im, nota("Sigue siendo un solo asiento de venta", VERDE_SUAVE, VERDE, 720), ui.ease((t - 0.45) / 0.3), (1080, 780))
    return im


def escena_entrega(t):
    im, d = escena_base(BOD / "16-confirmar-entrega.png", "Entregar confirma la salida. El ingreso ya estaba en la factura.")
    ui.pegar(im, chip_caso("Caso 2  ·  entregar"), ui.ease((t - 0.1) / 0.3), (1480, 96))
    p, pd = panel(780, 160)
    pd.text((28, 20), "Recibe María Rodríguez.", font=ui.f_num, fill=TEXTO + (255,))
    pd.text((28, 68), "No hay un segundo asiento de venta.", font=ui.f_lbl, fill=VERDE + (255,))
    pd.text((28, 108), "Preventa: Entregar espera a facturar.", font=ui.f_lbl, fill=GRIS + (255,))
    ui.pegar(im, p, ui.ease((t - 0.45) / 0.35), (1070, 760))
    return im


def escena_entregado(t):
    im, d = escena_base(BOD / "17-cola-entregado.png", "Entregado. El mayor no se volvió a mover.")
    ui.pegar(im, chip_caso("Caso 2  ·  listo"), ui.ease((t - 0.1) / 0.3), (1540, 96))
    ui.pegar(im, nota("Factura 6  ·  10 / 10  ·  un solo VentaEmitida", VERDE_SUAVE, VERDE, 820), ui.ease((t - 0.5) / 0.3), (980, 780))
    return im


def escena_manual(t):
    im, d = chrome_modulo("Bandeja", "Pólizas → Nueva póliza manual. Fecha en un mes abierto. Debe = haber.")
    ui.pegar(im, chip_caso("Caso 3  ·  asiento a mano"), ui.ease((t - 0.1) / 0.3), (1400, 96))
    d.rounded_rectangle((X0 + 24, 400, 1860, 920), radius=18, fill=BLANCO, outline=LINEA, width=2)
    d.text((X0 + 48, 424), "Pestaña Pólizas  ·  Nueva póliza manual", font=ui.f_num, fill=TEXTO)
    d.text((X0 + 48, 472), "Sucursal CostaPets    Fecha  30/09/2026", font=ui.f_lbl, fill=GRIS)
    lineas = [
        ("Débito", "1.1.2.1  BAC colones", 50000, True),
        ("Crédito", "1.1.1.2  Caja cobro ruta", 50000, False),
    ]
    for i, args in enumerate(lineas):
        ui.pegar(im, fila_corta(*args, 900), ui.ease((t - 0.6 - i * 0.3) / 0.3), (X0 + 48, 530 + i * 70))
    if t > 1.5:
        ui.pegar(im, nota("Debe igual al haber   ₡50.000,00", VERDE_SUAVE, VERDE, 900), ui.ease((t - 1.5) / 0.3), (X0 + 48, 700))
    return im


def escena_diario(t):
    im, d = chrome_modulo("Diario", "Diario. Un período a la vez. Aquí se leen las pólizas del mes.")
    ui.pegar(im, chip_caso("Consulta  ·  Diario"), ui.ease((t - 0.1) / 0.3), (1480, 96))
    d.rounded_rectangle((X0 + 24, 400, 1860, 920), radius=18, fill=BLANCO, outline=LINEA, width=2)
    anchos = [220, 520, 160, 260, 260]
    ui.pegar(im, fila_tabla(["Fecha", "Origen", "Líneas", "Débito", "Crédito"], anchos, True), 1, (X0 + 48, 430))
    filas = [
        ["2026-09-15", "Ventas  ·  Adrián José Chaves", "3", ui.colones(1276.90), ui.colones(1276.90)],
        ["2026-09-30", "Manual  ·  caja a banco", "2", ui.colones(50000), ui.colones(50000)],
    ]
    for i, fila in enumerate(filas):
        ui.pegar(im, fila_tabla(fila, anchos), ui.ease((t - 0.5 - i * 0.3) / 0.3), (X0 + 48, 490 + i * 56))
    d.text((X0 + 48, 640), "Período  09/2026 (Abierto)    Sucursal  CostaPets", font=ui.f_lbl, fill=GRIS)
    return im


def escena_mayor(t):
    im, d = chrome_modulo("Mayor", "Mayor. Una cuenta y un período. Saldo inicial, movimientos y saldo final.")
    ui.pegar(im, chip_caso("Consulta  ·  Mayor"), ui.ease((t - 0.1) / 0.3), (1480, 96))
    d.rounded_rectangle((X0 + 24, 400, 1860, 920), radius=18, fill=BLANCO, outline=LINEA, width=2)
    d.text((X0 + 48, 424), "1.1.4.1  —  Cuentas por cobrar comercio", font=ui.f_num, fill=TEXTO)
    anchos = [220, 520, 240, 240, 240]
    ui.pegar(im, fila_tabla(["Fecha", "Origen", "Débito", "Crédito", "Saldo"], anchos, True), 1, (X0 + 48, 480))
    ui.pegar(im, fila_tabla(["2026-09-15", "Venta  ·  Adrián", ui.colones(1276.90), "—", ui.colones(1276.90)], anchos),
             ui.ease((t - 0.5) / 0.3), (X0 + 48, 540))
    if t > 1.2:
        ui.pegar(im, nota("Saldo final   ₡1.276,90", VERDE_SUAVE, VERDE, 640), ui.ease((t - 1.2) / 0.3), (X0 + 48, 640))
    return im


def escena_cxc(t):
    im, d = chrome_modulo("Auxiliar CxC", "Auxiliar CxC. El cliente y Conciliar auxiliar contra el mayor.")
    ui.pegar(im, chip_caso("Consulta  ·  CxC"), ui.ease((t - 0.1) / 0.3), (1520, 96))
    d.rounded_rectangle((X0 + 24, 400, 1860, 920), radius=18, fill=BLANCO, outline=LINEA, width=2)
    d.text((X0 + 48, 424), "Adrián José Chaves Hernández", font=ui.f_num, fill=TEXTO)
    anchos = [220, 400, 280, 280]
    ui.pegar(im, fila_tabla(["Fecha", "Origen", "Débito", "Crédito"], anchos, True), 1, (X0 + 48, 480))
    ui.pegar(im, fila_tabla(["15/09/2026", "Ventas", ui.colones(1276.90), "—"], anchos),
             ui.ease((t - 0.45) / 0.3), (X0 + 48, 540))
    if t > 1.1:
        ui.pegar(im, nota("Conciliar auxiliar   ·   Sin diferencias", VERDE_SUAVE, VERDE, 720), ui.ease((t - 1.1) / 0.3), (X0 + 48, 640))
    d.text((X0 + 48, 740), "Auxiliar CxP e Inventario funcionan igual, con proveedor o artículo.", font=ui.f_lbl, fill=GRIS)
    return im


def escena_balanza(t):
    im, d = chrome_modulo("Balanza", "Balanza. Fecha de corte. El total debe tiene que igualar al haber. CSV disponible.")
    ui.pegar(im, chip_caso("Consulta  ·  Balanza"), ui.ease((t - 0.1) / 0.3), (1460, 96))
    d.rounded_rectangle((X0 + 24, 400, 1860, 920), radius=18, fill=BLANCO, outline=LINEA, width=2)
    d.text((X0 + 48, 420), "Fecha de corte    30/09/2026", font=ui.f_lbl, fill=GRIS)
    anchos = [180, 420, 240, 240, 240]
    ui.pegar(im, fila_tabla(["Código", "Descripción", "Débito", "Crédito", "Saldo"], anchos, True), 1, (X0 + 40, 460))
    filas = [
        ["1.1.2.1", "BAC colones", ui.colones(50000), "—", ui.colones(50000)],
        ["1.1.1.2", "Caja cobro ruta", "—", ui.colones(50000), ui.colones(-50000)],
        ["1.1.4.1", "Cuentas por cobrar", ui.colones(1276.90), "—", ui.colones(1276.90)],
        ["2.1.3.1", "IVA devengado", "—", ui.colones(146.90), ui.colones(-146.90)],
        ["4.1.2", "Ventas gravadas", "—", ui.colones(1130.00), ui.colones(-1130.00)],
    ]
    for i, fila in enumerate(filas):
        ui.pegar(im, fila_tabla(fila, anchos), ui.ease((t - 0.35 - i * 0.16) / 0.25), (X0 + 40, 510 + i * 48))
    if t > 1.8:
        ui.pegar(im, nota("Total debe  ₡51.276,90   =   Total haber  ₡51.276,90", VERDE_SUAVE, VERDE, 900),
                 ui.ease((t - 1.8) / 0.3), (X0 + 40, 770))
    return im


def escena_resultado(t):
    im, d = chrome_modulo("Estado de resultados", "Ingresos, gastos y utilidad neta. También Balance general y Flujo de efectivo.")
    ui.pegar(im, chip_caso("Consulta  ·  resultados"), ui.ease((t - 0.1) / 0.3), (1400, 96))
    d.rounded_rectangle((X0 + 24, 400, 1860, 920), radius=18, fill=BLANCO, outline=LINEA, width=2)
    d.text((X0 + 48, 424), "Del 1 al 30 de septiembre de 2026", font=ui.f_lbl, fill=GRIS)
    d.text((X0 + 48, 480), "Ingresos", font=ui.f_num, fill=AZUL)
    d.text((X0 + 80, 530), "4.1.2    Ventas gravadas", font=ui.f_lbl, fill=TEXTO)
    d.text((X0 + 980, 530), ui.colones(1130), font=ui.f_num, fill=TEXTO)
    d.text((X0 + 48, 600), "Gastos", font=ui.f_num, fill=ROJO)
    d.text((X0 + 80, 650), "El costo de ventas sigue en borrador. La compra, si hubiera, iría a inventario.", font=ui.f_lbl, fill=GRIS)
    if t > 1.2:
        ui.pegar(im, nota("Utilidad neta    ₡1.130,00     ·     Descargar CSV", VERDE_SUAVE, VERDE, 900),
                 ui.ease((t - 1.2) / 0.3), (X0 + 48, 740))
    return im


def escena_anual(t):
    im, d = chrome_modulo("Cierre anual", "Los doce meses cerrados, y después Ejecutar cierre anual.")
    ui.pegar(im, chip_caso("Cierre del ejercicio"), ui.ease((t - 0.1) / 0.3), (1460, 96))
    d.rounded_rectangle((X0 + 24, 400, 1860, 920), radius=18, fill=BLANCO, outline=LINEA, width=2)
    d.text((X0 + 48, 424), "Ejercicio 2026", font=ui.f_num, fill=TEXTO)
    meses = ["Ene", "Feb", "Mar", "Abr", "May", "Jun", "Jul", "Ago", "Sep", "Oct", "Nov", "Dic"]
    for i, mes in enumerate(meses):
        col, row = i % 6, i // 6
        x, y = X0 + 48 + col * 230, 500 + row * 100
        chip = Image.new("RGBA", (210, 80), (0, 0, 0, 0))
        cd = ImageDraw.Draw(chip)
        cd.rounded_rectangle((0, 0, 200, 72), radius=12, fill=(236, 239, 236, 255), outline=LINEA + (255,))
        cd.text((16, 8), mes, font=ui.f_num, fill=TEXTO + (255,))
        cd.text((16, 40), "Cerrado", font=ui.f_lbl, fill=VERDE + (255,))
        ui.pegar(im, chip, ui.ease((t - i * 0.08) / 0.22), (x, y))
    if t > 1.6:
        ui.pegar(im, nota("Ejecutar cierre anual    ·    pide la contraseña", VERDE, BLANCO, 720),
                 ui.ease((t - 1.6) / 0.3), (X0 + 48, 740))
    return im


def escena_reproceso(t):
    im, d = chrome_modulo("Reproceso", "Simular primero. Aprobar pide la contraseña. Bitácora y Dimensiones están en el mismo menú.")
    ui.pegar(im, chip_caso("Cierre del ciclo"), ui.ease((t - 0.1) / 0.3), (1480, 96))
    d.rounded_rectangle((X0 + 24, 400, 1860, 900), radius=18, fill=BLANCO, outline=LINEA, width=2)
    pasos = [
        "Reproceso: rango, Simular, y si cuadra, Aprobar.",
        "Bitácora: quién cambió cuentas, plantillas o períodos.",
        "Dimensiones: centros de costo, si el contador las pide.",
        "Si no aparecen, Bandeja → Eventos → Reintentar.",
    ]
    for i, texto in enumerate(pasos):
        ui.pegar(im, ui.texto_rgba(f"{i + 1}.  {texto}", ui.f_num, TEXTO, 1400),
                 ui.ease((t - 0.3 - i * 0.22) / 0.28), (X0 + 48, 440 + i * 80))
    return im


ESCENAS = [
    ("01", escena_config,
     "Recorremos el módulo de Contabilidad en SeePOS, con este emisor real. En Configuración emisor se ven los cuatro estados: sucursal encendida, libro creado, relación vigente y contabilidad efectiva activa. CostaPets ya contabiliza. La retención se elige aquí. El porcentaje no: se escribe en la ficha de cada proveedor."),
    ("02", escena_catalogo,
     "En Catálogo cuentas está el árbol de este libro: activos, pasivos, patrimonio, ingresos, costo de ventas y gastos. Cada cuenta se edita y se pueden agregar hijas. El asiento usa la cuenta hoja. La merma, cuando el contador la pida, es la seis punto uno punto seis punto ocho. Si el libro ya tenía catálogo, esa cuenta se crea a mano."),
    ("03", escena_plantillas,
     "En Plantillas están las recetas. Para el día se activan venta, cobro, compra y pago. También la nota de crédito y la devolución de compra. Se abre Versiones, se simula y se pulsa Activar. El costo de ventas y AjusteInventario quedan en borrador hasta que el contador los pida. Mientras diga Borrador, el documento se guarda y no hay póliza."),
    ("04", escena_cierre_abrir,
     "En Cierre se indica el ejercicio y el mes. Aquí, dos mil veintiséis y septiembre. Se pulsa Abrir período. Un asiento solo entra si su fecha cae en un mes abierto."),
    ("05", escena_cierre,
     "Septiembre queda abierto. Al terminar el mes se ejecuta el pre-cierre y se pulsa Cerrar período. Si hiciera falta un asiento, se reabre con la contraseña. Bloquear es definitivo. El mismo día se abre el mes siguiente."),
    ("06", escena_factura_activa,
     "Caso de venta, en Facturación, la pantalla de siempre. La caja ya está activa, cajero Costa Pets Admin, apertura siete. El emisor es Rafael Alberto Martinez Quesada. Una preventa no se contabiliza. Esta factura sí, cuando se emita o cuando Hacienda la acepte."),
    ("07", escena_cliente,
     "El cliente de este caso es Adrián José Chaves Hernández, cédula uno uno cinco nueve nueve cero dos cinco cero. Condición de venta contado, moneda colones, bodega Facturación. El asiento de cuentas por cobrar queda a nombre de este cliente."),
    ("08", escena_buscador,
     "Se buscan los artículos. Manual empaque unidad, a ciento trece colones, y la caja de diez, a mil diecisiete. Esos precios, más el trece por ciento de IVA, van a armar el total de la factura."),
    ("09", escena_venta,
     "La factura ya tiene detalle. Subtotal mil ciento treinta. IVA ciento cuarenta y seis con noventa. Total mil doscientos setenta y seis con noventa. El asiento debita cuentas por cobrar por el total, acredita las ventas por el subtotal y acredita el IVA. El debe y el haber quedan iguales. Como es electrónica, espera a Hacienda. Un tiquete interno se contabilizaría al emitirlo."),
    ("10", escena_bandeja_venta,
     "Esa venta se busca en Bandeja. Pestaña Eventos, si todavía no hay póliza. Pestaña Pólizas, cuando Hacienda aceptó. Si la plantilla sigue en borrador, el mensaje lo dice: se activa y se pulsa Reintentar. El documento no se pierde. Desde Pólizas también se crea un asiento a mano y se reversa, con la contraseña."),
    ("11", escena_dia,
     "El resto del día deja el mismo rastro. El cobro, cuando se confirma, es CobroAplicado. La compra, CompraRegistrada: sin clave al registrar, con clave al aceptar el mensaje receptor. El pago al proveedor usa la retención de su ficha. La nota de crédito, de mostrador o de un faltante de Bodega, espera Hacienda si es electrónica. La toma física y la merma usan AjusteInventario, en borrador."),
    ("12", escena_cola,
     "Caso de despacho. En Bodega, la factura seis de crédito ya está en la cola. Alistar no duplica la venta. Hay un solo asiento, el de la factura. El check Requiere despacho, en Facturación, decide si la factura entra a esta cola."),
    ("13", escena_faltante,
     "En Empaque está el artículo, lo facturado y el botón Faltante. Un faltante o un no entregado arma nota de crédito. Tiquete interno, al grabarla. Electrónica, cuando Hacienda acepta. Si hay merma, AjusteInventario, en borrador, con la sucursal de la bodega. Anular una orden empacada usa el mismo camino que no entregado."),
    ("14", escena_ruta,
     "La orden sale a ruta. Empacar y salir a ruta esperan a que Hacienda acepte la factura electrónica. Se puede alistar antes. Sigue siendo un solo asiento de venta."),
    ("15", escena_entrega,
     "Se confirma la entrega. Recibe María Rodríguez. La firma es el rastro de quién recibió. Eso no genera un segundo asiento. El ingreso ya estaba en la factura. Una preventa se alista, y no se marca Entregado hasta facturarla."),
    ("16", escena_entregado,
     "La cola queda en Entregado. Factura seis, diez de diez. El mayor no se volvió a mover."),
    ("17", escena_manual,
     "Caso de asiento a mano. En Bandeja, pestaña Pólizas, Nueva póliza manual. Sucursal CostaPets, fecha treinta de septiembre, mes abierto. Se pasan cincuenta mil de la caja al banco: débito al BAC colones y crédito a la caja de ruta. Si la cuenta es de control, el sistema pide la contraseña."),
    ("18", escena_diario,
     "En Diario se leen las pólizas del período. Septiembre abierto, sucursal CostaPets. Aparece la venta de Adrián, mil doscientos setenta y seis con noventa, y el asiento manual de caja a banco. Un período a la vez."),
    ("19", escena_mayor,
     "En Mayor se elige una cuenta. Cuentas por cobrar, uno punto uno punto cuatro punto uno, muestra el débito de la factura de Adrián y el saldo final, mil doscientos setenta y seis con noventa."),
    ("20", escena_cxc,
     "El auxiliar de cuentas por cobrar muestra a Adrián, con ese mismo débito. Conciliar auxiliar compara el saldo de la operación con el del libro. Si coinciden, no hay diferencia. Auxiliar de cuentas por pagar y auxiliar de inventario hacen lo mismo con el proveedor o el artículo. Resolver pide motivo y contraseña; no cambia el asiento solo."),
    ("21", escena_balanza,
     "La balanza, a treinta de septiembre, lista cada cuenta. Con la venta de Adrián y el pase de caja a banco, el total del debe iguala al del haber: cincuenta y un mil doscientos setenta y seis con noventa. Si no coinciden, se revisa la bandeja antes de cerrar el mes. Se puede descargar en CSV."),
    ("22", escena_resultado,
     "El estado de resultados, del uno al treinta de septiembre, muestra ventas gravadas por mil ciento treinta. El costo de ventas sigue en borrador, así que no hay gasto de costo. La utilidad neta es mil ciento treinta. En el mismo menú están Balance general y Flujo de efectivo. También se descargan en CSV."),
    ("23", escena_anual,
     "El cierre anual se hace cuando los doce meses ya están cerrados. En Cierre anual se consulta el ejercicio. Si falta un mes, o si alguno sigue abierto, no sigue. Cuando los doce dicen cerrado, se pulsa Ejecutar cierre anual y se confirma la contraseña."),
    ("24", escena_reproceso,
     "Para terminar el módulo: Reproceso vuelve a armar pólizas de un rango. Primero Simular. Si es el resultado esperado, Aprobar, con la contraseña. Bitácora lista quién cambió cuentas, plantillas o períodos. Dimensiones se usa si el contador pide un centro de costo. El resto del menú, tipos de cambio, saldos iniciales o puesta en marcha, espera a que el contador lo pida. Si un asiento no aparece, Bandeja, Eventos, y Reintentar. Así se recorre Contabilidad, de la sucursal encendida al cierre del ejercicio."),
]


async def main():
    for nombre, pintar, texto in ESCENAS:
        dest = ui.CLIPS / f"{nombre}.mp4"
        if dest.exists():
            print(nombre, "ya estaba", flush=True)
            continue
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
    final = BASE / "Capacitacion-Casos-Sistema.mp4"
    subprocess.run(
        ["ffmpeg", "-y", "-loglevel", "error", "-f", "concat", "-safe", "0", "-i", str(lista), "-c", "copy", str(final)],
        check=True, cwd=ui.CLIPS)
    print(final)


if __name__ == "__main__":
    asyncio.run(main())
