#!/bin/bash

# Configuration
RABBIT_CONTAINER_NAME="auto-mender-rabbit"
RABBIT_IMAGE="rabbitmq:3-management"
BACKEND_DIR="./AutoMender.Core"
FRONTEND_DIR="./AutoMender.Web"

echo "🚀 Starting AutoMender Development Environment..."

# 1. Start RabbitMQ
echo "🐰 Checking RabbitMQ..."
if [ "$(docker ps -q -f name=$RABBIT_CONTAINER_NAME)" ]; then
    echo "   Running."
elif [ "$(docker ps -aq -f name=$RABBIT_CONTAINER_NAME)" ]; then
    echo "   Starting existing container..."
    docker start $RABBIT_CONTAINER_NAME
else
    echo "   Creating new container..."
    docker run -d --hostname my-rabbit --name $RABBIT_CONTAINER_NAME -p 5672:5672 -p 15672:15672 $RABBIT_IMAGE
fi

# Wait for RabbitMQ to be ready (simple generic wait)
echo "⏳ Waiting 10s for RabbitMQ to initialize..."
sleep 10

# Create the queue explicitly to prevent "NOT_FOUND" errors in Azure Functions
echo "mw🔧 Creating 'incidents-queue'..."
curl -i -u guest:guest -H "content-type:application/json" -XPUT -d'{"auto_delete":false,"durable":true}' http://localhost:15672/api/queues/%2f/incidents-queue || echo "Queue creation warning (might already exist)"

# Cleanup function
cleanup() {
    echo ""
    echo "🛑 Shutting down..."
    kill $(jobs -p) 2>/dev/null
    # Aggressive cleanup of ports
    lsof -ti:7071 | xargs kill -9 2>/dev/null
    lsof -ti:5110 | xargs kill -9 2>/dev/null
    echo "✅ Done."
}
trap cleanup SIGINT SIGTERM

# Ensure clean state before starting
./stop-dev.sh

# 3. Start Backend (ASP.NET Core)
echo "🧠 Starting AutoMender.Core (Backend)..."
pushd AutoMender.Core > /dev/null
# Use dotnet run on port 7071
dotnet run --urls=http://localhost:7071 &
BACKEND_PID=$!
popd > /dev/null

# 3. Start Frontend
echo "💻 Building Frontend..."
cd $FRONTEND_DIR
dotnet build
echo "💻 Starting Frontend (Blazor)..."
dotnet run --no-build --urls=http://localhost:5110 &
FRONTEND_PID=$!
cd ..

echo "🎉 Environment is Ready!"
echo "   Backend:  http://localhost:7071"
echo "   Frontend: http://localhost:5110"
echo "   RabbitMQ: http://localhost:15672"
echo "Press Ctrl+C to stop everything."

# Wait for both processes
wait $BACKEND_PID $FRONTEND_PID
