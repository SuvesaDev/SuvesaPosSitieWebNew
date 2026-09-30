# -*- coding: utf-8 -*-
"""Genera el manual de usuario de Contabilidad en Word."""
from pathlib import Path

from docx import Document
from docx.enum.text import WD_ALIGN_PARAGRAPH, WD_LINE_SPACING
from docx.oxml import OxmlElement
from docx.oxml.ns import qn
from docx.shared import Cm, Inches, Pt, RGBColor

BASE = Path(__file__).resolve().parent
AZUL = RGBColor(0x10, 0x72, 0xA9)
OSCURO = RGBColor(0x1F, 0x2A, 0x37)


def sombrear(celda, hex_color):
    tc = celda._tc
    tcPr = tc.get_or_add_tcPr()
    shd = OxmlElement("w:shd")
    shd.set(qn("w:fill"), hex_color)
    shd.set(qn("w:val"), "clear")
    tcPr.append(shd)


def pie_pagina(parrafo):
    parrafo.alignment = WD_ALIGN_PARAGRAPH.RIGHT
    run = parrafo.add_run("SeePOS · Contabilidad  ·  Página ")
    run.font.size = Pt(9)
    run.font.color.rgb = RGBColor(0x5C, 0x6B, 0x7A)
    fld_begin = OxmlElement("w:fldChar")
    fld_begin.set(qn("w:fldCharType"), "begin")
    instr = OxmlElement("w:instrText")
    instr.set(qn("xml:space"), "preserve")
    instr.text = " PAGE "
    fld_end = OxmlElement("w:fldChar")
    fld_end.set(qn("w:fldCharType"), "end")
    run2 = parrafo.add_run()
    run2._r.append(fld_begin)
    run2._r.append(instr)
    run2._r.append(fld_end)
    run2.font.size = Pt(9)


def estilo(doc):
    normal = doc.styles["Normal"]
    normal.font.name = "Calibri"
    normal.font.size = Pt(11)
    normal.font.color.rgb = OSCURO
    pf = normal.paragraph_format
    pf.space_after = Pt(8)
    pf.line_spacing = 1.08
    for nombre, tam in (("Heading 1", 18), ("Heading 2", 14), ("Heading 3", 12)):
        st = doc.styles[nombre]
        st.font.name = "Calibri"
        st.font.color.rgb = AZUL
        st.font.size = Pt(tam)
        st.font.bold = True
        st.paragraph_format.space_before = Pt(16)
        st.paragraph_format.space_after = Pt(6)


def p(doc, texto, bold=False, italic=False, size=11, center=False, space_after=8, color=None):
    par = doc.add_paragraph()
    par.paragraph_format.space_after = Pt(space_after)
    par.paragraph_format.space_before = Pt(0)
    if center:
        par.alignment = WD_ALIGN_PARAGRAPH.CENTER
    run = par.add_run(texto)
    run.bold = bold
    run.italic = italic
    run.font.size = Pt(size)
    run.font.name = "Calibri"
    if color:
        run.font.color.rgb = color
    return par


def pasos(doc, items):
    for i, texto in enumerate(items, 1):
        par = doc.add_paragraph()
        par.paragraph_format.left_indent = Cm(0.75)
        par.paragraph_format.space_after = Pt(4)
        r1 = par.add_run(f"{i}.  ")
        r1.bold = True
        r1.font.color.rgb = AZUL
        r1.font.size = Pt(11)
        r2 = par.add_run(texto)
        r2.font.size = Pt(11)


def bullets(doc, items):
    for texto in items:
        par = doc.add_paragraph(texto, style="List Bullet")
        par.paragraph_format.space_after = Pt(2)


def tabla(doc, encabezados, filas, anchos=None):
    t = doc.add_table(rows=1, cols=len(encabezados))
    t.style = "Table Grid"
    t.autofit = True
    for i, h in enumerate(encabezados):
        celda = t.rows[0].cells[i]
        celda.text = ""
        run = celda.paragraphs[0].add_run(h)
        run.bold = True
        run.font.color.rgb = RGBColor(255, 255, 255)
        run.font.size = Pt(10)
        run.font.name = "Calibri"
        sombrear(celda, "1072A9")
    for fila in filas:
        row = t.add_row()
        for i, valor in enumerate(fila):
            row.cells[i].text = ""
            run = row.cells[i].paragraphs[0].add_run(valor)
            run.font.size = Pt(10)
            run.font.name = "Calibri"
    if anchos:
        for row in t.rows:
            for i, w in enumerate(anchos):
                row.cells[i].width = Inches(w)
    p(doc, "", size=6, space_after=4)
    return t


