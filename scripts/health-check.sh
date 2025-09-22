#!/bin/bash

# Health Check and Auto-Recovery Script
# Run every 5 minutes via cron: */5 * * * * /path/to/health-check.sh

# Configuration
API_URL="https://api.yourdomain.com"
FRONTEND_URL="https://yourdomain.com"
SERVICE_NAME="document-management"
LOG_FILE="/var/log/health-check.log"
ALERT_EMAIL="admin@yourdomain.com"
MAX_RETRIES=3
RETRY_DELAY=30

# Logging function
log() {
    echo "$(date '+%Y-%m-%d %H:%M:%S') - $1" | tee -a $LOG_FILE
}

# Send alert function
send_alert() {
    local subject="$1"
    local message="$2"
    echo "$message" | mail -s "$subject" $ALERT_EMAIL
    log "Alert sent: $subject"
}

# Check API health
check_api() {
    local retry_count=0
    
    while [ $retry_count -lt $MAX_RETRIES ]; do
        if curl -f -s --max-time 10 "$API_URL/health" > /dev/null; then
            log "API health check passed"
            return 0
        else
            retry_count=$((retry_count + 1))
            log "API health check failed (attempt $retry_count/$MAX_RETRIES)"
            
            if [ $retry_count -lt $MAX_RETRIES ]; then
                sleep $RETRY_DELAY
            fi
        fi
    done
    
    return 1
}

# Check frontend
check_frontend() {
    if curl -f -s --max-time 10 "$FRONTEND_URL" > /dev/null; then
        log "Frontend health check passed"
        return 0
    else
        log "Frontend health check failed"
        return 1
    fi
}

# Check database connection
check_database() {
    if mysql -u docuser -pyour_very_strong_password_123! -e "SELECT 1" DocumentManagementDB > /dev/null 2>&1; then
        log "Database health check passed"
        return 0
    else
        log "Database health check failed"
        return 1
    fi
}

# Check disk space
check_disk_space() {
    local usage=$(df / | tail -1 | awk '{print $5}' | sed 's/%//')
    
    if [ $usage -gt 90 ]; then
        log "CRITICAL: Disk usage is ${usage}%"
        send_alert "CRITICAL: Disk Space Alert" "Disk usage is ${usage}% on $(hostname)"
        return 1
    elif [ $usage -gt 80 ]; then
        log "WARNING: Disk usage is ${usage}%"
        send_alert "WARNING: Disk Space Alert" "Disk usage is ${usage}% on $(hostname)"
    else
        log "Disk usage is ${usage}% - OK"
    fi
    
    return 0
}

# Check memory usage
check_memory() {
    local usage=$(free | grep Mem | awk '{printf "%.0f", $3/$2 * 100.0}')
    
    if [ $usage -gt 90 ]; then
        log "CRITICAL: Memory usage is ${usage}%"
        send_alert "CRITICAL: Memory Alert" "Memory usage is ${usage}% on $(hostname)"
        return 1
    elif [ $usage -gt 80 ]; then
        log "WARNING: Memory usage is ${usage}%"
    else
        log "Memory usage is ${usage}% - OK"
    fi
    
    return 0
}

# Restart service
restart_service() {
    log "Attempting to restart $SERVICE_NAME service..."
    
    if sudo systemctl restart $SERVICE_NAME; then
        log "Service restarted successfully"
        sleep 10
        
        if check_api; then
            log "Service recovery successful"
            send_alert "Service Recovery" "$SERVICE_NAME service was restarted and is now healthy on $(hostname)"
            return 0
        else
            log "Service restart failed to resolve the issue"
            return 1
        fi
    else
        log "Failed to restart service"
        return 1
    fi
}

# Main health check
main() {
    log "Starting health check..."
    
    local api_healthy=true
    local frontend_healthy=true
    local db_healthy=true
    
    # Check API
    if ! check_api; then
        api_healthy=false
        send_alert "API Health Check Failed" "API at $API_URL is not responding on $(hostname)"
        
        # Attempt service restart
        if restart_service; then
            api_healthy=true
        else
            send_alert "CRITICAL: Service Recovery Failed" "Failed to recover $SERVICE_NAME service on $(hostname). Manual intervention required."
        fi
    fi
    
    # Check Frontend
    if ! check_frontend; then
        frontend_healthy=false
        send_alert "Frontend Health Check Failed" "Frontend at $FRONTEND_URL is not responding"
    fi
    
    # Check Database
    if ! check_database; then
        db_healthy=false
        send_alert "Database Health Check Failed" "Database connection failed on $(hostname)"
    fi
    
    # Check system resources
    check_disk_space
    check_memory
    
    # Overall status
    if $api_healthy && $frontend_healthy && $db_healthy; then
        log "All health checks passed"
    else
        log "Some health checks failed - manual investigation may be required"
    fi
    
    log "Health check completed"
}

# Run main function
main