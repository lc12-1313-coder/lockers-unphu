# Lockers UNPHU

Proyecto académico de gestión de lockers para la Biblioteca UNPHU, Santo Domingo.

## Estado actual

La beta usa SQL Server, una migración de Entity Framework Core, ASP.NET Core Identity y tres roles. Permite iniciar sesión, consultar el rol propio, cambiar contraseña y cerrar sesión.

Están disponibles el registro y la búsqueda de estudiantes, la consulta de lockers, el préstamo propio y el mantenimiento administrativo del inventario. El personal asigna y devuelve lockers desde Mostrador. Personal y administrador consultan Reportes. El administrador puede crear cuentas del equipo y cambiar los roles de otras cuentas desde Equipo; los cambios de rol invalidan las sesiones anteriores. El equipo parte de una beta local previa. Este repositorio registra su incorporación y revisión por módulos, junto con las mejoras y verificaciones pendientes. Cada pull request identifica la procedencia y las comprobaciones realizadas.

## Requisitos

- Git y SDK .NET 10.0.404 o un parche posterior de la banda 10.0.4xx.
- SQL Server accesible. En Windows puede usarse LocalDB.
- PowerShell 7 para los ejemplos de configuración.

La solución usa ASP.NET Core MVC, vistas Razor y Bootstrap. Identity, EF Core SQL Server y la herramienta local dotnet-ef están fijados en 10.0.15.

## Preparar la aplicación

Clonar el repositorio público y entrar en la carpeta:

```powershell
git clone https://github.com/lc12-1313-coder/lockers-unphu.git
cd lockers-unphu
dotnet --version
```

Comprobar que el SDK corresponde a 10.0.404 o a un parche posterior de la banda 10.0.4xx. Tener solo .NET 9 o el runtime sin SDK no es suficiente. SQL Server/LocalDB debe instalarse por separado; la aplicación no instala el motor. La verificación local se realizó con SQL Server 2019 Express LocalDB 15.0.4382.1; no se ha validado una versión mínima diferente.

Desde la raíz:

```powershell
dotnet restore
dotnet tool restore
dotnet build --no-restore
```

El entorno Development configura LocalDB y la base `LockersUnphu` con autenticación integrada. Puede comprobarse la instancia con `SqlLocalDB info` y arrancarla con `SqlLocalDB start MSSQLLocalDB`.

Para otra instancia, configurar la conexión en la misma terminal. Este ejemplo corresponde a desarrollo local:

```powershell
$env:ConnectionStrings__Lockers = 'Server=localhost;Database=LockersUnphu;Integrated Security=True;TrustServerCertificate=True'
```

Las conexiones sensibles se proporcionan por variables de entorno o User Secrets. Para desarrollo, el comando `dotnet user-secrets set 'ConnectionStrings:Lockers' <cadena> --project src/LockersUnphu` guarda la conexión fuera del repositorio.

Crear la estructura:

```powershell
dotnet ef database update --project src/LockersUnphu -- --environment Development
```

Preparar roles y datos ficticios:

```powershell
$env:Inicializacion__Administrador = 'admin.demo'
$env:Inicializacion__ClaveAdministrador = Read-Host 'Contraseña inicial del administrador' -MaskInput
$env:Inicializacion__Demo = 'true'
$env:Inicializacion__ClaveDemo = Read-Host 'Contraseña para cuentas ficticias' -MaskInput
dotnet run --project src/LockersUnphu -- --inicializar
```

Usar claves distintas para administrador y demostración, con al menos 12 caracteres, mayúscula, minúscula y número. El comando prepara los roles Estudiante, PersonalBiblioteca y Administrador. Con la opción Demo en Development agrega `biblioteca.demo`, tres estudiantes con matrícula `DEMO-1001` a `DEMO-1003`, 12 lockers y una asignación histórica cerrada. El locker 08 está fuera de servicio.

Ejecutar la inicialización de forma secuencial antes de repartir cuentas. Al repetirla, las cuentas existentes conservan sus contraseñas.

```powershell
Remove-Item Env:Inicializacion__ClaveAdministrador, Env:Inicializacion__ClaveDemo
dotnet run --project src/LockersUnphu
```

