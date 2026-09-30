# -*- coding: utf-8 -*-
"""Video de capacitación sobre capturas reales del sistema, con movimiento."""
import asyncio
import subprocess
from pathlib import Path

from PIL import Image, ImageDraw, ImageFont
import edge_tts

BASE = Path("/Users/amartinez/Downloads/Git/SuvesaPosSitieWebNew/docs/manual-contabilidad")
CAP = BASE / "video" / "capturas"
OUT = BASE / "video" / "clips"
OUT.mkdir(parents=True, exist_ok=True)
FONT = "/System/Library/Fonts/Supplemental/Arial Unicode.ttf"
VOZ = "es-CR-MariaNeural"

ESCENAS = [
    ("01", "config.png", 0.02, 0.22,
     "Mire la pantalla real. La contabilidad ya está activa.",
     "Esta es Configuración emisor, dentro de SeePOS. Arriba se ven los cuatro estados que tienen que estar listos: la sucursal encendida, el libro creado, la relación vigente y la contabilidad efectiva activa. Cuando esos cuatro están en verde, el módulo ya puede ayudar. La venta y la compra se siguen guardando igual. Lo que cambia es que, además, dejan una póliza en el libro del emisor."),
    ("02", "config.png", 0.28, 0.62,
     "Las cuentas de uso se eligen aquí, y se pueden cambiar.",
     "Baje en la misma pantalla hasta Cuentas de uso. Aquí el libro ya tiene la retención en la cuenta dos punto uno punto tres punto dos, el banco en dólares y el tránsito. Esas cuentas son un punto de partida. Si mañana el contador quiere otra, la elige en la lista y pulsa Guardar cuentas de uso. El porcentaje de la retención no vive aquí: se escribe en la ficha de cada proveedor. Vacío o cero significa que a ese proveedor no se le retiene."),
    ("03", "catalogo.png", 0.18, 0.48,
     "El catálogo se ve, se edita y se le pueden agregar cuentas.",
     "En Catálogo cuentas aparece el árbol real de este emisor: activos, pasivos, patrimonio, ingresos, costo de ventas y gastos. Cada cuenta tiene Agregar y Editar. Nada queda cerrado. El catálogo inicial solo sirve para arrancar. Una cuenta de grupo, como Activos, no recibe el importe. El asiento usa la cuenta hija, por ejemplo la de inventario o la de por cobrar."),
    ("04", "plantillas.png", 0.22, 0.55,
     "Cada movimiento tiene su receta. Hay que activarla.",
     "En Plantillas está la lista real. Venta emitida, compra registrada, cobro aplicado, pago a proveedor, nota de crédito, devolución de compra, comisión e importación. Mientras la versión esté en borrador, el documento se guarda y no hay póliza. Se abre Versiones, se revisan las cuentas y se pulsa Activar. La venta sugerida debita el por cobrar y el descuento, y acredita las ventas y el IVA. La compra debita inventario e IVA, y acredita el por pagar. Si la caja o el banco de su sucursal son otros, se cambian en la plantilla antes de activarla."),
    ("05", "cierre.png", 0.12, 0.32,
     "Abra el mes en el que va a trabajar.",
     "En Cierre se elige el ejercicio y el mes. Aquí el año es dos mil veintiséis y el mes es septiembre. El botón Abrir período deja ese mes abierto. Un asiento solo entra si su fecha cae dentro de un mes abierto. Al terminar septiembre, ciérrelo y abra octubre el mismo día. Si necesita registrar algo de un mes ya cerrado, se reabre con la contraseña. Un mes bloqueado ya no se reabre."),
    ("06", "bandeja.png", 0.15, 0.42,
     "Si no ve la póliza, la respuesta está en Eventos.",
     "La Bandeja tiene dos pestañas. Eventos dice por qué un documento no dejó asiento: plantilla en borrador, mes sin abrir, sucursal apagada, o Hacienda todavía no acepta. Se corrige eso y se pulsa Reintentar. El documento operativo no se pierde. En Pólizas se ven los asientos que sí se hicieron, y también se crea una póliza manual cuando el movimiento no nace de una factura. Los débitos tienen que sumar lo mismo que los créditos."),
    ("07", "plantillas.png", 0.35, 0.70,
     "Así ayuda en el día a día.",
     "Facture, compre y pague como siempre. El tiquete interno se contabiliza al emitirlo. La factura electrónica, cuando Hacienda la acepta. La compra sin clave, al registrarla. La compra con clave, cuando el mensaje receptor queda aceptado. La preventa no se contabiliza: el asiento nace al facturar. El módulo ordena el libro por emisor y por sucursal, y no frena la operación si el asiento tiene que esperar. El orden para arrancar es corto: encender la sucursal, crear el libro, revisar el catálogo, abrir el mes y activar las plantillas."),
]


