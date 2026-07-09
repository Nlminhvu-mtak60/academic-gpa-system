BACKUP DATABASE [AcademicGPA]
TO DISK = N'/var/opt/mssql/backup/AcademicGPA.bak'
WITH FORMAT, INIT, NAME = N'AcademicGPA-Full Database Backup', SKIP, NOREWIND, NOUNLOAD, STATS = 10;
GO
