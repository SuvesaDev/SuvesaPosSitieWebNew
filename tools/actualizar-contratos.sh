#!/usr/bin/env bash
# Regenera los contratos del API existente.
#
#   ./tools/actualizar-contratos.sh [url-del-api]
#
# Produce dos ficheros, separados segun la convencion de capas:
#   DTOs/Generated/SeePosDtos.cs          los tipos de datos
#   ApiConexion/Generated/SeePosApiClientes.cs   los clientes HTTP
#
# Ninguno de los dos se edita a mano. Las clases de ApiConexion/ProxyClass
# envuelven estos clientes y devuelven ResponseGeneric<T>.

set -euo pipefail

API_URL="${1:-https://devapi.pos2650.com}"
RAIZ="$(cd "$(dirname "$0")/.." && pwd)"
API="$RAIZ/src/SuvesaPosSitioAplicacion/ApiConexion"

echo "Descargando OpenAPI de $API_URL ..."
curl -sf --max-time 120 "$API_URL/swagger/v1/swagger.json" -o "$API/swagger.raw.json"

echo "Saneando el documento ..."
python3 "$RAIZ/tools/sanear-openapi.py" "$API/swagger.raw.json" "$API/swagger.json"
rm -f "$API/swagger.raw.json"

cd "$API"
echo "Generando DTOs ..."
dotnet nswag run nswag.dtos.json > /dev/null
echo "Generando clientes ..."
dotnet nswag run nswag.clientes.json > /dev/null

echo "Listo."
