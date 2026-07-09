#!/bin/bash
# Shell script to trigger database restore inside MS SQL Server container

CONTAINER_NAME=${1:-gpa-db-prod}
DB_PASSWORD=${2:-YourSecurePassword123!}

echo "========================================="
echo "Starting Database Restore: AcademicGPA"
echo "Target Container: $CONTAINER_NAME"
echo "========================================="

# Execute restore sqlcmd command
docker exec -i "$CONTAINER_NAME" /opt/mssql-tools18/bin/sqlcmd \
  -S localhost \
  -U sa \
  -P "$DB_PASSWORD" \
  -Q "ALTER DATABASE [AcademicGPA] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; RESTORE DATABASE [AcademicGPA] FROM DISK = '/var/opt/mssql/backup/AcademicGPA.bak' WITH REPLACE; ALTER DATABASE [AcademicGPA] SET MULTI_USER;" \
  -C

if [ $? -eq 0 ]; then
  echo ">>> Success: Database restore completed."
else
  echo ">>> Error: Database restore failed."
  exit 1
fi
