#!/bin/bash

# Production Backup Script
# Run daily via cron: 0 2 * * * /path/to/backup.sh

set -e

# Configuration
DATE=$(date +%Y%m%d_%H%M%S)
BACKUP_DIR="/backups"
DB_NAME="DocumentManagementDB"
DB_USER="docuser"
DB_PASS="your_very_strong_password_123!"
DB_HOST="localhost"
UPLOADS_DIR="/var/www/document-management/uploads"
RETENTION_DAYS=30
LOG_FILE="/var/log/backup.log"

# Create backup directory if it doesn't exist
mkdir -p $BACKUP_DIR

# Logging function
log() {
    echo "$(date '+%Y-%m-%d %H:%M:%S') - $1" | tee -a $LOG_FILE
}

log "Starting backup process..."

# Database backup
log "Creating database backup..."
if mysqldump -h$DB_HOST -u$DB_USER -p$DB_PASS $DB_NAME | gzip > $BACKUP_DIR/db_$DATE.sql.gz; then
    log "Database backup completed: db_$DATE.sql.gz"
else
    log "ERROR: Database backup failed!"
    exit 1
fi

# Files backup
log "Creating files backup..."
if tar -czf $BACKUP_DIR/files_$DATE.tar.gz -C $(dirname $UPLOADS_DIR) $(basename $UPLOADS_DIR); then
    log "Files backup completed: files_$DATE.tar.gz"
else
    log "ERROR: Files backup failed!"
    exit 1
fi

# Upload to cloud storage (uncomment and configure as needed)
# log "Uploading to cloud storage..."
# aws s3 cp $BACKUP_DIR/db_$DATE.sql.gz s3://your-backup-bucket/database/
# aws s3 cp $BACKUP_DIR/files_$DATE.tar.gz s3://your-backup-bucket/files/

# Clean old backups
log "Cleaning old backups (older than $RETENTION_DAYS days)..."
find $BACKUP_DIR -name "db_*.sql.gz" -mtime +$RETENTION_DAYS -delete
find $BACKUP_DIR -name "files_*.tar.gz" -mtime +$RETENTION_DAYS -delete

# Calculate backup sizes
DB_SIZE=$(du -h $BACKUP_DIR/db_$DATE.sql.gz | cut -f1)
FILES_SIZE=$(du -h $BACKUP_DIR/files_$DATE.tar.gz | cut -f1)

log "Backup process completed successfully!"
log "Database backup size: $DB_SIZE"
log "Files backup size: $FILES_SIZE"

# Send notification (optional)
# echo "Backup completed successfully on $(hostname) at $(date)" | mail -s "Backup Success" admin@yourdomain.com