# PF-002 Intentar asignar un locker ocupado

- ID: PF-002
- Objetivo relacionado: OE4
- Requerimientos relacionados: RF-03 y RF-05
- Fecha y hora: 27/11/2026, entre 05:19:24 y 05:20:07, UTC−04:00. La entrega original se guardó a las 05:19:33.6539349. El intento inválido ocurrió entre las consultas SQL de 05:19:44.1991374 y 05:20:07.4788204; no se atribuye un segundo exacto no registrado al clic.
- Rama: main durante la ejecución. Las evidencias se incorporan mediante test/prueba-funcional-pf002.
- Commit: 544d630caf0f7dbba2e8b7f20b8ac0ab931747ef, coincidente con main remoto al comenzar. Este SHA identifica el código probado; el commit posterior registra las evidencias.
- Entorno: Windows; SDK .NET 10.0.404; ASP.NET Core MVC; EF Core e Identity 10.0.15; Development; aplicación real en http://127.0.0.1:5176; SQL Server 2019 Express LocalDB 15.0.4382.1, instancia (localdb)\MSSQLLocalDB.
- Base utilizada: LockersPF002_0f1af3bd, creada exclusivamente para esta comprobación mediante la migración y el seeder existentes. Conexión: Server=(localdb)\MSSQLLocalDB;Database=LockersPF002_0f1af3bd;Integrated Security=True;TrustServerCertificate=True. No contiene credenciales SQL.
- Datos ficticios utilizados: A = DEMO-1002, Estudiante Demo Dos, Id 2; B = DEMO-1003, Estudiante Demo Tres, Id 3; Locker 02, Id 2. La sesión de operación corresponde a biblioteca.demo, PersonalBiblioteca. Las claves temporales no se incluyen en las evidencias.
- Estado inicial: ambos estudiantes tenían cero asignaciones activas y el locker 02 estaba Disponible (Estado = 0), con cero asignaciones activas.
- Acción ejecutada: abrir para B el formulario con Locker 02 seleccionado mientras estaba disponible; en otra pestaña registrar la entrega de ese mismo locker a A; verificar la asignación activa y el estado Ocupado; enviar después el formulario original de B sin recargarlo.
- Resultado esperado: rechazar la segunda entrega, conservar una sola asignación activa y la asignación de A sin cambios; mantener el locker Ocupado y mostrar el rechazo real.
- Resultado observado: el servidor rechazó el envío de B y la interfaz mostró el mensaje de rechazo. La asignación Id 2 de A mantuvo estudiante, locker, fecha de inicio y FechaHoraFin null. B continuó con cero asignaciones activas. El inventario mostró Locker 02 Ocupado.
- Mensaje real mostrado por el sistema: **El locker seleccionado no está disponible.**
- Estado del locker después del intento: Ocupado (Estado = 1).
- Número de asignaciones activas del locker después del intento: 1.
- Estado de la prueba: Aprobada.
- Defecto detectado: Ninguno en el alcance de PF-002.
- Evidencia generada: cuatro capturas auténticas de la aplicación y tres consultas de solo lectura del estado SQL, con fecha/hora y versión del servidor. El manifiesto registra SHA-256 de los archivos.
- Nombre de la captura: PF-002-rechazo-locker-ocupado.jpg.

## Procedimiento y alcance

La interfaz actual filtra los lockers ocupados al cargar el formulario. Por ello se utilizó un formulario previamente cargado, que conservaba la opción Locker 02. Su envío ejecutó la acción real POST /mostrador/entregar con el token antifalsificación normal de la aplicación. No se alteraron el DOM, el HTML, los controles, las respuestas HTTP ni la lógica para provocar el caso.

La captura del formulario desactualizado conserva la información que existía cuando se abrió, incluidas sus cero asignaciones activas. Ese contenido no describe el estado posterior del servidor: la consulta 02-antes-intento.json demuestra que, al enviarlo, el locker ya estaba ocupado por A. Después del rechazo la página se recargó y dejó de ofrecer Locker 02.

La compilación previa terminó con cero errores y cero advertencias. Se aplicó la migración existente y se inicializaron los datos ficticios. La aplicación arrancó y atendió las operaciones desde el navegador. No se corrigió ni modificó código funcional en esta comprobación. Esta es una prueba funcional específica, no una nueva ejecución de toda la suite automatizada.

## Evidencias

| Archivo | Contenido |
| --- | --- |
| 01-inicial.json | Locker disponible, A y B sin asignaciones activas. |
| PF-002-A-asignacion-original.jpg | Entrega válida a A, mensaje de éxito y asignación activa de Locker 02. |
| 02-antes-intento.json | Locker ocupado, una asignación de A y ninguna de B. |
| PF-002-B-formulario-desactualizado.jpg | Formulario real de B que conserva Locker 02 seleccionado antes de enviar el intento inválido. |
| PF-002-rechazo-locker-ocupado.jpg | Mensaje real de rechazo para B y asignación original de A aún visible. |
| PF-002-C-inventario-posterior.jpg | Inventario posterior con Locker 02 Ocupado. |
| 03-despues-intento.json | SQL posterior con una sola asignación, A intacto y B sin préstamo. |
| consulta-estado.sql | Consulta de solo lectura empleada en las tres comprobaciones. |
| manifiesto.json | Identificación del código probado y hashes de los archivos de evidencia. |

Las capturas se guardaron directamente con el formato JPEG que devuelve el navegador; no se recrearon, recortaron, retocaron ni cambiaron sus resultados. El estado de prueba se conserva en su base aislada. No se modificaron datos importantes de otras bases.

## Comparación SQL

La asignación antes y después del intento inválido conserva exactamente:

```text
Id: 2
EstudianteId: 2
Matricula: DEMO-1002
LockerId: 2
Numero: 2
FechaHoraInicio: 2026-11-27T09:19:33.6539349Z
FechaHoraFin: null
```

Los valores del enum del código probado son Disponible = 0 y Ocupado = 1. Ambas consultas posteriores a la entrega válida registran Estado = 1 y AsignacionesActivas = 1. El estudiante B conserva AsignacionesActivas = 0.

## Pie de figura sugerido

**Figura X. Resultado de PF-002 al intentar asignar un locker ocupado a otro estudiante ficticio.**

Fuente: elaboración propia a partir de la ejecución de la beta Lockers UNPHU, 27 de noviembre de 2026. Se observa el rechazo del segundo préstamo y la asignación original del Locker 02.
