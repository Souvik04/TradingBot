@echo off
echo 🚀 Starting TradingBot...

REM Check if Docker is running
docker info >nul 2>&1
if %errorlevel% neq 0 (
    echo ❌ Docker is not running. Please start Docker and try again.
    pause
    exit /b 1
)

REM Check if .env file exists, if not create from example
if not exist .env (
    echo 📝 Creating .env file from template...
    copy env.example .env
    echo ✅ Created .env file. Please edit it with your configuration.
)

REM Build and start services
echo 🔨 Building and starting services...
docker-compose up --build -d

REM Wait for services to be ready
echo ⏳ Waiting for services to be ready...
timeout /t 10 /nobreak >nul

echo.
echo 🎉 TradingBot is ready!
echo.
echo 📖 Access points:
echo    API Documentation: http://localhost:5000/swagger
echo    Signal Engine Docs: http://localhost:8000/docs
echo    API Health: http://localhost:5000/api/health
echo    Signal Engine Health: http://localhost:8000/health
echo.
echo 📋 Quick test commands:
echo    curl http://localhost:5000/api/trade/config
echo    curl -X POST http://localhost:8000/signal -H "Content-Type: application/json" -d "{\"symbol\": \"AAPL\"}"
echo.
echo 📝 To view logs: docker-compose logs -f
echo 🛑 To stop: docker-compose down
echo.
pause 