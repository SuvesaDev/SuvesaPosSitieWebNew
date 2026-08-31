using SuvesaPosSitioAplicacion.Models;

namespace SuvesaPosSitioAplicacion.Class;

/// <summary>
/// Menu lateral del sistema. Portado tal cual desde SidebarData.jsx del sistema actual:
/// 8 raices y 82 nodos en total. Los titulos se conservan literalmente
/// porque son la llave contra la que el API devuelve los permisos.
///
/// GENERADO desde el arbol de React. Si el menu cambia alli, conviene regenerarlo
/// en vez de editar a mano.
/// </summary>
public static partial class MenuSeePos
{
    public static readonly IReadOnlyList<ItemMenu> Items = new ItemMenu[]
    {
        new ItemMenu
        {
            Titulo = "Inicio",
            Codigo = "INICIO",
            Ruta = "/initial",
            Icono = "bi-house-door-fill",
            Hijos = new ItemMenu[]
            {
                new ItemMenu
                {
                    Titulo = "Clientes",
                    Codigo = "INICIO.CLIENTES",
                    Ruta = "/initial/customers",
                },
                new ItemMenu
                {
                    Titulo = "Inventarios",
                    Codigo = "INICIO.INVENTARIOS",
                    Ruta = "/initial/inventory",
                },
                new ItemMenu
                {
                    Titulo = "Caja",
                    Codigo = "INICIO.CAJA",
                    Ruta = "/initial/cash/closecash",
                    Hijos = new ItemMenu[]
                    {
                        new ItemMenu
                        {
                            Titulo = "Apertura Caja",
                            Codigo = "INICIO.CAJA.APERTURA_CAJA",
                            Ruta = "/initial/cash/opencash",
                        },
                        new ItemMenu
                        {
                            Titulo = "Arqueo Caja",
                            Codigo = "INICIO.CAJA.ARQUEO_CAJA",
                            Ruta = "/initial/cash/arqueocash",
                        },
                        new ItemMenu
                        {
                            Titulo = "Cierre Caja",
                            Codigo = "INICIO.CAJA.CIERRE_CAJA",
                            Ruta = "/initial/cash/closecash",
                        },
                        new ItemMenu
                        {
                            Titulo = "Depósitos",
                            Codigo = "INICIO.CAJA.DEPOSITOS",
                            Ruta = "/initial/cash/deposits",
                            Hijos = new ItemMenu[]
                            {
                                new ItemMenu
                                {
                                    Titulo = "Pre Depósito",
                                    Codigo = "INICIO.CAJA.DEPOSITOS.PRE_DEPOSITO",
                                    Ruta = "/initial/cash/deposits/predeposits",
                                },
                                new ItemMenu
                                {
                                    Titulo = "Generar Depósito",
                                    Codigo = "INICIO.CAJA.DEPOSITOS.GENERAR_DEPOSITO",
                                    Ruta = "/initial/cash/deposits/generatedeposits",
                                },
                                new ItemMenu
                                {
                                    Titulo = "Consulta Depósitos",
                                    Codigo = "INICIO.CAJA.DEPOSITOS.CONSULTA_DEPOSITOS",
                                    Ruta = "/initial/cash/deposits/consultdeposits",
                                }
                            }
                        }
                    }
                },
                new ItemMenu
                {
                    Titulo = "Facturación",
                    Codigo = "INICIO.FACTURACION",
                    Ruta = "/initial/billing",
                },
                new ItemMenu
                {
                    Titulo = "Cobrar",
                    Codigo = "INICIO.COBRAR",
                    Ruta = "/initial/charge",
                },
                new ItemMenu
                {
                    Titulo = "Entrega a Cuenta",
                    Codigo = "INICIO.ENTREGA_A_CUENTA",
                    Ruta = "/initial/downPayment",
                },
                new ItemMenu
                {
                    Titulo = "Documentos Emitidos",
                    Codigo = "INICIO.DOCUMENTOS_EMITIDOS",
                    Ruta = "/initial/documents",
                },
                new ItemMenu
                {
                    Titulo = "Bandeja Fiscal V4.4",
                    Codigo = "INICIO.BANDEJA_FISCAL_V4_4",
                    Ruta = "/invoices/fiscal-tray",
                },
                new ItemMenu
                {
                    Titulo = "Devoluciones",
                    Codigo = "INICIO.DEVOLUCIONES",
                    Ruta = "/initial/repayment",
                },
                new ItemMenu
                {
                    Titulo = "Consulta Albaranes",
                    Codigo = "INICIO.CONSULTA_ALBARANES",
                    Ruta = "/initial/consultAlbaranes",
                }
            }
        },
        new ItemMenu
        {
            Titulo = "Compras",
            Codigo = "COMPRAS",
            Ruta = "/buys",
            Icono = "bi-cart-fill",
            Hijos = new ItemMenu[]
            {
                new ItemMenu
                {
                    Titulo = "Compra",
                    Codigo = "COMPRAS.COMPRA",
                    Ruta = "/buys/buy",
                },
                new ItemMenu
                {
                    Titulo = "Proveedores",
                    Codigo = "COMPRAS.PROVEEDORES",
                    Ruta = "/buys/providers",
                },
                new ItemMenu
                {
                    Titulo = "Cuentas por pagar",
                    Codigo = "COMPRAS.CUENTAS_POR_PAGAR",
                    Ruta = "/buys/countswihoutpay",
                },
                new ItemMenu
                {
                    Titulo = "Pedidos",
                    Codigo = "COMPRAS.PEDIDOS",
                    Ruta = "/buys/orders/warehouseorders",
                    Hijos = new ItemMenu[]
                    {
                        new ItemMenu
                        {
                            Titulo = "Pedidos a Bodega",
                            Codigo = "COMPRAS.PEDIDOS.PEDIDOS_A_BODEGA",
                            Ruta = "/buys/orders/warehouseorders",
                        },
                        new ItemMenu
                        {
                            Titulo = "Consultar Pedidos",
                            Codigo = "COMPRAS.PEDIDOS.CONSULTAR_PEDIDOS",
                            Ruta = "/buys/orders/checkorders",
                        }
                    }
                },
                new ItemMenu
                {
                    Titulo = "Bodegas",
                    Codigo = "COMPRAS.BODEGAS",
                    Ruta = "/buys/orders/wineryadjustment",
                    Hijos = new ItemMenu[]
                    {
                        new ItemMenu
                        {
                            Titulo = "Ajuste Bodega",
                            Codigo = "COMPRAS.BODEGAS.AJUSTE_BODEGA",
                            Ruta = "/buys/wineryadjustment",
                        },
                        new ItemMenu
                        {
                            Titulo = "Solicitud Bodega",
                            Codigo = "COMPRAS.BODEGAS.SOLICITUD_BODEGA",
                            Ruta = "/buys/requestWinery",
                        }
                    }
                },
                new ItemMenu
                {
                    Titulo = "Toma",
                    Codigo = "COMPRAS.TOMA",
                    Ruta = "/buys/pretake",
                    Hijos = new ItemMenu[]
                    {
                        new ItemMenu
                        {
                            Titulo = "Toma",
                            Codigo = "COMPRAS.TOMA.TOMA",
                            Ruta = "/buys/take",
                        },
                        new ItemMenu
                        {
                            Titulo = "Pretoma",
                            Codigo = "COMPRAS.TOMA.PRETOMA",
                            Ruta = "/buys/pretake",
                        },
                        new ItemMenu
                        {
                            Titulo = "Pretoma Fisica General",
                            Codigo = "COMPRAS.TOMA.PRETOMA_FISICA_GENERAL",
                            Ruta = "/buys/taxclaim",
                        }
                    }
                },
                new ItemMenu
                {
                    Titulo = "Movimientos de articulos",
                    Codigo = "COMPRAS.MOVIMIENTOS_DE_ARTICULOS",
                    Ruta = "/buys/movementitems",
                },
                new ItemMenu
                {
                    Titulo = "Orden de compra manual",
                    Codigo = "COMPRAS.ORDEN_DE_COMPRA_MANUAL",
                    Ruta = "/buys/purchaseorder",
                },
                new ItemMenu
                {
                    Titulo = "Devoluciones compra",
                    Codigo = "COMPRAS.DEVOLUCIONES_COMPRA",
                    Ruta = "/buys/purchasereturns",
                },
                new ItemMenu
                {
                    Titulo = "Gastos",
                    Codigo = "COMPRAS.GASTOS",
                    Ruta = "/buys/bills",
                },
                new ItemMenu
                {
                    Titulo = "Ajuste Inventario",
                    Codigo = "COMPRAS.AJUSTE_INVENTARIO",
                    Ruta = "/buys/inventoryadjustment",
                },
                new ItemMenu
                {
                    Titulo = "Abono Pagar",
                    Codigo = "COMPRAS.ABONO_PAGAR",
                    Ruta = "/buys/pay",
                },
                new ItemMenu
                {
                    Titulo = "Ajuste Pagar",
                    Codigo = "COMPRAS.AJUSTE_PAGAR",
                    Ruta = "/buys/payadjustment",
                },
                new ItemMenu
                {
                    Titulo = "Prestamos",
                    Codigo = "COMPRAS.PRESTAMOS",
                    Ruta = "/buys/loans",
                }
            }
        },
        new ItemMenu
        {
            // ANADIDO, no viene de SidebarData.jsx.
            //
            // Las tres rutas existen en el enrutador de React pero el menu actual no
            // tenia ninguna entrada que llevara a ellas, asi que eran inalcanzables
            // desde la barra lateral. El API si concede permiso "Consignacion".
            //
            // Va bajo Compras porque ahi viven sus rutas. El API lo declara bajo
            // "Inicio", pero eso no afecta al filtrado: los permisos casan por el
            // titulo de la PANTALLA, no por el del menu.
            Titulo = "Consignación",
            Codigo = "CONSIGNACION",
            Hijos = new ItemMenu[]
            {
                new ItemMenu
                {
                    Titulo = "Registro de Consignaciones",
                    Codigo = "CONSIGNACION.REGISTRO_DE_CONSIGNACIONES",
                    Ruta = "/buys/consignment/register",
                },
                new ItemMenu
                {
                    Titulo = "Facturacion de Consignaciones",
                    Codigo = "CONSIGNACION.FACTURACION_DE_CONSIGNACIONES",
                    Ruta = "/buys/consignment/billing",
                },
                new ItemMenu
                {
                    Titulo = "Seguimiento de Consignaciones",
                    Codigo = "CONSIGNACION.SEGUIMIENTO_DE_CONSIGNACIONES",
                    Ruta = "/buys/consignment/following",
                }
            }
        },
        new ItemMenu
        {
            Titulo = "Ventas",
            Codigo = "VENTAS",
            Ruta = "/sales",
            Icono = "bi-receipt",
            Hijos = new ItemMenu[]
            {
                new ItemMenu
                {
                    Titulo = "Facturación",
                    Codigo = "VENTAS.FACTURACION",
                    Ruta = "/sales/billing",
                },
                new ItemMenu
                {
                    Titulo = "Presupuestos",
                    Codigo = "VENTAS.PRESUPUESTOS",
                    Ruta = "/sales/budgets/proforma",
                    Hijos = new ItemMenu[]
                    {
                        new ItemMenu
                        {
                            Titulo = "Proformas o Cotización",
                            Codigo = "VENTAS.PRESUPUESTOS.PROFORMAS_O_COTIZACION",
                            Ruta = "/sales/budgets/proforma",
                        },
                        new ItemMenu
                        {
                            Titulo = "Seguimiento Cotizaciones",
                            Codigo = "VENTAS.PRESUPUESTOS.SEGUIMIENTO_COTIZACIONES",
                            Ruta = "/sales/budgets/seguimiento",
                        }
                    }
                },
                new ItemMenu
                {
                    Titulo = "Agente de ventas",
                    Codigo = "VENTAS.AGENTE_DE_VENTAS",
                    Ruta = "/sales/salesagent",
                },
                new ItemMenu
                {
                    Titulo = "Abono Cobrar",
                    Codigo = "VENTAS.ABONO_COBRAR",
                    Ruta = "/sales/collect",
                },
                new ItemMenu
                {
                    Titulo = "Ajuste Cobrar",
                    Codigo = "VENTAS.AJUSTE_COBRAR",
                    Ruta = "/sales/adjustmentcollect",
                },
                new ItemMenu
                {
                    Titulo = "Devoluciones",
                    Codigo = "VENTAS.DEVOLUCIONES",
                    Ruta = "/sales/repayment",
                }
            }
        },
        new ItemMenu
        {
            Titulo = "Utilidades",
            Codigo = "UTILIDADES",
            Ruta = "/utilities",
            Icono = "bi-tools",
            Hijos = new ItemMenu[]
            {
                new ItemMenu
                {
                    Titulo = "Rifa",
                    Codigo = "UTILIDADES.RIFA",
                    Ruta = "/utilities/raffle",
                },
                new ItemMenu
                {
                    Titulo = "Etiquetador",
                    Codigo = "UTILIDADES.ETIQUETADOR",
                    Ruta = "/utilities/tagger",
                },
                new ItemMenu
                {
                    Titulo = "Unificar codigos",
                    Codigo = "UTILIDADES.UNIFICAR_CODIGOS",
                    Ruta = "/utilities/unifycodes",
                },
                new ItemMenu
                {
                    Titulo = "Lista articulos MAG",
                    Codigo = "UTILIDADES.LISTA_ARTICULOS_MAG",
                    Ruta = "/utilities/magitemslist",
                },
                new ItemMenu
                {
                    Titulo = "Asignar Codigo Cabys",
                    Codigo = "UTILIDADES.ASIGNAR_CODIGO_CABYS",
                    Ruta = "/utilities/assigncabyscode",
                }
            }
        },
        new ItemMenu
        {
            Titulo = "Parametros",
            Codigo = "PARAMETROS",
            Ruta = "/parameters",
            Icono = "bi-gear-fill",
            Hijos = new ItemMenu[]
            {
                new ItemMenu
                {
                    Titulo = "Configuración",
                    Codigo = "PARAMETROS.CONFIGURACION",
                    Ruta = "/parameters/settings",
                },
                new ItemMenu
                {
                    Titulo = "Tipos de Factura",
                    Codigo = "PARAMETROS.TIPOS_DE_FACTURA",
                    Ruta = "/parameters/invoice-types",
                },
                new ItemMenu
                {
                    Titulo = "Tipos de Identificación",
                    Codigo = "PARAMETROS.TIPOS_DE_IDENTIFICACION",
                    Ruta = "/parameters/identification-types",
                },
                new ItemMenu
                {
                    Titulo = "Impuestos",
                    Codigo = "PARAMETROS.IMPUESTOS",
                    Ruta = "/parameters/taxes",
                },
                new ItemMenu
                {
                    Titulo = "Tipos de Cobro",
                    Codigo = "PARAMETROS.TIPOS_DE_COBRO",
                    Ruta = "/parameters/collection-types",
                },
                new ItemMenu
                {
                    Titulo = "Formas de Pago",
                    Codigo = "PARAMETROS.FORMAS_DE_PAGO",
                    Ruta = "/parameters/payment-methods",
                },
                new ItemMenu
                {
                    Titulo = "Tipos de Exoneración",
                    Codigo = "PARAMETROS.TIPOS_DE_EXONERACION",
                    Ruta = "/parameters/exemption-types",
                },
                new ItemMenu
                {
                    Titulo = "Monedas Fiscales",
                    Codigo = "PARAMETROS.MONEDAS_FISCALES",
                    Ruta = "/parameters/currencies",
                },
                new ItemMenu
                {
                    Titulo = "Denominaciones de Moneda",
                    Codigo = "PARAMETROS.DENOMINACIONES_DE_MONEDA",
                    Ruta = "/parameters/currency-denominations",
                },
                new ItemMenu
                {
                    Titulo = "Configuración de Plazos",
                    Codigo = "PARAMETROS.CONFIGURACION_DE_PLAZOS",
                    Ruta = "/parameters/payment-terms",
                },
                new ItemMenu
                {
                    Titulo = "Geografía Fiscal",
                    Codigo = "PARAMETROS.GEOGRAFIA_FISCAL",
                    Ruta = "/parameters/fiscal-geography",
                },
                new ItemMenu
                {
                    Titulo = "Emisores",
                    Codigo = "PARAMETROS.EMISORES",
                    Ruta = "/parameters/issuers",
                },
                new ItemMenu
                {
                    Titulo = "Series de Facturación",
                    Codigo = "PARAMETROS.SERIES_DE_FACTURACION",
                    Ruta = "/parameters/invoice-series",
                },
                new ItemMenu
                {
                    Titulo = "Usuarios",
                    Codigo = "PARAMETROS.USUARIOS",
                    Ruta = "/parameters/users",
                },
                new ItemMenu
                {
                    Titulo = "Roles",
                    Codigo = "PARAMETROS.ROLES",
                    Ruta = "/parameters/role",
                },
                new ItemMenu
                {
                    Titulo = "Empresas",
                    Codigo = "PARAMETROS.EMPRESAS",
                    Ruta = "/parameters/company",
                },
                new ItemMenu
                {
                    Titulo = "Surcursales",
                    Codigo = "PARAMETROS.SURCURSALES",
                    Ruta = "/parameters/branch",
                },
                new ItemMenu
                {
                    Titulo = "Bancos",
                    Codigo = "PARAMETROS.BANCOS",
                    Ruta = "/parameters/bank",
                },
                new ItemMenu
                {
                    Titulo = "Clientes Frecuentes",
                    Codigo = "PARAMETROS.CLIENTES_FRECUENTES",
                    Ruta = "/parameters/usualcustomers",
                },
                new ItemMenu
                {
                    Titulo = "Asignar Ficha Por Usuarios",
                    Codigo = "PARAMETROS.ASIGNAR_FICHA_POR_USUARIOS",
                    Ruta = "/parameters/assigntab",
                },
                new ItemMenu
                {
                    Titulo = "Tarifas",
                    Codigo = "PARAMETROS.TARIFAS",
                    Ruta = "/parameters/rates",
                },
                new ItemMenu
                {
                    Titulo = "Ubicaciones",
                    Codigo = "PARAMETROS.UBICACIONES",
                    Ruta = "/parameters/locations",
                },
                new ItemMenu
                {
                    Titulo = "Presentaciones",
                    Codigo = "PARAMETROS.PRESENTACIONES",
                    Ruta = "/parameters/presentations",
                },
                new ItemMenu
                {
                    Titulo = "Monedas",
                    Codigo = "PARAMETROS.MONEDAS",
                    Ruta = "/parameters/coins",
                },
                new ItemMenu
                {
                    Titulo = "Denominación monedas",
                    Codigo = "PARAMETROS.DENOMINACION_MONEDAS",
                    Ruta = "/parameters/denominationcoins",
                },
                new ItemMenu
                {
                    Titulo = "Bodegas",
                    Codigo = "PARAMETROS.BODEGAS",
                    Ruta = "/parameters/wineries",
                },
                new ItemMenu
                {
                    Titulo = "Areas",
                    Codigo = "PARAMETROS.AREAS",
                    Ruta = "/parameters/areas",
                },
                new ItemMenu
                {
                    Titulo = "Familias",
                    Codigo = "PARAMETROS.FAMILIAS",
                    Ruta = "/parameters/family",
                },
                new ItemMenu
                {
                    Titulo = "Categorias",
                    Codigo = "PARAMETROS.CATEGORIAS",
                    Ruta = "/parameters/category",
                },
                new ItemMenu
                {
                    Titulo = "Registro de pantalla",
                    Codigo = "PARAMETROS.REGISTRO_DE_PANTALLA",
                    Ruta = "/parameters/screenregister",
                },
                new ItemMenu
                {
                    Titulo = "Bloquea/Desbloquea bodega",
                    Codigo = "PARAMETROS.BLOQUEA_DESBLOQUEA_BODEGA",
                    Ruta = "/parameters/lockunlockwarehouse",
                },
                new ItemMenu
                {
                    Titulo = "Bloquea/desbloquea X Casa Comercial",
                    Codigo = "PARAMETROS.BLOQUEA_DESBLOQUEA_X_CASA_COMERCIAL",
                    Ruta = "/parameters/lock/unlockcommercialhouse",
                },
                new ItemMenu
                {
                    Titulo = "Translado entre puntos de venta",
                    Codigo = "PARAMETROS.TRANSLADO_ENTRE_PUNTOS_DE_VENTA",
                    Ruta = "/parameters/lock/transferpointssale",
                },
                new ItemMenu
                {
                    Titulo = "Convertir Saco por Kilos",
                    Codigo = "PARAMETROS.CONVERTIR_SACO_POR_KILOS",
                    Ruta = "/parameters/bagskilos",
                },
                new ItemMenu
                {
                    Titulo = "Categoría de acción",
                    Codigo = "PARAMETROS.CATEGORIA_DE_ACCION",
                    Ruta = "/parameters/actions",
                },
                new ItemMenu
                {
                    Titulo = "Condicciones de Uso Firmado Contado",
                    Codigo = "PARAMETROS.CONDICCIONES_DE_USO_FIRMADO_CONTADO",
                    Ruta = "/parameters/terms",
                }
            }
        },
        new ItemMenu
        {
            Titulo = "Módulo Inventario",
            Codigo = "MODULO_INVENTARIO.MODULO_INVENTARIO",
            Ruta = "/moduloInventario",
            Icono = "bi-boxes",
        },
        new ItemMenu
        {
            Titulo = "Módulo Reportes",
            Codigo = "MODULO_REPORTES.MODULO_REPORTES",
            Ruta = "/moduloReportes",
            Icono = "bi-bar-chart-fill",
        },
        new ItemMenu
        {
            Titulo = "Módulo Farmacia",
            Codigo = "MODULO_FARMACIA.MODULO_FARMACIA",
            Ruta = "/moduloFarmacia",
            Icono = "bi-capsule",
        }
    };
}
