# Lockers UNPHU

Proyecto académico de gestión de lockers para la Biblioteca UNPHU, Santo Domingo.

## Estado actual

Esta incorporación agrega SQL Server, las entidades y su migración, ASP.NET Core Identity, los tres roles y la inicialización reproducible de cuentas y datos ficticios. Se puede iniciar sesión, consultar el rol propio, cambiar contraseña y cerrar sesión desde una interfaz básica.

Los módulos operativos de estudiantes, inventario, préstamos y reportes se incorporarán en las siguientes ramas. El equipo parte de una beta local previa. Este repositorio organiza su incorporación y revisión por módulos, junto con las mejoras y verificaciones pendientes. Cada pull request identifica la procedencia y las comprobaciones realizadas.

## Requisitos

- Git y SDK .NET 10.0.404 o un parche posterior de la banda 10.0.4xx.
- SQL Server accesible. En Windows puede usarse LocalDB.
- PowerShell 7 para los ejemplos de configuración.

La solución usa ASP.NET Core MVC, vistas Razor y Bootstrap. Identity, EF Core SQL Server y la herramienta local dotnet-ef están fijados en 10.0.15.

## Preparar la aplicación

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

Sin esa variable la prueba SQL se omite. Este bloque cubre dos pruebas automatizadas, una HTTP y una integral con múltiples escenarios. Los resultados de los módulos posteriores se registrarán cuando se incorporen. El registro de esta incorporación está en [verificación de base de datos](docs/verificacion-base-datos.md).

## Organización y contribuciones

- `src/LockersUnphu` contiene MVC, el modelo, los servicios y las migraciones.
- `tests/LockersUnphu.Tests` contiene las pruebas.
- [Plan de trabajo](docs/plan-trabajo.md) describe responsables y revisión.
- [ADR 0001](docs/adr/0001-arquitectura.md) registra las decisiones de la beta.

Crear una rama por tarea desde main y registrar cambios comprobables en commits descriptivos. Cada PR explica su procedencia, alcance, verificación y limitaciones. Otro integrante revisa el cambio antes de integrarlo. Cada integrante revisa el trabajo registrado con su identidad y usa un correo asociado a su cuenta.
