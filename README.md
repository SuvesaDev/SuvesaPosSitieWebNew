# SUVESA SeePOS — sitio web

Migracion del punto de venta SeePOS de React a .NET 10 con Blazor.

## Requisitos

- .NET SDK 10.0 o superior

## Arrancar

```bash
dotnet run --project src/SuvesaPosSitioAplicacion --launch-profile http
```

Queda en <http://localhost:5199>.

## Compilar y probar

```bash
dotnet build SuvesaPosSitioWeb.slnx
dotnet test tests/SuvesaPosSitioAplicacion.Tests
```

## Configuracion

Las URLs no se versionan. Van en `appsettings.Development.json` (ignorado por git)
o en variables de entorno:

| Clave | Que es |
|---|---|
| `SeePos:ApiBaseUrl` | URL del API REST existente (desarrollo: `https://devapi.pos2650.com`) |
| `SeePos:LegacySpaUrl` | URL de la SPA React, para la convivencia de la semana 4 |

## Regenerar los contratos del API

```bash
./tools/actualizar-contratos.sh
```

## Estructura

Sigue la organizacion estandar de la casa, la misma de `FCRCASitioAplicacion`:

```
src/SuvesaPosSitioAplicacion/
  ApiConexion/        ProxyInterface + ProxyClass + Generated (clientes del API)
  Class/              tipos transversales y enumeraciones
  Controllers/        endpoints MVC, solo si hacen falta (descargas, callbacks)
  DTOs/               contratos de datos, Generated los del API
  Helpers/            envelope de respuesta, handler de autenticacion
  Models/             ViewModels de las pantallas
  Security/           sesion y permisos, en servidor
  Services/           logica de aplicacion propia del sitio
  Views/              pantallas .razor, por modulo, mas Shared
  wwwroot/            estaticos
```

## Estado

**Ola 0 — Cimientos.** Todavia no hay pantallas de negocio.
El plan por semanas y las decisiones de arquitectura estan en `CLAUDE.md`.
