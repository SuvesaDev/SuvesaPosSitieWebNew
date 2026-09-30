# -*- coding: utf-8 -*-
"""Diapositivas 1920x1080 para el video de capacitación."""
from pathlib import Path
from PIL import Image, ImageDraw, ImageFont

BASE = Path(__file__).resolve().parent
OUT = BASE / "video"
OUT.mkdir(exist_ok=True)
FONT = "/System/Library/Fonts/Supplemental/Arial Unicode.ttf"
W, H = 1920, 1080
AZUL = (16, 114, 169)
VERDE = (31, 122, 77)
NARANJA = (238, 117, 25)
OSCURO = (31, 42, 55)
GRIS = (92, 107, 122)
BLANCO = (255, 255, 255)
FONDO = (246, 248, 250)

f_titulo = ImageFont.truetype(FONT, 58)
f_paso = ImageFont.truetype(FONT, 28)
f_item = ImageFont.truetype(FONT, 34)
f_pie = ImageFont.truetype(FONT, 24)
f_marca = ImageFont.truetype(FONT, 26)


def wrap(draw, text, font, max_w):
    palabras = text.split()
    lineas, actual = [], ""
    for pal in palabras:
        prueba = pal if not actual else actual + " " + pal
        if draw.textlength(prueba, font=font) <= max_w:
            actual = prueba
        else:
            if actual:
                lineas.append(actual)
            actual = pal
    if actual:
        lineas.append(actual)
    return lineas


def diapositiva(nombre, kicker, titulo, items, figura=None, total=14, n=1):
    im = Image.new("RGB", (W, H), FONDO)
    d = ImageDraw.Draw(im)
    d.rectangle((0, 0, W, 14), fill=VERDE)
    d.rectangle((0, 14, W, 118), fill=AZUL)
    d.text((72, 48), "SeePOS  ·  Capacitación de Contabilidad", font=f_marca, fill=BLANCO)
    d.text((W - 160, 48), f"{n} / {total}", font=f_marca, fill=(210, 228, 240))
    y = 160
    if kicker:
        d.text((72, y), kicker.upper(), font=f_paso, fill=NARANJA)
        y += 48
    for linea in wrap(d, titulo, f_titulo, 1100 if figura else 1700):
        d.text((72, y), linea, font=f_titulo, fill=OSCURO)
        y += 72
    y += 16
    texto_w = 1080 if figura else 1700
    for item in items:
        lineas = wrap(d, item, f_item, texto_w - 50)
        d.rounded_rectangle((72, y + 10, 96, y + 34), radius=6, fill=AZUL)
        for i, linea in enumerate(lineas):
            d.text((116, y), linea, font=f_item, fill=OSCURO)
            y += 46
        y += 18
    if figura:
        fig = Image.open(BASE / figura).convert("RGB")
        caja_w, caja_h = 680, 520
        fig.thumbnail((caja_w, caja_h))
        x = W - fig.width - 64
        yy = 220
        d.rounded_rectangle((x - 12, yy - 12, x + fig.width + 12, yy + fig.height + 12), radius=16, fill=BLANCO, outline=(220, 226, 232), width=2)
        im.paste(fig, (x, yy))
    d.rectangle((0, H - 64, W, H), fill=AZUL)
    d.text((72, H - 46), "El documento se guarda. El asiento espera a que la sucursal, el libro y el mes estén listos.", font=f_pie, fill=BLANCO)
    im.save(OUT / nombre, "PNG")


