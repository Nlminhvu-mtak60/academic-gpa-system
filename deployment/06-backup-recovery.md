# 06 - Backup & Recovery Strategy (Phase 11)

This guide documents the database backup, restore, scheduling, snapshot retention, and verification scripts for the Academic GPA Management System.

---

## 1. Backup Configuration & Operations

The system utilizes standard MS SQL Server backup mechanisms (`.bak` files). Backups are created inside the database container and stored at `/var/opt/mssql/backup/`.

### Manual Backup Operation
Execute the automated wrapper script on the host to create a new backup:
```bash
./deployment/scripts/backup.sh gpa-db-prod YourSecurePassword123!
```

---

## 2. Recovery & Restore Operations

### Manual Restore Operation
Ensure the target backup file `AcademicGPA.bak` is located at `/var/opt/mssql/backup/` inside the target database container. Then run:
```bash
./deployment/scripts/restore.sh gpa-db-prod YourSecurePassword123!
```

---

## 3. Production Backup Schedule

| Backup Type | Frequency | Execution Window | Retention Period | Target Directory |
|---|---|---|---|---|
| **Full Backup** | Daily | 02:00 AM (UTC) | 14 Days | `/var/opt/mssql/backup/` |
| **Transaction Logs** | Hourly | Every 60 Mins | 3 Days | `/var/opt/mssql/backup/logs/` |

### Setting Up Daily Cron Job (Linux Host)
Add the backup execution commands to the host server's cron tab to schedule automated backups:
```bash
# Open crontab
crontab -e

# Write cron rule (Daily at 2:00 AM)
0 2 * * * /bin/bash /path/to/deployment/scripts/backup.sh gpa-db-prod YourSecurePassword123! >> /var/log/gpa-db-backup.log 2>&1
```
