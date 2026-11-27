SET NOCOUNT ON;
SELECT MigrationId, ProductVersion
FROM dbo.__EFMigrationsHistory
ORDER BY MigrationId
FOR JSON PATH;