def leyenda(ruta, texto):
    im = Image.new("RGBA", (1920, 170), (16, 114, 169, 230))
    d = ImageDraw.Draw(im)
    fuente = ImageFont.truetype(FONT, 36)
    palabras, linea, lineas = texto.split(), "", []
    for pal in palabras:
        prueba = pal if not linea else linea + " " + pal
        if d.textlength(prueba, font=fuente) < 1760:
            linea = prueba
        else:
            lineas.append(linea)
            linea = pal
    if linea:
        lineas.append(linea)
    y = 28
    for linea in lineas[:3]:
        d.text((70, y), linea, font=fuente, fill=(255, 255, 255, 255))
        y += 46
    im.save(ruta)


async def narrar(destino, texto):
    await edge_tts.Communicate(texto, VOZ, rate="-6%", pitch="+1Hz").save(str(destino))


def duracion(ruta):
    r = subprocess.run(
        ["ffprobe", "-v", "error", "-show_entries", "format=duration", "-of", "csv=p=0", str(ruta)],
        capture_output=True, text=True, check=True)
    return float(r.stdout.strip())


def clip(nombre, imagen, y0, y1, audio, segundos):
    frames = max(int(segundos * 30) + 12, 30)
    src = CAP / imagen
    cap = OUT / f"{nombre}-leyenda.png"
    destino = OUT / f"{nombre}.mp4"
    filtro = (
        f"[0:v]scale=2100:-1,zoompan="
        f"z='1.08+0.10*on/{frames}':"
        f"x='(iw-iw/zoom)*0.62':"
        f"y='(ih-ih/zoom)*({y0}+({y1}-{y0})*on/{frames})':"
        f"d={frames}:s=1920x1080:fps=30,"
        f"fade=t=in:st=0:d=0.35,fade=t=out:st={max(segundos-0.35, 0.4):.2f}:d=0.35[bg];"
        f"[1:v]format=rgba,fade=t=in:st=0.25:d=0.4:alpha=1[cap];"
        f"[bg][cap]overlay=0:H-h[v]"
    )
    subprocess.run([
        "ffmpeg", "-y", "-loglevel", "error",
        "-loop", "1", "-i", str(src),
        "-loop", "1", "-i", str(cap),
        "-i", str(audio),
        "-filter_complex", filtro,
        "-map", "[v]", "-map", "2:a",
        "-t", f"{segundos + 0.45:.2f}",
        "-c:v", "libx264", "-pix_fmt", "yuv420p", "-preset", "veryfast",
        "-c:a", "aac", "-b:a", "192k", "-movflags", "+faststart",
        str(destino),
    ], check=True)


async def main():
    piezas = []
    for nombre, imagen, y0, y1, titulo, narracion in ESCENAS:
        mp3 = OUT / f"{nombre}.mp3"
        await narrar(mp3, narracion)
        leyenda(OUT / f"{nombre}-leyenda.png", titulo)
        seg = duracion(mp3)
        print(nombre, f"{seg:.1f}s", flush=True)
        clip(nombre, imagen, y0, y1, mp3, seg)
        piezas.append(OUT / f"{nombre}.mp4")
    lista = OUT / "lista.txt"
    lista.write_text("".join(f"file '{p.name}'\n" for p in piezas), encoding="utf-8")
    final = BASE / "Capacitacion-Contabilidad.mp4"
    subprocess.run([
        "ffmpeg", "-y", "-loglevel", "error",
        "-f", "concat", "-safe", "0", "-i", str(lista),
        "-c", "copy", str(final),
    ], check=True, cwd=OUT)
    print(final)


if __name__ == "__main__":
    asyncio.run(main())
