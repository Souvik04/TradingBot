#!/bin/bash

echo "🚀 Starting TradingBot..."

# Check if Docker is running
if ! docker info > /dev/null 2>&1; then
    echo "❌ Docker is not running. Please start Docker and try again."
    exit 1
fi

# Check if .env file exists, if not create from example
if [ ! -f .env ]; then
    echo "📝 Creating .env file from template..."
    cp env.example .env
    echo "✅ Created .env file. Please edit it with your configuration."
fi

# Build and start services
echo "🔨 Building and starting services..."
docker-compose up --build -d

# Wait for services to be ready
echo "⏳ Waiting for services to be ready..."
sleep 10

# Check service health
echo "🏥 Checking service health..."

# Check API health
if curl -f http://localhost:5000/api/health > /dev/null 2>&1; then
    echo "✅ API is healthy"
else
    echo "❌ API health check failed"
fi

# Check Signal Engine health
if curl -f http://localhost:8000/health > /dev/null 2>&1; then
    echo "✅ Signal Engine is healthy"
else
    echo "❌ Signal Engine health check failed"
fi

echo ""
echo "🎉 TradingBot is ready!"
echo ""
echo "📖 Access points:"
echo "   API Documentation: http://localhost:5000/swagger"
echo "   Signal Engine Docs: http://localhost:8000/docs"
echo "   API Health: http://localhost:5000/api/health"
echo "   Signal Engine Health: http://localhost:8000/health"
echo ""
echo "📋 Quick test commands:"
echo "   curl http://localhost:5000/api/trade/config"
echo "   curl -X POST http://localhost:8000/signal -H 'Content-Type: application/json' -d '{\"symbol\": \"AAPL\"}'"
echo ""
echo "📝 To view logs: docker-compose logs -f"
echo "🛑 To stop: docker-compose down" 