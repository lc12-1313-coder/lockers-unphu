-- Ejecutar conectado a la base de la demostración. Consulta de solo lectura.
SET NOCOUNT ON;
SELECT
    DB_NAME() AS BaseDatos,
    CONVERT(nvarchar(128), SERVERPROPERTY('Edition')) AS Edicion,
    CONVERT(nvarchar(128), SERVERPROPERTY('ProductVersion')) AS VersionSQL,
    CONVERT(int, SERVERPROPERTY('IsLocalDB')) AS EsLocalDB,
    CONVERT(nvarchar(40), SYSUTCDATETIME(), 127) + 'Z' AS CapturadoUTC
FOR JSON PATH;
