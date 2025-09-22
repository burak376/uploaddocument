@echo off
echo Starting Document Management System...
echo.

echo [1/2] Starting Backend (.NET API)...
start "Backend API" cmd /k "cd DocumentManagementAPI && dotnet run"

echo [2/2] Starting Frontend (React)...
timeout /t 3 /nobreak > nul
start "Frontend React" cmd /k "npm run dev"

echo.
echo Both projects are starting...
echo Backend will be available at: https://localhost:7001
echo Frontend will be available at: http://localhost:5173
echo.
pause