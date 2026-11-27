-- Abrir esta consulta en SSMS o Azure Data Studio conectado a la base de la evidencia.
-- Ejecutar cada bloque por separado con resultados en cuadrícula.
-- Todas las operaciones son de solo lectura.

-- CAPTURA 1. Base, tablas y migración aplicada.
SELECT DB_NAME() AS BaseDatos,
    CONVERT(nvarchar(128), SERVERPROPERTY('Edition')) AS Edicion,
    CONVERT(nvarchar(128), SERVERPROPERTY('ProductVersion')) AS VersionSQL,
    CONVERT(int, SERVERPROPERTY('IsLocalDB')) AS EsLocalDB;

SELECT s.name AS Esquema, t.name AS Tabla
FROM sys.tables AS t
JOIN sys.schemas AS s ON s.schema_id = t.schema_id
ORDER BY s.name, t.name;

SELECT MigrationId, ProductVersion
FROM dbo.__EFMigrationsHistory
ORDER BY MigrationId;

-- CAPTURA 2. Asignación ficticia persistida.
SELECT a.Id AS AsignacionId, e.Nombre AS EstudianteFicticio,
    e.Matricula AS MatriculaFicticia, l.Numero AS Locker,
    CASE l.Estado WHEN 0 THEN N'Disponible' WHEN 1 THEN N'Ocupado'
        WHEN 2 THEN N'Fuera de servicio' END AS EstadoActualLocker,
    a.FechaHoraInicio, a.FechaHoraFin
FROM dbo.Asignaciones AS a
JOIN dbo.Estudiantes AS e ON e.Id = a.EstudianteId
JOIN dbo.Lockers AS l ON l.Id = a.LockerId
WHERE
    (e.Matricula = 'DEMO-1001' AND e.Nombre = N'Estudiante Demo Uno')
    OR (e.Matricula = 'DEMO-1002' AND e.Nombre = N'Estudiante Demo Dos')
    OR (e.Matricula = 'DEMO-1003' AND e.Nombre = N'Estudiante Demo Tres')
ORDER BY a.Id;

-- CAPTURA 3 OPCIONAL. Índices únicos que aplican las reglas.
SELECT t.name AS Tabla, i.name AS Indice,
    i.is_unique AS EsUnico, i.filter_definition AS Filtro,
    c.name AS Columna
FROM sys.indexes AS i
JOIN sys.tables AS t ON t.object_id = i.object_id
JOIN sys.index_columns AS ic ON ic.object_id = i.object_id AND ic.index_id = i.index_id
JOIN sys.columns AS c ON c.object_id = ic.object_id AND c.column_id = ic.column_id
WHERE (t.name = 'Estudiantes' AND i.name = 'IX_Estudiantes_Matricula')
    OR (t.name = 'Asignaciones' AND i.name IN (
        'IX_Asignaciones_EstudianteId', 'IX_Asignaciones_LockerId'))
ORDER BY t.name, i.name, ic.key_ordinal;
