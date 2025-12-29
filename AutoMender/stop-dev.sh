#!/bin/bash
echo "🧹 Cleaning up AutoMender processes..."

# Kill Azure Functions
pkill -f "func start"
pkill -f "AutoMender.Core.dll"

# Kill Blazor Frontend
pkill -f "dotnet run --urls=http://localhost:5110"
pkill -f "dotnet watch"
pkill -f "AutoMender.Web.dll"
pkill -f "VBCSCompiler"

# Aggressive Port Cleanup
lsof -ti:7071 | xargs kill -9 2>/dev/null
lsof -ti:5110 | xargs kill -9 2>/dev/null

# Optional: Stop RabbitMQ if desired (commented out by default to keep state)
# docker stop auto-mender-rabbit

echo "✅ All clean."
