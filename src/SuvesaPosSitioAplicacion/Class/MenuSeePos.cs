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
public static class MenuSeePos
{
    public static readonly IReadOnlyList<ItemMenu> Items = new ItemMenu[]
    {
        new ItemMenu
        {
            Titulo = "Inicio",
            Ruta = "/initial",
            Icono = "bi-house-door-fill",
            Hijos = new ItemMenu[]
            {
                new ItemMenu
                {
                    Titulo = "Clientes",
                    Ruta = "/initial/customers",
                },
                new ItemMenu
                {
                    Titulo = "Inventarios",
                    Ruta = "/initial/inventory",
                },
                new ItemMenu
                {
                    Titulo = "Caja",
                    Ruta = "/initial/cash/closecash",
                    Hijos = new ItemMenu[]
                    {
                        new ItemMenu
                        {
                            Titulo = "Apertura Caja",
                            Ruta = "/initial/cash/opencash",
                        },
                        new ItemMenu
                        {
                            Titulo = "Arqueo Caja",
                            Ruta = "/initial/cash/arqueocash",
                        },
                        new ItemMenu
                        {
                            Titulo = "Cierre Caja",
                            Ruta = "/initial/cash/closecash",
                        },
                        new ItemMenu
                        {
                            Titulo = "Depósitos",
                            Ruta = "/initial/cash/deposits",
                            Hijos = new ItemMenu[]
                            {
                                new ItemMenu
                                {
                                    Titulo = "Pre Depósito",
                                    Ruta = "/initial/cash/deposits/predeposits",
                                },
                                new ItemMenu
                                {
                                    Titulo = "Generar Depósito",
                                    Ruta = "/initial/cash/deposits/generatedeposits",
                                },
                                new ItemMenu
                                {
                                    Titulo = "Consulta Depósitos",
                                    Ruta = "/initial/cash/deposits/consultdeposits",
                                }
                            }
                        }
                    }
                },
                new ItemMenu
                {
                    Titulo = "Facturación",
                    Ruta = "/initial/billing",
                },
                new ItemMenu
                {
                    Titulo = "Cobrar",
                    Ruta = "/initial/charge",
                },
                new ItemMenu
                {
                    Titulo = "Entrega a Cuenta",
                    Ruta = "/initial/downPayment",
                },
                new ItemMenu
                {
                    Titulo = "Documentos Emitidos",
                    Ruta = "/initial/documents",
                },
                new ItemMenu
                {
                    Titulo = "Devoluciones",
                    Ruta = "/initial/repayment",
                },
                new ItemMenu
                {
                    Titulo = "Consulta Albaranes",
                    Ruta = "/initial/consultAlbaranes",
                }
            }
        },
        new ItemMenu
        {
            Titulo = "Compras",
            Ruta = "/buys",
            Icono = "bi-cart-fill",
            Hijos = new ItemMenu[]
            {
                new ItemMenu
                {
                    Titulo = "Compra",
                    Ruta = "/buys/buy",
                },
                new ItemMenu
                {
                    Titulo = "Proveedores",
                    Ruta = "/buys/providers",
                },
                new ItemMenu
                {
                    Titulo = "Cuentas por pagar",
                    Ruta = "/buys/countswihoutpay",
                },
                new ItemMenu
                {
                    Titulo = "Pedidos",
                    Ruta = "/buys/orders/warehouseorders",
                    Hijos = new ItemMenu[]
                    {
                        new ItemMenu
                        {
                            Titulo = "Pedidos a Bodega",
                            Ruta = "/buys/orders/warehouseorders",
                        },
                        new ItemMenu
                        {
                            Titulo = "Consultar Pedidos",
                            Ruta = "/buys/orders/checkorders",
                        }
                    }
                },
                new ItemMenu
                {
                    Titulo = "Bodegas",
                    Ruta = "/buys/orders/wineryadjustment",
                    Hijos = new ItemMenu[]
                    {
                        new ItemMenu
                        {
                            Titulo = "Ajuste Bodega",
                            Ruta = "/buys/wineryadjustment",
                        },
                        new ItemMenu
                        {
                            Titulo = "Solicitud Bodega",
                            Ruta = "/buys/requestWinery",
                        }
                    }
                },
                new ItemMenu
                {
                    Titulo = "Toma",
                    Ruta = "/buys/pretake",
                    Hijos = new ItemMenu[]
                    {
                        new ItemMenu
                        {
                            Titulo = "Toma",
                            Ruta = "/buys/take",
                        },
                        new ItemMenu
                        {
                            Titulo = "Pretoma",
                            Ruta = "/buys/pretake",
                        },
                        new ItemMenu
                        {
                            Titulo = "Pretoma Fisica General",
                            Ruta = "/buys/taxclaim",
                        }
                    }
                },
                new ItemMenu
                {
                    Titulo = "Movimientos de articulos",
                    Ruta = "/buys/movementitems",
                },
                new ItemMenu
                {
                    Titulo = "Orden de compra manual",
                    Ruta = "/buys/purchaseorder",
                },
                new ItemMenu
                {
                    Titulo = "Devoluciones compra",
                    Ruta = "/buys/purchasereturns",
                },
                new ItemMenu
                {
                    Titulo = "Gastos",
                    Ruta = "/buys/bills",
                },
                new ItemMenu
                {
                    Titulo = "Ajuste Inventario",
                    Ruta = "/buys/inventoryadjustment",
                },
                new ItemMenu
                {
                    Titulo = "Abono Pagar",
                    Ruta = "/buys/pay",
                },
                new ItemMenu
                {
                    Titulo = "Ajuste Pagar",
                    Ruta = "/buys/payadjustment",
                },
                new ItemMenu
                {
                    Titulo = "Prestamos",
                    Ruta = "/buys/loans",
                }
            }
        },
        new ItemMenu
        {
            Titulo = "Ventas",
            Ruta = "/sales",
            Icono = "bi-receipt",
            Hijos = new ItemMenu[]
            {
                new ItemMenu
                {
                    Titulo = "Facturación",
                    Ruta = "/sales/billing",
                },
                new ItemMenu
                {
                    Titulo = "Presupuestos",
                    Ruta = "/sales/budgets/proforma",
                    Hijos = new ItemMenu[]
                    {
                        new ItemMenu
                        {
                            Titulo = "Proformas o Cotización",
                            Ruta = "/sales/budgets/proforma",
                        },
                        new ItemMenu
                        {
                            Titulo = "Seguimiento Cotizaciones",
                            Ruta = "/sales/budgets/seguimiento",
                        }
                    }
                },
                new ItemMenu
                {
                    Titulo = "Agente de ventas",
                    Ruta = "/sales/salesagent",
                },
                new ItemMenu
                {
                    Titulo = "Abono Cobrar",
                    Ruta = "/sales/collect",
                },
                new ItemMenu
                {
                    Titulo = "Ajuste Cobrar",
                    Ruta = "/sales/adjustmentcollect",
                },
                new ItemMenu
                {
                    Titulo = "Devoluciones",
                    Ruta = "/sales/repayment",
                }
            }
        },
        new ItemMenu
        {
            Titulo = "Utilidades",
            Ruta = "/utilities",
            Icono = "bi-tools",
            Hijos = new ItemMenu[]
            {
                new ItemMenu
                {
                    Titulo = "Rifa",
                    Ruta = "/utilities/raffle",
                },
                new ItemMenu
                {
                    Titulo = "Etiquetador",
                    Ruta = "/utilities/tagger",
                },
                new ItemMenu
                {
                    Titulo = "Unificar codigos",
                    Ruta = "/utilities/unifycodes",
                },
                new ItemMenu
                {
                    Titulo = "Lista articulos MAG",
                    Ruta = "/utilities/magitemslist",
                },
                new ItemMenu
                {
                    Titulo = "Asignar Codigo Cabys",
                    Ruta = "/utilities/assigncabyscode",
                }
            }
        },
        new ItemMenu
        {
            Titulo = "Parametros",
            Ruta = "/parameters",
            Icono = "bi-gear-fill",
            Hijos = new ItemMenu[]
            {
                new ItemMenu
                {
                    Titulo = "Configuración",
                    Ruta = "/parameters/settings",
                },
                new ItemMenu
                {
                    Titulo = "Usuarios",
                    Ruta = "/parameters/users",
                },
                new ItemMenu
                {
                    Titulo = "Roles",
                    Ruta = "/parameters/role",
                },
                new ItemMenu
                {
                    Titulo = "Empresas",
                    Ruta = "/parameters/company",
                },
                new ItemMenu
                {
                    Titulo = "Surcursales",
                    Ruta = "/parameters/branch",
                },
                new ItemMenu
                {
                    Titulo = "Bancos",
                    Ruta = "/parameters/bank",
                },
                new ItemMenu
                {
                    Titulo = "Clientes Frecuentes",
                    Ruta = "/parameters/usualcustomers",
                },
                new ItemMenu
                {
                    Titulo = "Asignar Ficha Por Usuarios",
                    Ruta = "/parameters/assigntab",
                },
                new ItemMenu
                {
                    Titulo = "Tarifas",
                    Ruta = "/parameters/rates",
                },
                new ItemMenu
                {
                    Titulo = "Ubicaciones",
                    Ruta = "/parameters/locations",
                },
                new ItemMenu
                {
                    Titulo = "Presentaciones",
                    Ruta = "/parameters/presentations",
                },
                new ItemMenu
                {
                    Titulo = "Monedas",
                    Ruta = "/parameters/coins",
                },
                new ItemMenu
                {
                    Titulo = "Denominación monedas",
                    Ruta = "/parameters/denominationcoins",
                },
                new ItemMenu
                {
                    Titulo = "Bodegas",
                    Ruta = "/parameters/wineries",
                },
                new ItemMenu
                {
                    Titulo = "Areas",
                    Ruta = "/parameters/areas",
                },
                new ItemMenu
                {
                    Titulo = "Familias",
                    Ruta = "/parameters/family",
                },
                new ItemMenu
                {
                    Titulo = "Categorias",
                    Ruta = "/parameters/category",
                },
                new ItemMenu
                {
                    Titulo = "Registro de pantalla",
                    Ruta = "/parameters/screenregister",
                },
                new ItemMenu
                {
                    Titulo = "Bloquea/Desbloquea bodega",
                    Ruta = "/parameters/lockunlockwarehouse",
                },
                new ItemMenu
                {
                    Titulo = "Bloquea/desbloquea X Casa Comercial",
                    Ruta = "/parameters/lock/unlockcommercialhouse",
                },
                new ItemMenu
                {
                    Titulo = "Translado entre puntos de venta",
                    Ruta = "/parameters/lock/transferpointssale",
                },
                new ItemMenu
                {
                    Titulo = "Convertir Saco por Kilos",
                    Ruta = "/parameters/bagskilos",
                },
                new ItemMenu
                {
                    Titulo = "Categoría de acción",
                    Ruta = "/parameters/actions",
                },
                new ItemMenu
                {
                    Titulo = "Condicciones de Uso Firmado Contado",
                    Ruta = "/parameters/terms",
                }
            }
        },
        new ItemMenu
        {
            Titulo = "Módulo Inventario",
            Ruta = "/moduloInventario",
            Icono = "bi-boxes",
        },
        new ItemMenu
        {
            Titulo = "Módulo Reportes",
            Ruta = "/moduloReportes",
            Icono = "bi-bar-chart-fill",
        },
        new ItemMenu
        {
            Titulo = "Módulo Farmacia",
            Ruta = "/moduloFarmacia",
            Icono = "bi-capsule",
        }
    };
}
