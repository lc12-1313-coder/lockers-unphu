# Capturas de interfaces — sección 6.5

## Procedencia

Las seis imágenes JPEG son capturas originales de página completa tomadas desde el navegador sobre la beta implementada en el commit 38bfa01a49246b868c9c9e9696fd9fe9426c45e0. Se tomaron con cuentas ficticias y una base LocalDB aislada, LockersCapturas65_41752f13, preparada con la migración y el seeder del proyecto. La aplicación se ejecutó en http://127.0.0.1:5175.

Las imágenes documentan interfaces implementadas. No se encontraron wireframes, mockups ni prototipos previos de interfaces guardados en los archivos revisados.

Para preparar la evidencia se inició sesión como estudiante, personal y administrador. El personal registró desde la interfaz la entrega del locker 02 a DEMO-1002. Así se obtuvieron la pantalla de devolución pendiente y la consulta del préstamo propio del estudiante. El historial incluye además una devolución ficticia de 45 minutos creada por el seeder.

## Pies de figura sugeridos

| Archivo | Rol y pantalla | Pie de figura |
| --- | --- | --- |
| 01-estudiante-disponibilidad.jpg | Estudiante · /lockers | Vista de consulta del estudiante con la cantidad y el estado de los lockers disponibles, ocupados y fuera de servicio. |
| 02-estudiante-prestamo-activo.jpg | Estudiante · /lockers | Consulta del préstamo propio del estudiante ficticio, con el locker asignado y la fecha de entrega. |
| 03-personal-preparar-entrega.jpg | Personal · /mostrador?matricula=DEMO-1002 | Búsqueda del estudiante por matrícula y selección de un locker disponible antes de registrar la entrega de la llave. |
| 04-personal-devolucion-pendiente.jpg | Personal · /mostrador?matricula=DEMO-1002 | Confirmación de la entrega y listado de asignaciones activas con la opción de registrar la devolución de la llave. |
| 05-administrador-inventario.jpg | Administrador · /lockers | Inventario administrativo con alta de lockers y controles para habilitar o poner fuera de servicio casilleros libres. |
| 06-administrador-reportes.jpg | Administrador · /reportes | Reporte de asignaciones, duración promedio de los préstamos devueltos e historial reciente con datos ficticios. |

## Correspondencia con el código

- 01, 02 y 05 corresponden a src/LockersUnphu/Views/Inventario/Index.cshtml. La vista adapta la información y los controles al rol.
- 03 y 04 corresponden a src/LockersUnphu/Views/Mostrador/Index.cshtml.
- 06 corresponde a src/LockersUnphu/Views/Reportes/Index.cshtml.

El administrador y el personal pueden consultar reportes. La entrega y devolución pertenecen al personal. La pantalla de administración de cuentas del equipo sigue pendiente de incorporación a main y no forma parte de estas imágenes.

## Uso en el informe

Se pueden usar dos figuras por rol. Si el espacio es limitado, seleccionar 02 para estudiante, 04 para personal y 05 para administrador, y dejar las restantes como anexos.

Conservar la proporción original y colocar los pies de figura fuera de la imagen. La captura 01 muestra el estado anterior a la entrega. Las capturas 02, 04, 05 y 06 muestran el estado posterior, con diez lockers disponibles, uno ocupado y uno fuera de servicio. La captura 03 muestra el formulario preparado antes de confirmar la operación.

La captura 04 evidencia la devolución pendiente y su control de acción. La devolución de este préstamo todavía no se ejecutó en el escenario capturado. El promedio de 45 minutos de la captura 06 corresponde al préstamo histórico ya cerrado del seeder.

Los datos visibles son ficticios. Las imágenes se conservaron en el formato JPEG original entregado por el navegador, sin modificar el contenido de las pantallas. Los hashes SHA-256 están registrados en manifiesto.json.