SLIDES = [
    dict(nombre="01.png", kicker="Módulo de Contabilidad", titulo="Cómo funciona y cómo ayuda",
         items=["El libro es del emisor. La sucursal se enciende o se apaga.",
                "La venta y la compra se guardan siempre.",
                "La póliza aparece cuando la sucursal, el libro y el mes están listos.",
                "Si algo falta, la Bandeja dice qué corregir."],
         n=1),
    dict(nombre="02.png", kicker="Las tres condiciones", titulo="Sin las tres, no hay asiento",
         items=["1. La sucursal del documento está encendida.",
                "2. El emisor tiene el libro creado.",
                "3. La relación emisor–sucursal está vigente.",
                "Las tres juntas dejan la contabilidad efectiva activa."],
         n=2),
    dict(nombre="03.png", kicker="Paso 1", titulo="Encender la sucursal",
         items=["Menú Contabilidad → Configuración emisor.",
                "Elija empresa, sucursal y emisor.",
                "Pulse Encender sucursal.",
                "Al encenderla aparece el resto del menú."],
         figura="contabilidad-puesta-en-marcha.jpg", n=3),
    dict(nombre="04.png", kicker="Paso 2", titulo="Crear el libro del emisor",
         items=["En la misma pantalla pulse Crear libro del emisor.",
                "Si la sucursal no estaba ligada, el botón la deja vigente.",
                "El libro queda en colones.",
                "Confirme: encendida, libro creado, relación vigente, efectiva activa."],
         n=4),
    dict(nombre="05.png", kicker="Paso 3", titulo="Cargar el catálogo",
         items=["Catálogo cuentas. Si el libro está vacío, cargue el catálogo Tico Foodster.",
                "Es un punto de partida: las cuentas se pueden cambiar y agregar.",
                "Las plantillas quedan en borrador. Todavía no contabilizan.",
                "En Cuentas de uso guarde retención, banco en dólares, tránsito y comisiones."],
         n=5),
    dict(nombre="06.png", kicker="Retención", titulo="El porcentaje va en el proveedor",
         items=["Compras → Proveedores → editar la ficha.",
                "Retención en porcentaje. Ejemplo: 2.",
                "Vacío o cero: a ese proveedor no se le retiene.",
                "Al pagar, el banco recibe el total menos esa retención."],
         n=6),
    dict(nombre="07.png", kicker="Paso 4", titulo="Abrir el mes",
         items=["Cierre. Elija el año y el mes.",
                "Pulse Abrir período. El mes nace abierto.",
                "El asiento entra solo si su fecha cae en un mes abierto.",
                "Al cerrar el mes, abra el siguiente el mismo día."],
         n=7),
    dict(nombre="08.png", kicker="Paso 5", titulo="Activar las plantillas",
         items=["Plantillas → Versiones → revise las cuentas → Activar.",
                "Venta: por cobrar, descuento, ventas e IVA.",
                "Compra: inventario, IVA soportado y por pagar.",
                "Cobro y pago: revise caja y banco antes de activar."],
         n=8),
    dict(nombre="09.png", kicker="Ventas", titulo="Cuándo la venta deja asiento",
         items=["Preventa: no se contabiliza. El asiento nace al facturar.",
                "Tiquete interno: al emitirlo.",
                "Factura electrónica: cuando Hacienda acepta.",
                "El cobro es otro asiento, al confirmar depósito o cheque."],
         figura="contabilidad-venta.jpg", n=9),
    dict(nombre="10.png", kicker="Compras", titulo="Cuándo la compra deja asiento",
         items=["La sucursal es la de la bodega de la compra.",
                "Sin clave fiscal: al registrar.",
                "Con clave: cuando el mensaje receptor queda aceptado.",
                "El pago al proveedor usa la retención de su ficha."],
         figura="contabilidad-compra.jpg", n=10),
    dict(nombre="11.png", kicker="Si no aparece", titulo="Mire la Bandeja, pestaña Eventos",
         items=["El documento sí se guardó.",
                "El evento dice si falta plantilla, período o sucursal.",
                "Corrija eso y pulse Reintentar.",
                "En Pólizas puede hacer un asiento manual, con debe igual al haber."],
         figura="contabilidad-bandeja.jpg", n=11),
    dict(nombre="12.png", kicker="El día a día", titulo="Cómo ayuda",
         items=["Facture, compre y pague como siempre.",
                "El libro se arma por emisor y por sucursal.",
                "La operación no se detiene si el asiento espera.",
                "Orden de arranque: sucursal, libro, catálogo, mes y plantillas."],
         n=12),
]

