-- Metadatos del catálogo, sin consultar contenido de las cuentas Identity.
SET NOCOUNT ON;
SELECT s.name AS Esquema, t.name AS Tabla
FROM sys.tables AS t
JOIN sys.schemas AS s ON s.schema_id = t.schema_id
ORDER BY s.name, t.name
FOR JSON PATH;
