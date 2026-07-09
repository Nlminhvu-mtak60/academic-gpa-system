#!/bin/bash
# Shell script to trigger database backup inside MS SQL Server container

CONTAINER_NAME=${1:-gpa-db-prod}
DB_PASSWORD=${2:-YourSecurePassword123!}

echo "========================================="
echo "Starting Full Backup: AcademicGPA"
echo "Target Container: $CONTAINER_NAME"
echo "========================================="

# Ensure backup directory exists
docker exec -i "$CONTAINER_NAME" mkdir -p /var/opt/mssql/backup

# Execute backup sqlcmd command
docker exec -i "$CONTAINER_NAME" /opt/mssql-tools18/bin/sqlcmd \
  -S localhost \
  -U sa \
  -P "$DB_PASSWORD" \
  -Q "BACKUP DATABASE [AcademicGPA] TO DISK = '/var/opt/mssql/backup/AcademicGPA.bak' WITH FORMAT, INIT, SKIP, NOREWIND, NOUNLOAD, STATS = 10;" \
  -C

if [ $? -eq 0 ]; then
  echo ">>> Success: Database backup completed."
else
  echo ">>> Error: Database backup failed."
  exit 1
fi
