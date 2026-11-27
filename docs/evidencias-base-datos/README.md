# Evidencias reales de la base de datos — sección 6.4

## Alcance y versión revisada

Esta evidencia corresponde al código de la beta en el commit 419b03a4c08d210ec8d0e72722fa8a97018efc75. La revisión se realizó el 26 de noviembre de 2026, hora de America/La_Paz. Los archivos de resultados conservan sus marcas de captura en UTC.

Se ejecutaron consultas de lectura sobre una base de demostración existente. Se identificó una asignación ficticia ya almacenada y se reinició el proceso de la aplicación para comprobar su persistencia. Las tablas y los préstamos existentes conservaron sus valores durante la revisión.

## Base utilizada realmente en la comprobación

- Base de datos de la ejecución comprobada: LockersInterfazDemo_c8b34ec0.
- Instancia: (localdb)\MSSQLLocalDB.
- Motor devuelto por SQL Server: Express Edition (64-bit), versión 15.0.4382.1, IsLocalDB = 1.
- Autenticación integrada de Windows.
- Conexión utilizada durante esta revisión:

```text
Server=(localdb)\MSSQLLocalDB;Database=LockersInterfazDemo_c8b34ec0;Integrated Security=True;TrustServerCertificate=True;Application Name=LockersUNPHU.Evidencia64
```

La conexión utiliza autenticación integrada. El nombre Application Name permitió comprobar la sesión SQL de la aplicación en el catálogo del servidor.

**Diferencia de configuración encontrada.** appsettings.Development.json apunta por defecto a una base llamada LockersUnphu. Esa base no existía en esta instancia al comenzar la revisión. La beta estaba detenida. Para esta evidencia se arrancó expresamente con ConnectionStrings__Lockers apuntando a la base ficticia existente LockersInterfazDemo_c8b34ec0. Los resultados conexion-aplicacion-antes.json y conexion-aplicacion-despues.json confirman la conexión efectiva antes y después del reinicio.

Los nombres de las bases de demostración son propios de esta instalación. En otra computadora, configurar la conexión y aplicar la migración según el README del proyecto.

## Tablas reales

El catálogo devolvió 11 tablas, todas en dbo:

| Tabla | Responsabilidad |
| --- | --- |
| Estudiantes | Matrícula, nombre y vínculo con la cuenta de acceso |
| Lockers | Número, estado y versión para concurrencia |
| Asignaciones | Historial de préstamos con fechas inicial y final |
| AspNetUsers | Cuentas administradas por Identity |
| AspNetRoles | Roles del sistema |
| AspNetUserRoles | Relación entre cuentas y roles |
| AspNetRoleClaims | Claims de roles |
| AspNetUserClaims | Claims de usuarios |
| AspNetUserLogins | Estructura estándar de inicios externos de Identity |
| AspNetUserTokens | Estructura estándar de tokens de Identity |
| __EFMigrationsHistory | Historial de migraciones aplicadas |

Las tablas estándar de Identity forman parte de su esquema. Su existencia representa la estructura de persistencia y no prueba que todas sus funciones estén habilitadas.

## Migración aplicada

La tabla __EFMigrationsHistory contiene:

| MigrationId | ProductVersion |
| --- | --- |
| 20261116224457_Inicial | 10.0.15 |

Archivos reales del proyecto:

- src/LockersUnphu/Data/Migrations/20261116224457_Inicial.cs
- src/LockersUnphu/Data/Migrations/20261116224457_Inicial.Designer.cs
- src/LockersUnphu/Data/Migrations/LockersDbModelSnapshot.cs

El snapshot describe el modelo que EF Core compara con migraciones posteriores. La evidencia de aplicación en esta base procede de __EFMigrationsHistory.

## Asignación ficticia y persistencia

La consulta 03-asignaciones-ficticias.sql une Asignaciones, Estudiantes y Lockers. Aplica un filtro por las matrículas y nombres ficticios exactos del seeder.

| Dato | Valor observado |
| --- | --- |
| AsignacionId | 1 |
| Estudiante | Estudiante Demo Uno |
| Matrícula ficticia | DEMO-1001 |
| Locker | 1 |
| Estado actual del locker | Disponible |
| FechaHoraInicio | 2026-11-26T00:36:28.4671707Z |
| FechaHoraFin | 2026-11-26T01:21:28.4671707Z |
| Duración | 2700 segundos, 45 minutos |

La asignación está cerrada y el locker figura disponible, coherente con una devolución.

Se consultó el registro, se detuvo la aplicación, se inició nuevamente con la misma conexión y se repitió la consulta. Los archivos persistencia-antes.json y persistencia-despues.json tienen el mismo SHA-256. verificacion-persistencia.json conserva el resultado completo. La sesión SQL de la aplicación también se confirmó después del reinicio.

Para activar una consulta de la aplicación al servidor se utilizó un intento de acceso con un nombre ficticio inexistente y aleatorio. Esto permitió verificar la conexión sin modificar cuentas existentes. La asignación observada procede de los datos de demostración y permaneció intacta.

## Restricciones confirmadas

