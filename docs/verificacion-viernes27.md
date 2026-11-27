# Verificación del viernes 27 de noviembre de 2026

## Procedencia y código comprobado

La rama feat/administracion-equipo parte de main después del PR 7, que integra PF-002. Incorpora EquipoController y sus vistas desde la beta local anterior, adapta sus modelos al repositorio actual y agrega el acceso desde la navegación administrativa. El servicio de creación y cambio de roles ya existía; no se modificó su lógica ni la migración.

El código funcional se registra en 77c921f y la prueba integral adicional en 10bfb67. La documentación posterior no modifica src ni tests. Los commits se registran con las identidades autorizadas por el equipo, y las revisiones automatizadas se declaran expresamente en el PR.

## Resultado automatizado

SDK .NET 10.0.404, EF Core e Identity 10.0.15, SQL Server LocalDB. La compilación del módulo terminó con cero errores y cero advertencias. Se configuró LOCKERS_TEST_SQL para ejecutar todas las pruebas SQL, y se ejecutó:

```powershell
dotnet test --logger 'trx;LogFileName=viernes-equipo.trx'
```

Resultado real: **Failed: 0, Passed: 5, Skipped: 0, Total: 5**. El resultado TRX queda en tests/LockersUnphu.Tests/TestResults/viernes-equipo.trx, excluido de Git.

La nueva prueba comprobó por HTTP y SQL:

- Personal y estudiante no pueden abrir ni enviar las acciones de Equipo.
- El listado no incluye estudiantes ni la contraseña de prueba.
- Se rechaza el envío sin token antifalsificación y la creación con rol Estudiante.
- El administrador crea una cuenta de Personal; un duplicado no crea otra cuenta.
- La cuenta se puede promover a Administrador y volver a Personal.
- Cada cambio de rol invalida la sesión anterior y exige ingresar de nuevo.
- El administrador no puede cambiar su propio rol desde esta acción.
- No se puede transformar una cuenta Estudiante mediante la acción de roles del equipo.
- Los roles finales se verifican desde un nuevo ámbito de base de datos.

Las cuatro pruebas anteriores también pasaron en esta ejecución; cubren migraciones y seed, autenticación, estudiantes, inventario, préstamos, devolución, reportes, concurrencia y persistencia tras reinicio. Cada prueba SQL crea y elimina su propia base temporal.

## Revisión de interfaz y arranque

Se inició una instancia separada de la aplicación en http://127.0.0.1:5177, con la base ficticia LockersViernes_41fc5cc5, creada mediante la migración y el seeder existentes. No se interrumpió la instancia que el usuario mantenía abierta en el puerto 5170.

Se inició sesión como admin.demo y se verificaron el enlace Equipo, el listado de cuentas y el formulario de alta. Se revisaron escritorio y móvil de 390 por 844 px. El formulario cabe en pantalla; el listado conserva su desplazamiento horizontal en el contenedor de tabla. No se usaron contraseñas fijas en el código ni se publicaron las claves de la sesión de prueba.

Las capturas de esta comprobación se guardaron fuera del repositorio, en outputs/verificacion-viernes27, con los nombres equipo-escritorio.jpg, equipo-nuevo-escritorio.jpg, equipo-movil.jpg y equipo-nuevo-movil.jpg. Son material local de revisión y no están incluidas en este commit.

## Estado y pendientes

La instalación en una segunda computadora se pospuso por indicación del usuario y sigue pendiente. También permanecen pendientes la evaluación formal de utilidad con usuarios y la publicación de una etiqueta de entrega. No se ha creado v0.1.0-beta en el repositorio actual.

Los informes previos de cuatro pruebas y las capturas donde Equipo todavía no existía describen sus revisiones históricas. La versión actual añade una quinta prueba y la pantalla administrativa. La carpeta de trabajo aislada permite comprobar la integración sin reemplazar los binarios de la instancia abierta por el usuario; esa instancia requiere reiniciarse y compilar la nueva revisión para mostrar Equipo.
