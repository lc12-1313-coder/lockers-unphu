# Verificación de base de datos e Identity

## Procedencia y alcance

La rama feat/base-datos incorpora el modelo, la migración y los servicios de cuentas e inicialización de la beta local anterior. Adapta el inicio de la aplicación y agrega una interfaz básica de acceso y pruebas específicas para este bloque.

La interfaz definitiva y los flujos de registro administrativo, inventario, préstamos y reportes corresponden a las siguientes incorporaciones.

## Comprobaciones realizadas

- Restauración de paquetes y herramientas locales.
- Compilación con cero errores y cero advertencias.
- Comprobación de EF Core sin diferencias entre modelo y migración.
- Dos pruebas automatizadas aprobadas contra SQL Server LocalDB, sin fallos ni omisiones.

La prueba HTTP verifica la redirección al acceso y el rechazo de formularios sin token antifalsificación.

La prueba integral aplica la migración a una base aislada, ejecuta dos veces el seeder y comprueba que hay tres roles, cinco cuentas, tres estudiantes, doce lockers y una asignación histórica. Verifica hashes de contraseña, pertenencia a roles, acceso HTTP con las tres cuentas representativas, cambio de contraseña, cierre de sesión y nuevo acceso con la contraseña actualizada. También comprueba matrícula duplicada, restricciones de asignaciones activas y lectura desde otra conexión.

Los casos que insertan asignaciones directamente prueban los índices de la base. Las transiciones operativas de estado se verificarán al incorporar el servicio de préstamos y sus endpoints.

## Reproducción y límites

Ejecutar los comandos del README con LOCKERS_TEST_SQL configurada. El informe TRX queda en el directorio TestResults, excluido de Git. La prueba SQL crea y elimina únicamente una base con prefijo LockersBasePruebas y nombre aleatorio.

La ejecución en una segunda computadora y la evaluación de utilidad con usuarios siguen pendientes.