| Regla | Evidencia real y alcance |
| --- | --- |
| Matrícula única | Índice único IX_Estudiantes_Matricula sobre Matricula |
| Una asignación activa por estudiante | Índice único IX_Asignaciones_EstudianteId con filtro FechaHoraFin IS NULL |
| Una asignación activa por locker | Índice único IX_Asignaciones_LockerId con el mismo filtro |
| Número de locker único | Índice único IX_Lockers_Numero |
| Cuenta única por estudiante | Índice único IX_Estudiantes_ApplicationUserId |
| Relaciones válidas | Claves foráneas de asignación a estudiante y locker, y de estudiante a AspNetUsers |
| Fin coherente con inicio | CHECK CK_Asignaciones_Fechas |
| Estados y número permitidos | CHECK CK_Lockers_Estado y CK_Lockers_Numero |
| Concurrencia de lockers | Columna Version de 8 bytes, generada como rowversion; el catálogo devuelve su sinónimo timestamp |
| Rechazo de lockers ocupados o fuera de servicio | Prestamos.EntregarAsync verifica EstadoLocker.Disponible en el backend antes de guardar; el cambio se protege mediante rowversion |

**Alcance de la protección por estado.** El CHECK del locker admite los valores 0, 1 y 2. El rechazo de una entrega de locker ocupado o fuera de servicio se aplica en el servicio Prestamos, acompañado de la autorización del controlador y las restricciones de concurrencia. Una inserción SQL directa podría omitir esa regla de la aplicación.

Los índices y las restricciones se comprobaron mediante sys.indexes, sys.check_constraints y sys.foreign_keys. Las seis restricciones CHECK y claves foráneas devueltas para las tablas del dominio aparecen habilitadas y confiables. Las pruebas funcionales anteriores del módulo comprobaron los rechazos desde la aplicación. Esta revisión de evidencias mantuvo las consultas sobre datos existentes en modo lectura.

## Diferencias con el modelo del informe

El diagrama y el diccionario de las páginas 35–36 del documento de trabajo requieren estas correspondencias:

| Modelo documentado | Implementación real |
| --- | --- |
| UserId | ApplicationUserId, nvarchar(450), obligatorio y único |
| RowVersion | Version; rowversion de 8 bytes, reportado como timestamp en el catálogo |
| FechaInicio | FechaHoraInicio, datetimeoffset(7), obligatorio |
| FechaFin | FechaHoraFin, datetimeoffset(7), admite NULL |
| Fechas datetime2 del diagrama | datetimeoffset(7) en SQL Server |
| Matrícula nvarchar(50) del diagrama | Matricula nvarchar(24) |
| Nombre nvarchar(150) del diagrama | Nombre nvarchar(120) |
| Estado como enum del diagrama | int en SQL, con CHECK para 0, 1 y 2 |
| Cuenta descrita como opcional | La implementación exige una cuenta asociada a cada estudiante |

La fecha final nula identifica una asignación activa. Los datos observados corresponden a una asignación ya devuelta.

## Capturas recomendadas

Abrir visualizar.html para una presentación limpia generada a partir de los JSON guardados. Los paneles identifican su origen como resultados de consultas a SQL Server. También se puede usar SSMS con consultas/08-capturas-en-cuadricula.sql para capturar directamente las cuadrículas reales.

**Captura 1 — Base, tablas y migración.** Capturar el panel Evidencia 1 de visualizar.html. Como alternativa, seleccionar el bloque CAPTURA 1 del script 08 y ejecutarlo en una ventana conectada a LockersInterfazDemo_c8b34ec0. Usar una ventana amplia o captura de página completa y mostrar los tres resultados completos, con nombre de base, los 11 nombres de tablas y la migración. Ocultar conexiones ajenas y cualquier panel de propiedades de conexión. Pie sugerido: «Tablas y migración aplicadas en la base SQL Server LocalDB utilizada para la comprobación de la beta».

**Captura 2 — Asignación ficticia y persistencia.** Capturar el panel Evidencia 2. En SSMS, ejecutar el bloque CAPTURA 2 y ajustar las columnas para mostrar ambos instantes con su zona horaria. Incluir el Id, estudiante ficticio, matrícula, locker y estado. Para respaldar el reinicio, citar también los dos JSON y verificacion-persistencia.json. Pie sugerido: «Asignación ficticia identificada en SQL Server y recuperada sin cambios después del reinicio de la aplicación».

**Captura 3 opcional — Índices y validación de estado.** Capturar el panel Evidencia 3, o ejecutar el bloque CAPTURA 3 y mostrar los tres índices únicos con sus filtros. Acompañar la figura con la explicación del backend en esta sección. Pie sugerido: «Índices que impiden matrículas y asignaciones activas duplicadas; la disponibilidad del locker se valida en la aplicación».

Para las capturas, mostrar el título y la tabla completa a una escala legible. Los resultados guardados contienen metadatos y una cuenta ficticia. La consulta evita seleccionar hashes, tokens, direcciones de correo o contenido de las tablas de usuarios.

## Reproducir las consultas

Ejemplo con SQLCMD desde la raíz, usando autenticación integrada y una base ya existente:

```powershell
sqlcmd -S '(localdb)\MSSQLLocalDB' -d LockersInterfazDemo_c8b34ec0 -E -b -y 0 -w 65535 -f 65001 -i docs/evidencias-base-datos/consultas/03-asignaciones-ficticias.sql
```

Las consultas 01–07 producen JSON. El script 08 produce cuadrículas. Las consultas de metadatos usan la base a la que se conecte el cliente.

La conexión de la demostración puede proporcionarse temporalmente a la beta:

```powershell
$env:ConnectionStrings__Lockers = 'Server=(localdb)\MSSQLLocalDB;Database=LockersInterfazDemo_c8b34ec0;Integrated Security=True;TrustServerCertificate=True'
dotnet run --project src/LockersUnphu
```

Estas instrucciones presuponen que existe esa base en el equipo. Los nombres y resultados se deben volver a consultar en cada instalación que se documente.
