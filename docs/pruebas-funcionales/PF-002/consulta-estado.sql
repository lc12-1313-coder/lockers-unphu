SELECT DB_NAME() BaseDatos, @@VERSION VersionSql, SYSDATETIMEOFFSET() ConsultadoEn,
 (SELECT l.Id,l.Numero,l.Estado,
  (SELECT COUNT(*) FROM Asignaciones a WHERE a.LockerId=l.Id AND a.FechaHoraFin IS NULL) AsignacionesActivas
  FROM Lockers l WHERE l.Numero=2 FOR JSON PATH) Locker,
 (SELECT e.Id,e.Matricula,e.Nombre,
  (SELECT COUNT(*) FROM Asignaciones a WHERE a.EstudianteId=e.Id AND a.FechaHoraFin IS NULL) AsignacionesActivas
  FROM Estudiantes e WHERE e.Matricula IN ('DEMO-1002','DEMO-1003') ORDER BY e.Matricula FOR JSON PATH) Estudiantes,
 (SELECT a.Id,a.EstudianteId,e.Matricula,a.LockerId,l.Numero,a.FechaHoraInicio,a.FechaHoraFin
  FROM Asignaciones a JOIN Estudiantes e ON e.Id=a.EstudianteId JOIN Lockers l ON l.Id=a.LockerId
  WHERE l.Numero=2 ORDER BY a.Id FOR JSON PATH, INCLUDE_NULL_VALUES) Asignaciones
FOR JSON PATH, WITHOUT_ARRAY_WRAPPER;
