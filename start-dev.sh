#!/bin/bash

echo "🚀 Starting Document Management System..."
echo

# Function to check if port is in use
check_port() {
    if lsof -Pi :$1 -sTCP:LISTEN -t >/dev/null ; then
        echo "⚠️  Port $1 is already in use"
        return 1
    fi
    return 0
}

# Check ports
echo "🔍 Checking ports..."
check_port 5173 || echo "   Frontend port 5173 is busy"
check_port 7001 || echo "   Backend port 7001 is busy"
check_port 5001 || echo "   Backend HTTP port 5001 is busy"

echo

# Start backend
echo "🔧 [1/2] Starting Backend (.NET API)..."
cd DocumentManagementAPI
gnome-terminal --title="Backend API" -- bash -c "dotnet run; exec bash" &
cd ..

# Wait a bit for backend to start
echo "⏳ Waiting for backend to initialize..."
sleep 3

# Start frontend
echo "⚛️  [2/2] Starting Frontend (React)..."
gnome-terminal --title="Frontend React" -- bash -c "npm run dev; exec bash" &

echo
echo "✅ Both projects are starting..."
echo "📡 Backend will be available at: https://localhost:7001"
echo "🌐 Frontend will be available at: http://localhost:5173"
echo "📚 API Documentation: https://localhost:7001 (Swagger)"
echo
echo "Press Ctrl+C to stop this script (terminals will continue running)"
echo

# Keep script running
while true; do
    sleep 1
done