Abrir [el acceso local](http://127.0.0.1:5170/cuenta/ingresar). El administrador usa su contraseña inicial. El personal `biblioteca.demo` y los estudiantes usan la contraseña elegida para demostración.

El usuario autenticado puede cambiar su propia contraseña desde Mi cuenta. Si el proceso necesita un directorio explícito de claves de Data Protection, configurar `DataProtection__KeysDirectory` con una ruta persistente, protegida y externa al repositorio.

### Ejecutar desde CMD

Los ejemplos anteriores de variables `$env:` requieren PowerShell. Si la base ya fue migrada e inicializada y se utiliza CMD, la sintaxis es:

```cmd
set "ConnectionStrings__Lockers=Server=(localdb)\MSSQLLocalDB;Database=LockersUnphu;Integrated Security=True;TrustServerCertificate=True"
dotnet run --project src/LockersUnphu
```

El SDK correcto debe estar en PATH. Si se instaló en otra carpeta, ejecutar su `dotnet.exe` mediante la ruta completa entre comillas. Para usar una base de demostración existente, configurar su nombre real; los nombres usados en las evidencias no se crean al clonar. Mantener la terminal abierta y esperar `Now listening on: http://127.0.0.1:5170` antes de abrir el navegador. Ctrl+C detiene la aplicación. Evitar dos instancias en el mismo puerto.

## Modelo y restricciones

- `Estudiantes` guarda la matrícula única, el nombre y una referencia única a la cuenta Identity.
- `Lockers` guarda el número único, el estado y una columna rowversion para concurrencia.
- `Asignaciones` relaciona estudiante y locker con fechas de inicio y devolución.
- Los índices únicos filtrados impiden dos asignaciones activas para un estudiante o un locker.
- Identity administra cuentas, hashes de contraseña, roles y relaciones entre usuarios y roles.

## Verificación

```powershell
dotnet test
dotnet ef migrations has-pending-model-changes --project src/LockersUnphu -- --environment Development
```

La prueba integral crea una base temporal con nombre aleatorio y la elimina al finalizar. Requiere SQL Server y permiso para crear esa base:

```powershell
$env:LOCKERS_TEST_SQL = 'Server=(localdb)\MSSQLLocalDB;Integrated Security=True;TrustServerCertificate=True'
dotnet test --logger 'trx;LogFileName=base-datos.trx'
```

Sin esa variable se omiten las cuatro pruebas SQL. El repositorio contiene cinco pruebas automatizadas, una HTTP y cuatro integrales con múltiples escenarios. La comprobación del viernes 27 terminó con cinco aprobadas, cero fallos y cero omisiones. Los resultados anteriores de cuatro pruebas corresponden a la revisión previa a incorporar Equipo. Las comprobaciones se registran en [base de datos](docs/verificacion-base-datos.md), [estudiantes e inventario](docs/verificacion-estudiantes-inventario.md), [préstamos y reportes](docs/verificacion-prestamos-reportes.md) y [verificación del viernes 27](docs/verificacion-viernes27.md).

## Administración del equipo

Solo Administrador accede a Equipo. Puede crear cuentas de Personal de biblioteca o Administrador y cambiar el rol de otras cuentas del equipo. Su propio rol debe cambiarlo otro administrador; esta pantalla no transforma estudiantes en personal. Después de un cambio de permisos, la cuenta afectada debe volver a iniciar sesión. Las cuentas y roles se guardan en las tablas existentes de Identity, sin otra migración.

## Alcance de la validación

La compilación, las pruebas SQL y la revisión visual se han ejecutado en la computadora de desarrollo. La instalación y ejecución en otra computadora y la evaluación formal de utilidad con usuarios continúan pendientes. No se ha creado el tag `v0.1.0-beta` en este repositorio; su publicación requiere una decisión explícita del equipo. Las evidencias PF-002 están en [la ficha funcional](docs/pruebas-funcionales/PF-002/README.md).

## Demostración del flujo principal

1. Ingresar como biblioteca.demo y abrir Mostrador.
2. Buscar DEMO-1002 o registrar un estudiante nuevo.
3. Seleccionar un locker disponible y registrar la entrega de la llave.
4. Comprobar la asignación activa y el estado ocupado del locker.
5. Ingresar como el estudiante para consultar su propio préstamo.
6. Volver al personal y registrar la devolución.
7. Comprobar que el locker está disponible y consultar las fechas y duración en Reportes.
8. Reiniciar la aplicación con la misma conexión y comprobar el historial.

## Organización y contribuciones

- `src/LockersUnphu` contiene MVC, el modelo, los servicios y las migraciones.
- `tests/LockersUnphu.Tests` contiene las pruebas.
- [Plan de trabajo](docs/plan-trabajo.md) describe responsables y revisión.
- [ADR 0001](docs/adr/0001-arquitectura.md) registra las decisiones de la beta.

Crear una rama por tarea desde main y registrar cambios comprobables en commits descriptivos. Cada PR explica su procedencia, alcance, verificación y limitaciones. Otro integrante revisa el cambio antes de integrarlo. Cada integrante revisa el trabajo registrado con su identidad y usa un correo asociado a su cuenta.


## Evidencias de la base de datos

La revisión de solo lectura, las consultas, los resultados SQL y las instrucciones de captura para la sección 6.4 están en [evidencias de base de datos](docs/evidencias-base-datos/README.md). El informe distingue la conexión predeterminada de la base de demostración utilizada en la comprobación.


Las capturas reales por rol y sus pies de figura para la sección 6.5 están en [evidencias de interfaces](docs/evidencias-interfaces/README.md).
