SET NOCOUNT ON;
SELECT
    t.name AS Tabla,
    i.name AS Indice,
    i.is_unique AS EsUnico,
    i.has_filter AS TieneFiltro,
    i.filter_definition AS Filtro,
    STRING_AGG(CONVERT(nvarchar(max), c.name), ', ')
        WITHIN GROUP (ORDER BY ic.key_ordinal) AS Columnas
FROM sys.indexes AS i
JOIN sys.tables AS t ON t.object_id = i.object_id
JOIN sys.index_columns AS ic ON ic.object_id = i.object_id AND ic.index_id = i.index_id
JOIN sys.columns AS c ON c.object_id = ic.object_id AND c.column_id = ic.column_id
WHERE t.name IN ('Estudiantes', 'Lockers', 'Asignaciones')
    AND ic.key_ordinal > 0
GROUP BY t.name, i.name, i.is_unique, i.has_filter, i.filter_definition
ORDER BY t.name, i.name
FOR JSON PATH, INCLUDE_NULL_VALUES;