def figura(doc, nombre, pie):
    ruta = BASE / nombre
    par = doc.add_paragraph()
    par.alignment = WD_ALIGN_PARAGRAPH.CENTER
    par.paragraph_format.space_before = Pt(8)
    run = par.add_run()
    run.add_picture(str(ruta), width=Inches(6.3))
    p(doc, pie, italic=True, size=9, center=True, space_after=12, color=RGBColor(0x5C, 0x6B, 0x7A))


def main():
    doc = Document()
    sec = doc.sections[0]
    sec.page_width = Cm(21.0)
    sec.page_height = Cm(29.7)
    sec.left_margin = Cm(1.8)
    sec.right_margin = Cm(1.8)
    sec.top_margin = Cm(1.8)
    sec.bottom_margin = Cm(1.8)
    sec.header.paragraphs[0].text = ""
    hr = sec.header.paragraphs[0]
    hr.alignment = WD_ALIGN_PARAGRAPH.RIGHT
    rr = hr.add_run("SeePOS  ·  Manual de usuario")
    rr.font.size = Pt(9)
    rr.font.color.rgb = AZUL
    sec.footer.paragraphs[0].text = ""
    pie_pagina(sec.footer.paragraphs[0])
    estilo(doc)

    p(doc, "SEEPOS", bold=True, size=12, center=True, color=AZUL, space_after=2)
    p(doc, "Manual de usuario", bold=True, size=28, center=True, space_after=2)
    p(doc, "Módulo de Contabilidad", bold=True, size=18, center=True, color=AZUL, space_after=8)
    p(doc, "Guía para encender el libro, abrir los meses y ver cómo una venta, una compra o un pago se convierte en póliza.", center=True, size=12, space_after=4)
    p(doc, "Septiembre 2026", center=True, italic=True, size=11, space_after=14)

    figura(doc, "contabilidad-puesta-en-marcha.jpg", "Figura 1. Los cinco pasos, en este orden. Si se salta uno, el documento se guarda y el asiento espera.")

    # 1
    doc.add_heading("1. Para qué sirve este módulo", level=1)
    p(doc, "Contabilidad lleva el libro del emisor. Cada factura, compra, cobro o pago que ya esté listo deja una póliza: líneas de débito y crédito que cuadran, con fecha y sucursal.")
    p(doc, "El libro pertenece al emisor. Encender o apagar la contabilidad se hace en la sucursal. Para que un documento deje asiento hacen falta las tres cosas a la vez:")
    bullets(doc, [
        "La sucursal del documento está encendida.",
        "El emisor tiene libro creado.",
        "La relación entre ese emisor y esa sucursal está vigente.",
    ])
    p(doc, "Si falta alguna, la venta o la compra se guarda igual. El cajero y el comprador no se quedan trabados. El asiento aparece cuando se complete lo que faltaba, o se revisa en la Bandeja.")
    p(doc, "Cada emisor tiene su propio libro. Este módulo no junta los libros de varios emisores en uno solo.")

    doc.add_heading("1.1 Palabras que va a ver en pantalla", level=2)
    tabla(doc, ["Palabra", "Qué significa"], [
        ["Emisor", "Quien emite y a quien pertenece el libro. En ventas y compras antiguas, el campo que se llama empresa apunta a este emisor."],
        ["Sucursal", "El centro de trabajo. Es el interruptor: encendida contabiliza, apagada no."],
        ["Libro", "El juego de cuentas, plantillas y pólizas de un emisor. Moneda funcional: colones (CRC)."],
        ["Período", "Un mes del ejercicio. Solo un mes abierto recibe asientos de su fecha."],
        ["Plantilla", "La receta de un tipo de movimiento: qué cuentas y con qué importe. En borrador no contabiliza."],
        ["Evento", "El aviso de que ocurrió una venta, una compra, un pago. Vive en la Bandeja, pestaña Eventos."],
        ["Póliza o asiento", "El resultado ya cuadrado, con sus líneas. Vive en la Bandeja, pestaña Pólizas, y se consulta en Diario y Mayor."],
        ["Cuenta de movimiento", "Una cuenta hoja, donde sí se puede registrar un importe."],
        ["Cuenta de control", "Una cuenta sensible (por cobrar, inventario, por pagar, IVA). Un asiento manual que la use pide contraseña."],
    ])

    doc.add_heading("1.2 El menú", level=2)
    p(doc, "El menú Contabilidad está en la barra izquierda. Mientras la sucursal del centro abierto está apagada, solo se muestra Configuración emisor: ahí se enciende. Al encenderla aparecen el resto de las pantallas.")
    tabla(doc, ["Pantalla", "Cuándo entra usted"], [
        ["Configuración emisor", "El primer día, y cada vez que cambie una cuenta de uso."],
        ["Catálogo cuentas", "Para cargar el catálogo inicial o para agregar y corregir cuentas."],
        ["Plantillas", "Para revisar la receta de cada movimiento y activarla."],
        ["Cierre", "Para abrir el mes, cerrarlo, reabrirlo o bloquearlo."],
        ["Bandeja", "Para ver eventos y pólizas, reintentar un fallo y hacer un asiento a mano."],
        ["Diario", "Para leer las pólizas de un mes."],
        ["Mayor", "Para leer el movimiento de una cuenta en un mes."],
        ["Auxiliar CxC, CxP, Inventario", "Para comparar el saldo operativo con el mayor."],
        ["Balanza, Estado de resultados, Balance general, Flujo de efectivo", "Para consultar y descargar el estado."],
        ["Cierre anual", "Cuando los doce meses del año ya están cerrados."],
        ["Reproceso", "Para volver a armar pólizas de un rango, primero en simulación."],
        ["Dimensiones", "Solo si el contador pide clasificar líneas, por ejemplo por centro."],
        ["Bitácora", "Para ver quién cambió cuentas, plantillas, períodos o relaciones."],
    ])

    # 2
    doc.add_heading("2. Puesta en marcha", level=1)
    p(doc, "Haga estos cinco pasos una vez por emisor y por sucursal. El orden importa: sin libro no hay catálogo; sin período abierto no hay póliza; sin plantilla activa el evento se queda en error.")

    doc.add_heading("2.1 Encender la sucursal y crear el libro", level=2)
    pasos(doc, [
        "Abra Contabilidad → Configuración emisor.",
        "Elija Empresa, Sucursal y Emisor. Si solo hay uno de cada, la pantalla ya los trae.",
        "Pulse Encender sucursal. El recuadro Sucursal pasa a Encendida y el menú muestra el resto de Contabilidad.",
        "Pulse Crear libro del emisor. Si esa sucursal todavía no estaba ligada al emisor, este mismo botón la deja vigente y crea el libro en colones.",
        "Confirme los cuatro estados: Sucursal Encendida, Libro del emisor Creado, Relación emisor–sucursal Vigente, Contabilidad efectiva Activa.",
    ])
    p(doc, "Para dejar de contabilizar esa sucursal, pulse Apagar sucursal. Lo ya contabilizado se conserva. Lo nuevo se guarda en operación y no genera póliza.")
    p(doc, "Si al crear el libro aparece el aviso de que el emisor necesita una sucursal vigente, vuelva a pulsar Crear libro del emisor con la sucursal ya elegida en el tercer selector. El botón asocia esa sucursal y continúa. Reinicie el sitio si el botón todavía muestra el aviso viejo.")

    doc.add_heading("2.2 Cargar el catálogo", level=2)
    pasos(doc, [
        "Abra Contabilidad → Catálogo cuentas.",
        "Elija la misma empresa y el mismo emisor.",
        "Si el libro está vacío, verá «Este libro todavía no tiene cuentas». Pulse Cargar catálogo Tico Foodster.",
        "Espere el aviso de que el catálogo quedó cargado y las plantillas quedaron en borrador.",
    ])
    p(doc, "El botón de carga solo aparece con el libro vacío. Si ya hay cuentas, agregue o edite desde el árbol: Agregar bajo una cuenta que no admite movimiento, Editar en cualquier cuenta.")
    p(doc, "El catálogo inicial es un punto de partida. Cualquier código se puede renombrar, y se pueden crear cuentas nuevas. Una cuenta marcada Control (por cobrar, inventario, por pagar e IVA) exige contraseña cuando se usa en un asiento hecho a mano. Una cuenta de grupo no admite el importe; el asiento usa la cuenta hija.")
    p(doc, "Códigos con los que arranca, y que puede apuntar a otra cuenta desde Configuración emisor o desde la plantilla:", space_after=4)
    tabla(doc, ["Uso", "Código sugerido", "Nombre en el catálogo inicial"], [
        ["Caja chica", "1.1.1.1", "Caja chica"],
        ["Caja de ruta, usada en el cobro", "1.1.1.2", "Caja cobro ruta"],
        ["Banco en colones, usado en el pago", "1.1.2.1", "BAC colones"],
        ["Banco en dólares", "1.1.2.2", "BAC dólares"],
        ["Depósitos y cheques en tránsito", "1.1.2.3", "Depósitos y cheques en tránsito"],
        ["Cuentas por cobrar", "1.1.4.1", "Cuentas por cobrar comercio"],
        ["Inventario", "1.1.6.1", "Inventario disponible venta"],
        ["IVA soportado", "1.1.7.1", "IVA soportado"],
        ["Cuentas por pagar", "2.1.1.1", "Cuentas por pagar"],
        ["Comisiones por pagar", "2.1.2.5", "Comisiones por pagar"],
        ["IVA devengado", "2.1.3.1", "IVA devengado"],
        ["Retención a proveedores", "2.1.3.2", "Retención 2%"],
        ["Ventas gravadas", "4.1.2", "Ventas gravadas"],
        ["Descuento sobre ventas", "4.1.3", "Descuentos sobre ventas"],
    ])
    p(doc, "El nombre «Retención 2%» es el del catálogo sugerido. El porcentaje real no está fijo: se escribe en la ficha de cada proveedor, en el capítulo 4.")

    doc.add_heading("2.3 Guardar las cuentas de uso", level=2)
    pasos(doc, [
        "Vuelva a Contabilidad → Configuración emisor, con el libro ya creado.",
        "Baje hasta Cuentas de uso.",
        "Elija, de las cuentas que admiten movimiento, la de retención a proveedores, la de banco en dólares, la de depósitos y cheques en tránsito y la de comisiones por pagar.",
        "Pulse Guardar cuentas de uso.",
    ])
    p(doc, "Si acaba de cargar el catálogo en un libro vacío, esas cuatro ya vienen propuestas con los códigos de la tabla. Si el libro ya existía antes, los selectores pueden decir Sin asignar: elíjalas y guarde.")
    p(doc, "Al contabilizar, la retención y las comisiones por pagar salen de esta elección, aunque la plantilla muestre otro código. El banco en dólares y el tránsito quedan guardados para el día en que una plantilla los llame. Hoy el cobro sugerido usa la caja de ruta y el pago sugerido usa el banco en colones; ambos se cambian en la plantilla, antes de activarla.")

    doc.add_heading("2.4 Abrir el período", level=2)
    pasos(doc, [
        "Abra Contabilidad → Cierre.",
        "Elija empresa y emisor. Tiene que existir libro.",
        "En Ejercicio deje el año y en Mes el mes en el que va a contabilizar. La pantalla propone el mes de hoy.",
        "Pulse Abrir período. El mes nace abierto y aparece en la lista de arriba.",
        "Repita el paso para cada mes que vaya a usar. Puede tener varios meses abiertos.",
    ])
    p(doc, "Un asiento solo entra si su fecha cae dentro de un mes abierto. Septiembre no entra en un período de octubre.")
    p(doc, "Más adelante, en la misma pantalla, con el período elegido en la lista:")
    bullets(doc, [
        "Ejecutar pre-cierre muestra si ese mes está listo para cerrarse.",
        "Cerrar período deja de aceptar asientos nuevos en ese mes.",
        "Reabrir pide su contraseña y reabre ese mes y los posteriores que sigan cerrados. La pantalla lista cuáles son antes de confirmar.",
        "Bloquear es definitivo. Ese mes ya no se reabre.",
        "Puede adjuntar un archivo como evidencia del cierre o de la reapertura.",
    ])

    doc.add_heading("2.5 Revisar y activar las plantillas", level=2)
    pasos(doc, [
        "Abra Contabilidad → Plantillas y elija empresa y emisor.",
        "En la fila de la plantilla pulse Versiones.",
        "Pulse Detalle para ver cuentas y fórmulas, o Simular para probar importes de ejemplo.",
        "Si las cuentas son las de este emisor, pulse Activar. Confirme el aviso: a partir de ahí los eventos de ese tipo se contabilizan con esas cuentas.",
    ])
    p(doc, "Mientras la versión diga Borrador, el documento operativo se guarda y no hay póliza. Active solo las recetas que ya revisó. El cobro y el pago sugieren caja de ruta y banco en colones: cámbielos en una versión nueva si en esta sucursal el dinero entra o sale por otra cuenta, y active esa versión.")
    p(doc, "Para corregir una plantilla ya activa: Nueva versión, cambie las líneas, simule y active. La versión nueva reemplaza a la activa. Lo ya contabilizado no se reescribe solo; para eso está Reproceso, en el capítulo 9.")

    # 3 plantillas
    doc.add_heading("3. Qué hace cada plantilla", level=1)
    p(doc, "Estas son las recetas que deja el catálogo inicial. Los importes salen del documento. Una línea cuyo importe da cero no se escribe, así que un pago sin retención no exige la cuenta de retención.")

    doc.add_heading("3.1 Venta emitida", level=2)
    tabla(doc, ["Movimiento", "Cuenta sugerida", "Importe"], [
        ["Débito", "1.1.4.1 Cuentas por cobrar", "Total de la factura"],
        ["Débito", "4.1.3 Descuentos", "Descuento"],
        ["Crédito", "4.1.2 Ventas gravadas", "Subtotal"],
        ["Crédito", "2.1.3.1 IVA devengado", "Impuesto de la venta"],
    ])
    p(doc, "La identidad que cuadra es: Total = Subtotal − Descuento + Impuesto. Esta receta lleva el total a cuentas por cobrar, también en una venta de contado. El cobro, en la plantilla siguiente, baja esa cuenta y sube la caja. Si el contador prefiere que el contado vaya directo a caja, se cambia la plantilla antes de activarla.")

    doc.add_heading("3.2 Cobro aplicado", level=2)
    tabla(doc, ["Movimiento", "Cuenta sugerida", "Importe"], [
        ["Débito", "1.1.1.2 Caja cobro ruta", "Total cobrado"],
        ["Crédito", "1.1.4.1 Cuentas por cobrar", "Total cobrado"],
    ])
    p(doc, "Un depósito o un cheque que sigue pendiente no genera este asiento. Entra cuando la confirmación del instrumento queda hecha.")

    doc.add_heading("3.3 Nota de crédito emitida", level=2)
    p(doc, "Es el espejo de la venta: débito a ventas por el subtotal, débito al IVA, crédito al descuento y crédito a cuentas por cobrar por el total.")

    doc.add_heading("3.4 Compra registrada", level=2)
    tabla(doc, ["Movimiento", "Cuenta sugerida", "Importe"], [
        ["Débito", "1.1.6.1 Inventario", "Base gravada + base exenta − descuento"],
        ["Débito", "1.1.7.1 IVA soportado", "Impuesto de la compra"],
        ["Crédito", "2.1.1.1 Cuentas por pagar", "Total de la factura"],
    ])
    p(doc, "La identidad que cuadra es: Total = base gravada + base exenta − descuento + impuesto.")

    doc.add_heading("3.5 Devolución de compra", level=2)
    p(doc, "Débito a cuentas por pagar y crédito a inventario, por el total de la devolución.")

    doc.add_heading("3.6 Pago a proveedor", level=2)
    tabla(doc, ["Movimiento", "Cuenta", "Importe"], [
        ["Débito", "2.1.1.1 Cuentas por pagar", "Total pagado"],
        ["Crédito", "1.1.2.1 Banco en colones, o la cuenta que deje en la plantilla", "Total − retención"],
        ["Crédito", "La cuenta de uso Retención", "Retención calculada"],
    ])
    p(doc, "La retención es el total pagado por el porcentaje de la ficha del proveedor, dividido entre 100 y redondeado a dos decimales. Con porcentaje vacío o cero, la retención es cero y esa línea no se escribe: todo el pago va al banco.")

    doc.add_heading("3.7 Comisión liquidada", level=2)
    p(doc, "Débito al gasto de comisiones (sugerido 6.1.1.2) y crédito a la cuenta de uso Comisiones por pagar. Si esa cuenta no está elegida en Configuración emisor, la liquidación queda en la Bandeja como error de configuración.")

    doc.add_heading("3.8 Importación cerrada", level=2)
    p(doc, "Al cerrar la importación: débito a inventario y crédito a cuentas por pagar, por el costo total de la importación.")

    doc.add_heading("3.9 Movimientos que no usan esas plantillas", level=2)
    bullets(doc, [
        "Traslado de inventario entre dos sucursales del mismo emisor: débito y crédito a la misma cuenta de inventario, cada línea con su sucursal. El saldo total de la cuenta no se mueve. Hace falta la cuenta de inventario del emisor, las dos sucursales encendidas y ligadas, y un período abierto. Un traslado entre bodegas de la misma sucursal no genera póliza.",
        "Consignación entre sucursales del mismo emisor: la misma póliza de traslado. Dentro de la misma sucursal no hay asiento.",
        "Toma física: registra un evento de ajuste de inventario. El catálogo inicial no trae plantilla para ese evento. Para que deje póliza hay que crear y activar una plantilla de tipo AjusteInventario con las cuentas que indique el contador.",
        "Una conversión de producción que entra y sale por la misma cuenta y la misma bodega no genera póliza: el efecto neto es cero.",
    ])

    # 4 retencion
    doc.add_heading("4. Retención en la ficha del proveedor", level=1)
    pasos(doc, [
        "Abra Compras → Proveedores.",
        "Busque el proveedor y ábralo para editar.",
        "En Retención (%) escriba el porcentaje de ese proveedor. Ejemplo: 2 para el dos por ciento.",
        "Deje el campo vacío, o en cero, cuando a ese proveedor no se le retiene.",
        "Guarde. En la ficha, el dato se lee junto al plazo: el porcentaje, o el texto «No aplica».",
    ])
    p(doc, "Ejemplo. Pago de 100.000 colones a un proveedor con retención 2. La póliza, con la plantilla sugerida, queda así: débito a cuentas por pagar 100.000, crédito al banco 98.000 y crédito a la cuenta de retención 2.000. Otro proveedor con el campo vacío recibe los 100.000 en el banco y no genera la tercera línea.")
    p(doc, "El porcentaje se toma en el momento del pago. Si lo cambia después, los pagos ya hechos no se recalculan.")

    # 5 ventas
    doc.add_heading("5. Ventas", level=1)
    figura(doc, "contabilidad-venta.jpg", "Figura 2. La factura electrónica espera a Hacienda. El tiquete interno se contabiliza al emitirlo. La preventa no se contabiliza.")
    p(doc, "Antes de esperar un asiento, confirme el capítulo 2: sucursal encendida, libro, relación vigente, mes abierto y plantilla VentaEmitida activa.")
    doc.add_heading("5.1 Factura electrónica", level=2)
    pasos(doc, [
        "Emita la factura en Facturación, como siempre.",
        "Espere a que Hacienda la deje en Aceptado o en Aceptado parcial.",
        "Abra Contabilidad → Bandeja, pestaña Pólizas, y busque el asiento de esa venta.",
    ])
    p(doc, "Si Hacienda todavía no responde, no hay asiento. El evento se publica al aceptar. Rechazada, no se contabiliza la venta.")
    doc.add_heading("5.2 Tiquete interno", level=2)
    p(doc, "El tiquete interno no espera a Hacienda. El asiento se genera al emitirlo, con la misma plantilla VentaEmitida, si la sucursal, el libro, el período y la plantilla están listos.")
    doc.add_heading("5.3 Preventa", level=2)
    p(doc, "Una preventa reserva inventario. No es una venta fiscal y no genera asiento. El asiento nace cuando esa preventa se factura de verdad.")
    doc.add_heading("5.4 Cobro", level=2)
    pasos(doc, [
        "Active la plantilla CobroAplicado después de revisar si la caja es 1.1.1.2 u otra cuenta.",
        "Registre el cobro en Cobrar. Si el medio es depósito o cheque pendiente de confirmación, la póliza espera a la confirmación.",
        "Al confirmarse, el asiento debita la caja y acredita cuentas por cobrar.",
    ])
    doc.add_heading("5.5 Nota de crédito y anulación", level=2)
    p(doc, "La nota de crédito usa NotaCreditoEmitida y deja el espejo de la venta. Si se anula el origen y no hubo nota de crédito contabilizada, el sistema reversa la venta y, si existía, el cobro aplicado. La anulación de un cobro ya contabilizado deja la póliza contraria.")

    # 6 compras
    doc.add_heading("6. Compras", level=1)
    figura(doc, "contabilidad-compra.jpg", "Figura 3. Sin clave fiscal, el asiento nace al registrar. Con clave, espera el mensaje receptor aceptado.")
    p(doc, "La sucursal que cuenta es la de la bodega de las líneas de la compra, no otra sucursal que tenga abierta en el menú. Esa bodega tiene que pertenecer a una sucursal encendida y ligada al emisor de la compra. El período es el de la fecha de la factura de compra. La plantilla es CompraRegistrada.")
    doc.add_heading("6.1 Compra sin clave fiscal", level=2)
    pasos(doc, [
        "Registre la compra con su proveedor, bodega, bases, descuento e impuesto.",
        "Al guardar, si no hay clave fiscal, el evento CompraRegistrada se publica en ese momento.",
        "Revise la póliza en Bandeja → Pólizas.",
    ])
    doc.add_heading("6.2 Compra con clave fiscal", level=2)
    pasos(doc, [
        "Registre la compra con la clave del comprobante del proveedor.",
        "Envíe el mensaje receptor.",
        "Cuando el estado quede en Aceptado o Aceptado parcial, se publica el mismo evento.",
    ])
    p(doc, "Hasta que el mensaje no esté aceptado, la compra existe y el inventario se movió según la operación, pero no hay póliza.")
    doc.add_heading("6.3 Devolución y pago", level=2)
    bullets(doc, [
        "La devolución de compra usa DevolucionCompraRegistrada. Al anular esa devolución, si ya había póliza, se reversa.",
        "El pago al proveedor usa PagoProveedorAplicado y el porcentaje de la ficha, capítulo 4.",
        "La anulación de un pago ya contabilizado reversa esa póliza.",
    ])

    # 7 manual
    doc.add_heading("7. Asiento manual", level=1)
    p(doc, "Úselo para un movimiento que no nace de una factura, una compra o un pago: un ajuste, un traslado entre bancos, una corrección. No sustituye a las plantillas de la operación diaria.")
    pasos(doc, [
        "Abra Contabilidad → Bandeja. Elija empresa y emisor.",
        "Abra la pestaña Pólizas. El botón Nueva póliza manual está en la barra de acciones de esa pestaña.",
        "Elija la sucursal. Tiene que estar encendida y ligada al emisor del libro.",
        "Indique la fecha contable. Tiene que caer en un período abierto.",
        "En cada línea elija Débito o Crédito, una cuenta que admita movimiento, un monto mayor que cero y, si quiere, una descripción. Arranca con dos líneas; Agregar línea suma las que hagan falta.",
        "El cliente es opcional.",
        "Compruebe que la suma de débitos sea igual a la suma de créditos. Una diferencia mayor a un céntimo rechaza la póliza.",
        "Guarde. Si alguna línea usa una cuenta de control (por cobrar, inventario, por pagar o IVA), el sistema pide su contraseña antes de registrar.",
    ])
    p(doc, "La póliza queda en la misma pestaña. Detalle muestra las líneas. Reversar pide la contraseña y registra el asiento contrario; el original queda marcado como reversado.")

    # 8 bandeja
    doc.add_heading("8. Si el asiento no aparece", level=1)
    figura(doc, "contabilidad-bandeja.jpg", "Figura 4. El documento operativo se guardó. La pestaña Eventos dice por qué no hubo póliza.")
    pasos(doc, [
        "Abra Contabilidad → Bandeja, pestaña Eventos.",
        "Filtre por estado si la lista es larga.",
        "Lea el mensaje de la fila. Ahí está la causa.",
        "Corrija lo que falte: active la plantilla, abra el mes, encienda la sucursal, elija la cuenta de uso o espere la aceptación de Hacienda.",
        "Vuelva a la fila y pulse Reintentar.",
    ])
    tabla(doc, ["Lo que ve", "Qué hacer"], [
        ["No hay fila en Eventos", "La sucursal está apagada, no está ligada al emisor, el documento es preventa, la factura electrónica aún no está aceptada, o la compra con clave aún no tiene mensaje receptor aceptado."],
        ["No hay plantilla activa", "En Plantillas, abra la versión en borrador de ese tipo de evento, revísela y pulse Activar. Luego Reintentar."],
        ["No hay período abierto para la fecha", "En Cierre, abra el mes de la fecha del documento y pulse Reintentar."],
        ["Falta la cuenta de retención o de comisiones", "En Configuración emisor, elija la cuenta de uso y guarde. Luego Reintentar."],
        ["La partida no cuadra", "La plantilla no cierra con los importes del documento. Simule, corrija con una versión nueva, actívela y reintente."],
        ["La cuenta no admite movimiento", "El asiento apunta a una cuenta de grupo. Cambie la plantilla a una cuenta hija y reintente."],
        ["La póliza sí está, y quiere anularla", "Pestaña Pólizas → Reversar, con su contraseña."],
    ])

    # 9 consultas
    doc.add_heading("9. Consultar el libro", level=1)
    doc.add_heading("9.1 Diario y mayor", level=2)
    pasos(doc, [
        "Abra Diario o Mayor.",
        "Elija empresa y emisor.",
        "Elija el período. Si la lista está vacía, ese libro todavía no tiene meses: ábralos en Cierre.",
        "En el mayor, elija además la cuenta.",
        "La consulta usa las fechas de ese mes y la sucursal de su sesión cuando corresponde.",
    ])
    doc.add_heading("9.2 Estados", level=2)
    p(doc, "Balanza, Estado de resultados, Balance general y Flujo de efectivo piden empresa, emisor y una fecha de corte o un rango. Pulse Consultar. Descargar CSV baja el resultado.")
    doc.add_heading("9.3 Auxiliares", level=2)
    p(doc, "Auxiliar CxC, Auxiliar CxP y Auxiliar inventario comparan lo que dice la operación con lo que dice el mayor.")
    pasos(doc, [
        "Elija empresa y emisor y pulse Buscar.",
        "Pulse Conciliar cuando quiera una comparación nueva.",
        "Si aparece una diferencia, Resolver pide el motivo y su contraseña, y deja constancia. No cambia el asiento por sí solo: el ajuste, si hace falta, es una póliza manual o un reproceso.",
    ])
    doc.add_heading("9.4 Bitácora", level=2)
    p(doc, "Contabilidad → Bitácora lista quién creó o cambió cuentas, plantillas, períodos y la relación emisor–sucursal. Filtre por entidad, usuario o fechas.")

    # 10 cierre anual y reproceso
    doc.add_heading("10. Cierre del año", level=1)
    pasos(doc, [
        "En Cierre, abra los doce meses del ejercicio si todavía no existen.",
        "Cierre cada mes cuando ya no deba recibir asientos. Un mes abierto impide el cierre anual.",
        "Abra Contabilidad → Cierre anual, elija el ejercicio y pulse Consultar.",
        "La tabla muestra el estado de cada mes. Si faltan meses, lo dice. Si hay alguno abierto, lo dice.",
        "Cuando los doce existan y ninguno esté abierto, verá la utilidad neta del ejercicio. Pulse Ejecutar cierre anual.",
        "Después puede adjuntar el archivo de autorización.",
    ])

    doc.add_heading("11. Reproceso", level=1)
    p(doc, "Sirve para volver a armar pólizas de un rango, por ejemplo después de activar una plantilla corregida. No toca el libro en el primer paso.")
    pasos(doc, [
        "Abra Contabilidad → Reproceso.",
        "Indique el rango y pulse Simular. Lea el resultado.",
        "Si es el que espera, pulse Aprobar y confirme su contraseña.",
    ])

    doc.add_heading("12. Dimensiones", level=1)
    p(doc, "Úselas cuando el contador quiera clasificar líneas, por ejemplo por centro de costo. Si no las pidió, esta pantalla puede esperar.")
    pasos(doc, [
        "Abra Contabilidad → Dimensiones.",
        "Pulse Nueva dimensión y póngale nombre.",
        "Ábrala y pulse Nuevo valor para cada opción (sucursal, proyecto, lo que hayan definido).",
        "En la plantilla, la línea que deba llevar esa clasificación se completa al armar la versión.",
    ])

    # 13 dia a dia
    doc.add_heading("13. Un día de trabajo", level=1)
    p(doc, "Cuando la puesta en marcha ya está hecha, el día se parece a esto.")
    bullets(doc, [
        "Facture y cobre como siempre. La electrónica contabiliza al aceptarla Hacienda; el tiquete interno, al emitirlo; el cobro, al confirmarse.",
        "Registre la compra. Sin clave, el asiento nace al guardar. Con clave, al aceptar el mensaje receptor.",
        "Pague al proveedor. La retención sale de su ficha.",
        "Si algo no aparece en Pólizas, abra Eventos, lea el mensaje y pulse Reintentar cuando ya esté corregido.",
        "Al terminar el mes, ejecute el pre-cierre y cierre el período. Abra el mes siguiente el mismo día, para que lo del día uno no se quede sin período.",
    ])

    doc.add_heading("14. Lista de arranque", level=1)
    tabla(doc, ["Hecho", "Comprobación"], [
        ["☐", "Sucursal encendida en Configuración emisor."],
        ["☐", "Libro creado, relación vigente y contabilidad efectiva activa."],
        ["☐", "Catálogo cargado, o cuentas creadas a mano."],
        ["☐", "Cuentas de uso guardadas: retención, banco en dólares, tránsito y comisiones por pagar."],
        ["☐", "Mes de trabajo abierto en Cierre."],
        ["☐", "Plantillas del día revisadas y activadas: venta, cobro, compra y pago, como mínimo."],
        ["☐", "Porcentaje de retención en los proveedores que corresponda."],
        ["☐", "Una venta o compra de prueba y su póliza visible en Bandeja → Pólizas."],
    ])

    doc.add_heading("15. Preguntas frecuentes", level=1)
    doc.add_heading("El menú Contabilidad no aparece", level=2)
    p(doc, "Con la sucursal apagada el menú muestra solo Configuración emisor. Entre ahí, elija la sucursal y pulse Encender sucursal. Recargue el sitio si acaba de recibir una versión nueva del sistema.")
    doc.add_heading("La venta se guardó y no hay póliza", level=2)
    p(doc, "Mire primero si es preventa o si la factura electrónica todavía no está aceptada. Si ya debería contabilizarse, abra Bandeja → Eventos. Las causas más comunes son plantilla en borrador y mes sin abrir.")
    doc.add_heading("La compra se guardó y no hay póliza", level=2)
    p(doc, "Si tiene clave fiscal, falta el mensaje receptor en Aceptado o Aceptado parcial. Si no tiene clave, revise que la bodega pertenezca a la sucursal encendida y que el período de la fecha esté abierto.")
    doc.add_heading("¿El dos por ciento es para todos los proveedores?", level=2)
    p(doc, "Cada proveedor trae el suyo en la ficha. El catálogo solo sugiere la cuenta 2.1.3.2. Vacío o cero en la ficha significa que a ese proveedor no se le retiene.")
    doc.add_heading("Cambié una cuenta en el catálogo. ¿Cambian los asientos viejos?", level=2)
    p(doc, "Los asientos ya hechos conservan la cuenta con la que se registraron. Lo nuevo sigue la plantilla activa en ese momento. Para rehacer un rango, use Reproceso: simule, revise y apruebe.")
    doc.add_heading("¿Puedo contabilizar a mano en una cuenta de por cobrar o de inventario?", level=2)
    p(doc, "Sí, en Nueva póliza manual, confirmando su contraseña. Esas cuentas están marcadas como control precisamente para que un asiento directo no pase desapercibido.")
    doc.add_heading("Cerré el mes y necesito un asiento de ese mes", level=2)
    p(doc, "En Cierre, elija el período y pulse Reabrir. Confirme la contraseña. Si hay meses posteriores cerrados, se reabren junto con ese. Un mes bloqueado ya no se reabre.")

    p(doc, "Fin del manual.", italic=True, center=True, space_after=0)

    destino = BASE / "Manual de usuario - Contabilidad.docx"
    doc.save(destino)
    print(destino)


if __name__ == "__main__":
    main()
