# ADR 0001 — Aplicación MVC y consistencia de los préstamos

**Estado** · Aceptado para la beta académica

## Contexto

La biblioteca entrega y recibe llaves físicas. La aplicación debe conservar el historial y aplicar las reglas de acceso y préstamo aunque dos operadores actúen a la vez. El equipo necesita instalar y mantener la beta desde la terminal.

## Decisión

- Usar ASP.NET Core MVC y Razor en .NET 10 LTS. Identity gestiona cuentas, roles, hashes, bloqueo por intentos y sesiones. El alta de estudiantes es interna y usa la matrícula como nombre de usuario.
- Usar SQL Server y EF Core con migraciones versionadas. Una asignación está activa mientras su fecha final sea nula. Dos índices únicos filtrados protegen al estudiante y al locker. Una columna `rowversion` protege el estado del locker frente a cambios simultáneos. Cada entrega o devolución escribe la asignación y el locker en una sola llamada transaccional a `SaveChangesAsync`.
- El personal de biblioteca realiza entregas y devoluciones. El administrador gestiona cuentas, roles del equipo y mantenimiento de lockers libres. El estudiante consulta el inventario y su propio préstamo. Los permisos se verifican en servidor.
- Guardar fechas como `DateTimeOffset` en UTC y mostrarlas en hora de Santo Domingo. Reportar el total de asignaciones registradas y calcular el promedio con asignaciones devueltas.
- Diseñar formularios y tablas orientados al mostrador. Usar verde institucional como referencia, un verde más oscuro para controles y un acento ámbar para mantenimiento. Identificar estados con texto además del color y dar prioridad a la búsqueda por matrícula y a la lista de entregas activas. [Manual de identidad de la UNPHU](https://repositorio.unphu.edu.do/handle/123456789/1231) · [Impeccable](https://github.com/pbakaus/impeccable).

## Alternativas y consecuencias

Una SPA separada duplicaría validación, navegación y trabajo de autenticación para un flujo principalmente de formularios. MVC permite compilar el código y las vistas juntos. SQL Server requiere una instancia disponible en cada entorno y permite comprobar las restricciones y concurrencia con el mismo motor de la beta.

## Posible alojamiento en Cloudflare

La beta se ejecuta localmente. Para una evaluación remota futura, un host con Kestrel y acceso privado a SQL Server puede quedar detrás de Cloudflare Tunnel y Cloudflare Access. El equipo deberá configurar HTTPS, proxies confiables para encabezados reenviados, persistencia de claves de Data Protection y secretos del host antes de abrir el acceso. El origen seguirá ejecutando el servidor ASP.NET Core. [Cloudflare Tunnel](https://developers.cloudflare.com/tunnel/) · [Aplicaciones protegidas con Cloudflare Access](https://developers.cloudflare.com/cloudflare-one/access-controls/applications/http-apps/self-hosted-public-app/).
