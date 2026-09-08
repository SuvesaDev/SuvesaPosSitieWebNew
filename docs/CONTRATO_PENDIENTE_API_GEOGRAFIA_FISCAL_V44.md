# Contrato pendiente del API — geografía fiscal V4.4

## Motivo

La WEB necesita mostrar y editar provincia, cantón y distrito junto con su `CodigoFE`. Las rutas actuales de escritura ya reciben esos campos, pero `Geografia/getProvincias`, `getCanton` y `getDistrito` devuelven los DTOs históricos sin `CodigoFE`.

## Ruta propuesta

`GET /Geografia/Mantenimiento`

Debe devolver las tres colecciones siguientes, sin secretos ni datos de clientes:

```json
{
  "provincias": [{ "idProvincia": 1, "descripcion": "San José", "codigoFE": "1" }],
  "cantones": [{ "idCanton": 1, "idProvincia": 1, "descripcion": "San José", "codigoFE": "01" }],
  "distritos": [{ "idDistrito": 1, "idCanton": 1, "descripcion": "Carmen", "codigoFE": "01" }]
}
```

Como alternativa equivalente, puede exponerse un listado por entidad que incluya siempre su padre y `CodigoFE`:

- `GET /Geografia/Mantenimiento/Provincias`
- `GET /Geografia/Mantenimiento/Cantones?idProvincia={id}`
- `GET /Geografia/Mantenimiento/Distritos?idCanton={id}`

## Criterios para habilitar la WEB

- La respuesta debe incluir el identificador interno, el identificador del padre, descripción y `CodigoFE`.
- Los códigos deben conservar ceros a la izquierda como texto.
- Los listados deben estar autenticados y permitir solo consulta; la WEB ya usará las rutas existentes de crear/actualizar.
- Una vez disponible, la WEB podrá incorporar un único mantenimiento jerárquico con filtros de provincia y cantón.
