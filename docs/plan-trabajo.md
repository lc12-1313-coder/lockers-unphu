# Plan de trabajo para la revisión de la beta

Plan acordado el 25 de noviembre de 2026. La revisión de las funcionalidades principales está prevista para el viernes 27 antes del mediodía.

## Punto de partida

Existe una beta local anterior en el commit `578f9f03ca06f5a2278223287c52fa648d75a76d`, con etiqueta local `v0.1.0-beta`. En esa versión se ejecutaron 14 pruebas satisfactorias contra SQL Server. Esos resultados corresponden a la beta anterior. Cada módulo incorporado a este repositorio requiere sus propias comprobaciones después de integrarse.

Este repositorio se creó de nuevo el 25 de noviembre. Los cambios actuales registran la incorporación, revisión y mejora de los módulos existentes con sus fechas reales.

## Responsables y alcance

| Integrante | Responsabilidad | Rama propuesta |
| --- | --- | --- |
| Luis Carela | Base del proyecto, configuración, modelo, migraciones, autenticación y coordinación de integraciones | `chore/base-proyecto`, `feat/base-datos` |
| Leslie Gómez | Acceso y navegación, registro de estudiantes, inventario y revisión responsive | `feat/interfaz` |
| César Espinosa | Préstamos, devoluciones, reportes y pruebas de reglas de negocio | `feat/prestamos`, `test/flujo-completo` |

## Secuencia

| Fecha | Trabajo y resultado esperado |
| --- | --- |
| Miércoles 25 | Preparar y revisar la estructura ejecutable. Revisar requisitos de interfaz y préstamos. Avanzar con el modelo de datos y las migraciones después de revisar la base. |
| Jueves 26 | Incorporar autenticación, permisos, interfaz y operaciones de préstamos mediante pull requests. Comprobar integración y reportes. |
| Viernes 27, mañana | Ejecutar el flujo completo, revisar pantallas, comprobar instalación en otra computadora y corregir incidencias. Fijar la revisión exacta para la demostración antes del mediodía. |

La evaluación formal de utilidad con usuarios sigue pendiente para el cierre académico. El equipo debe registrar los resultados reales cuando se realice.

## Revisión e integración

1. El responsable trabaja en una rama por tarea y registra commits al completar cambios comprobables.
2. El pull request describe procedencia, cambios, pruebas ejecutadas y limitaciones.
3. Otro integrante revisa el código y comprueba el resultado desde su cuenta.
4. Los defectos se corrigen en la misma rama.
5. El equipo integra el pull request aprobado en `main`.

## Criterios de la demostración

- Ingresar con los tres roles y comprobar sus permisos.
- Registrar y buscar un estudiante por matrícula.
- Consultar lockers y entregar una llave disponible.
- Rechazar préstamos duplicados y lockers ocupados o fuera de servicio.
- Registrar devolución y comprobar que el locker se libera.
- Conservar datos después de reiniciar la aplicación.
- Consultar el préstamo propio del estudiante y los reportes básicos.