NARRACION = {
    "01.png": "Bienvenidos. En pocos minutos vamos a ver qué hace el módulo de Contabilidad de SeePOS, cómo se enciende y cómo ayuda en el trabajo de cada día. El libro pertenece al emisor. La sucursal es el interruptor: encendida, contabiliza; apagada, no. La ayuda concreta es esta: la venta y la compra se guardan siempre. El cajero y quien compra no se quedan trabados. El asiento, la póliza, aparece cuando la sucursal, el libro y el mes están listos. Si algo falta, una pantalla llamada Bandeja dice qué corregir.",
    "02.png": "Para que un documento deje asiento hacen falta tres cosas, juntas. Primera: la sucursal de ese documento está encendida. Segunda: el emisor ya tiene el libro creado. Tercera: la relación entre ese emisor y esa sucursal está vigente. En pantalla eso se lee como contabilidad efectiva activa. Si falta una de las tres, el documento se guarda y no hay póliza.",
    "03.png": "Paso uno. Abra el menú Contabilidad y entre a Configuración emisor. Mientras la sucursal está apagada, el menú solo muestra esta pantalla. Elija la empresa, la sucursal y el emisor. Pulse Encender sucursal. El estado pasa a Encendida, y entonces aparecen el catálogo, las plantillas, el cierre y la bandeja.",
    "04.png": "Paso dos, en la misma pantalla. Pulse Crear libro del emisor. Si esa sucursal todavía no estaba ligada al emisor, este botón la deja vigente y crea el libro en colones. Espere a ver los cuatro estados: sucursal encendida, libro creado, relación vigente, y contabilidad efectiva activa. Para dejar de contabilizar, se usa Apagar sucursal. Lo ya hecho se conserva.",
    "05.png": "Paso tres. Entre a Catálogo de cuentas. Si el libro está vacío, pulse Cargar catálogo Tico Foodster. Ese catálogo es un punto de partida. Los códigos se pueden cambiar, y se pueden agregar cuentas nuevas. La carga deja las plantillas en borrador: todavía no contabilizan. Después, en Configuración emisor, baje a Cuentas de uso. Elija la cuenta de retención, el banco en dólares, los depósitos en tránsito y las comisiones por pagar, y pulse Guardar.",
    "06.png": "El porcentaje de la retención no es el mismo para todos los proveedores. Se escribe en la ficha. Vaya a Compras, Proveedores, edite el proveedor y llene Retención, en porcentaje. Si pone dos, al pagarle se retiene el dos por ciento y el banco recibe el resto. Si deja el campo vacío, o en cero, a ese proveedor no se le retiene. Un pago de cien mil colones con el dos por ciento deja dos mil en la cuenta de retención y noventa y ocho mil en el banco.",
    "07.png": "Paso cuatro. Abra Cierre. Elija el año y el mes, y pulse Abrir período. El mes nace abierto. Un asiento solo entra si su fecha cae dentro de un mes abierto. Septiembre no entra en octubre. Cuando termine el mes, ciérrelo, y abra el mes siguiente el mismo día, para que el día primero no se quede sin período. Si necesita un asiento de un mes ya cerrado, reabra ese mes con su contraseña. Un mes bloqueado ya no se reabre.",
    "08.png": "Paso cinco, el que enciende los asientos automáticos. Abra Plantillas, pulse Versiones, revise las cuentas y pulse Activar. Mientras diga Borrador, no hay póliza. La venta sugerida debita cuentas por cobrar por el total, debita el descuento, y acredita ventas y el IVA. La compra debita inventario y el IVA soportado, y acredita cuentas por pagar. El cobro baja el por cobrar y sube la caja. El pago baja el por pagar y sale por el banco, menos la retención. Si en su sucursal el dinero entra por otra caja o sale por otro banco, cambie esa cuenta en la plantilla antes de activarla.",
    "09.png": "Ahora las ventas. Una preventa no se contabiliza: solo reserva inventario. El asiento nace cuando se factura de verdad. El tiquete interno se contabiliza al emitirlo. La factura electrónica espera a que Hacienda la deje aceptada, o aceptada parcial. El cobro es otro asiento. Si es un depósito o un cheque pendiente, la póliza espera a la confirmación.",
    "10.png": "En las compras, la sucursal que cuenta es la de la bodega de las líneas, y tiene que estar encendida y ligada al emisor. Si la compra no trae clave fiscal, el asiento nace al registrarla. Si trae clave, espera a que el mensaje receptor quede aceptado o aceptado parcial. El pago al proveedor es otro asiento, y usa el porcentaje que escribió en la ficha.",
    "11.png": "Si la venta o la compra se guardó y no ve la póliza, no se perdió. Abra Bandeja, pestaña Eventos, y lea el mensaje. Las causas más comunes son: la plantilla sigue en borrador, el mes no está abierto, la sucursal está apagada, o Hacienda todavía no acepta. Corrija eso y pulse Reintentar. Para un ajuste que no nace de una factura, en la pestaña Pólizas use Nueva póliza manual. Ponga débitos y créditos que sumen lo mismo. Si la cuenta es de control, el sistema pide su contraseña.",
    "12.png": "En el día a día, facture, compre y pague como siempre. El módulo arma el libro detrás, por emisor y por sucursal, y no frena la operación cuando el asiento tiene que esperar. El orden para arrancar es corto: encender la sucursal, crear el libro, cargar el catálogo, abrir el mes y activar las plantillas. Con eso el módulo ya está ayudando. Gracias.",
}

def main():
    total = len(SLIDES)
    for i, s in enumerate(SLIDES, 1):
        diapositiva(s["nombre"], s["kicker"], s["titulo"], s["items"], s.get("figura"), total, i)
        (OUT / s["nombre"].replace(".png", ".txt")).write_text(NARRACION[s["nombre"]], encoding="utf-8")
    print(OUT)

if __name__ == "__main__":
    main()
