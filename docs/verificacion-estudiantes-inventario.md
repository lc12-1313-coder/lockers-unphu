# Verificación de estudiantes e inventario

## Procedencia y alcance

La rama feat/estudiantes-inventario incorpora el inventario y el diseño de acceso de la beta local anterior. Agrega una pantalla de búsqueda de estudiantes y adapta el alta para volver al resultado de búsqueda. La navegación muestra los módulos presentes en esta incorporación.

Se conserva la separación de permisos. Estudiantes consultan el inventario y su préstamo activo. Personal y administradores registran y buscan estudiantes. Los cambios administrativos del inventario corresponden al administrador.

## Verificación funcional

Compilación con cero errores y cero advertencias. Las tres pruebas del repositorio pasaron contra SQL Server LocalDB, sin fallos ni omisiones. Dos corresponden al bloque anterior y una integra los nuevos escenarios.

La nueva prueba comprueba:

- Alta de estudiantes desde personal y administrador.
- Búsqueda por matrícula y normalización de letras.
- Rechazo de matrícula duplicada.
- Rechazo en servidor de consultas y operaciones del estudiante sobre registros de otros estudiantes.
- Alta de lockers, validación de número y rechazo de duplicados.
- Habilitación y salida de servicio de lockers libres.
- Rechazo de formularios de mantenimiento con versión desactualizada.
- Rechazo de mantenimiento de un locker ocupado.
- Consulta del préstamo propio sin datos de otros estudiantes.

Los préstamos activos utilizados para verificar privacidad se preparan directamente en la base de prueba. El flujo operativo de entrega y devolución se incorporará con el módulo de préstamos.

## Revisión visual

Se comprobaron el acceso administrativo, el inventario en escritorio y a 390 píxeles, la búsqueda de estudiante y el formulario de alta en móvil. La comprobación del ancho del documento mostró que el inventario y el formulario caben en el ancho móvil. La revisión utilizó una base aislada con datos ficticios.

## Reproducción

Configurar LOCKERS_TEST_SQL y ejecutar dotnet test con el registrador TRX indicado en el README. El informe de esta ejecución está excluido de Git dentro de TestResults, con nombre estudiantes-inventario.trx.

La prueba crea una base temporal con prefijo LockersInterfazPruebas y la elimina al terminar. La evaluación de utilidad con usuarios y la instalación en otra computadora siguen pendientes.
