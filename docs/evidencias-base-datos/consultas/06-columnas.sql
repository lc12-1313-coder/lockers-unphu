SET NOCOUNT ON;
SELECT
    t.name AS Tabla,
    c.column_id AS Posicion,
    c.name AS Columna,
    ty.name AS TipoSQL,
    CASE WHEN ty.name IN ('nvarchar', 'nchar') AND c.max_length > 0
        THEN c.max_length / 2 ELSE c.max_length END AS Longitud,
    c.precision AS Precision,
    c.scale AS Escala,
    c.is_nullable AS AceptaNulos,
    c.is_identity AS EsIdentidad
FROM sys.tables AS t
JOIN sys.columns AS c ON c.object_id = t.object_id
JOIN sys.types AS ty ON ty.user_type_id = c.user_type_id
WHERE t.name IN ('Estudiantes', 'Lockers', 'Asignaciones')
ORDER BY t.name, c.column_id
FOR JSON PATH;
