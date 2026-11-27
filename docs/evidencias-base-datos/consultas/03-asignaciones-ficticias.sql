-- Filtro estricto de las cuentas ficticias sembradas por el proyecto.
-- EstadoLocker corresponde al estado actual. La asignación mostrada puede estar cerrada.
SET NOCOUNT ON;
SELECT
    a.Id AS AsignacionId,
    e.Nombre AS EstudianteFicticio,
    e.Matricula AS MatriculaFicticia,
    l.Numero AS NumeroLocker,
    CASE l.Estado
        WHEN 0 THEN N'Disponible'
        WHEN 1 THEN N'Ocupado'
        WHEN 2 THEN N'Fuera de servicio'
    END AS EstadoLocker,
    a.FechaHoraInicio,
    a.FechaHoraFin,
    DATEDIFF(SECOND, a.FechaHoraInicio, a.FechaHoraFin) AS DuracionSegundos
FROM dbo.Asignaciones AS a
JOIN dbo.Estudiantes AS e ON e.Id = a.EstudianteId
JOIN dbo.Lockers AS l ON l.Id = a.LockerId
WHERE
    (e.Matricula = 'DEMO-1001' AND e.Nombre = N'Estudiante Demo Uno')
    OR (e.Matricula = 'DEMO-1002' AND e.Nombre = N'Estudiante Demo Dos')
    OR (e.Matricula = 'DEMO-1003' AND e.Nombre = N'Estudiante Demo Tres')
ORDER BY a.Id
FOR JSON PATH, INCLUDE_NULL_VALUES;
