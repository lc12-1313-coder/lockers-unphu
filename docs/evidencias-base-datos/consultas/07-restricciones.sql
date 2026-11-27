SET NOCOUNT ON;
SELECT N'CHECK' AS Tipo, t.name AS Tabla, cc.name AS Restriccion,
    cc.definition AS Definicion, cc.is_disabled AS Deshabilitada, cc.is_not_trusted AS NoConfiable
FROM sys.check_constraints AS cc
JOIN sys.tables AS t ON t.object_id = cc.parent_object_id
WHERE t.name IN ('Estudiantes', 'Lockers', 'Asignaciones')
UNION ALL
SELECT N'FOREIGN KEY', t.name, fk.name,
    CONCAT(c.name, N' -> ', rt.name, N'.', rc.name),
    fk.is_disabled, fk.is_not_trusted
FROM sys.foreign_keys AS fk
JOIN sys.foreign_key_columns AS fkc ON fkc.constraint_object_id = fk.object_id
JOIN sys.tables AS t ON t.object_id = fkc.parent_object_id
JOIN sys.columns AS c ON c.object_id = t.object_id AND c.column_id = fkc.parent_column_id
JOIN sys.tables AS rt ON rt.object_id = fkc.referenced_object_id
JOIN sys.columns AS rc ON rc.object_id = rt.object_id AND rc.column_id = fkc.referenced_column_id
WHERE t.name IN ('Estudiantes', 'Lockers', 'Asignaciones')
ORDER BY Tabla, Tipo, Restriccion
FOR JSON PATH;
