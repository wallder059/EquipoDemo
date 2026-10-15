# Mantenimiento Vehicular Móvil

Aplicación Android construida con **.NET MAUI** que consume la API de gestión de mantenimiento de vehículos.

## Funcionalidad

- Lista los vehículos disponibles del dueño.
- Permite elegir un vehículo y consultar su historial de mantenimientos.
- Muestra fecha, tipo de servicio y kilometraje de cada registro.
- Presenta el próximo mantenimiento sugerido cuando la API lo devuelve.
- Informa de manera clara cuando el vehículo no tiene mantenimientos registrados o existe un problema de conexión.

## Contrato esperado de la API

La dirección base está en `src/MantenimientoVehicular.Mobile/ApiSettings.cs` y está configurada para el emulador Android: `http://10.0.2.2:8000/`.

La aplicación consume los siguientes recursos:

- `GET /api/vehiculos`
- `GET /api/vehiculos/{vehiculoId}/mantenimientos`

El segundo recurso puede devolver una lista directa o un objeto como el siguiente:

```json
{
  "data": {
    "mantenimientos": [
      {
        "fecha": "2026-10-01",
        "tipo_servicio": "Cambio de aceite",
        "kilometraje": 45000
      }
    ],
    "proximo_mantenimiento": {
      "tipo_servicio": "Cambio de aceite y filtro",
      "kilometraje_sugerido": 50000,
      "fecha_sugerida": "2027-04-01"
    }
  }
}
```

## Ejecutar

1. Inicia la API en tu equipo.
2. Verifica la URL de `ApiSettings.cs`; para un teléfono físico usa la IP local del equipo en lugar de `10.0.2.2`.
3. Abre la solución en Visual Studio con la carga de trabajo **.NET MAUI** y ejecuta el destino Android.
