# Lockers UNPHU

Proyecto académico de gestión de lockers para la Biblioteca UNPHU, Santo Domingo.

## Estado de esta rama

La rama `chore/base-proyecto` incorpora la estructura ejecutable ASP.NET Core MVC, una pantalla inicial y las convenciones del repositorio. El siguiente bloque de trabajo incorporará la base de datos, Identity y sus permisos.

El equipo parte de una beta local previa que ya contiene los módulos funcionales. Este repositorio organiza su incorporación y revisión por módulos, junto con las mejoras y verificaciones pendientes. Cada pull request debe identificar qué parte procede de esa beta y qué cambios adicionales introduce.

## Requisitos y ejecución

Instalar Git y el SDK .NET 10.0.404 o un parche posterior de la banda 10.0.4xx. `global.json` fija la versión utilizada. La interfaz base utiliza Razor, Bootstrap y CSS.

Desde la raíz:

```powershell
dotnet restore
dotnet build --no-restore
dotnet run --project src/LockersUnphu
```

Abrir [la aplicación local](http://127.0.0.1:5170). Esta incorporación inicial se puede comprobar sin configurar una base de datos.

## Organización

- `src/LockersUnphu` contiene la aplicación MVC.
- `docs/plan-trabajo.md` describe responsabilidades, revisiones y criterios de integración.
- `docs/adr/0001-arquitectura.md` registra las decisiones de arquitectura de la beta que se incorporará por módulos.

## Contribuciones

Crear una rama por tarea a partir de `main` y registrar cambios verificables en commits descriptivos. Abrir un pull request con el alcance y las comprobaciones realizadas. Otro integrante revisa el cambio antes de integrarlo.

La cuenta que autoriza una subida y la identidad del autor del commit se configuran por separado. Cada integrante debe revisar el trabajo registrado con su identidad y utilizar un correo asociado a su cuenta de GitHub.
