#!/bin/bash

echo "Launching the FeedbackBoard development environment..."

# Running Docker Compose
docker-compose up -d miniblue redis sqlserver

echo "We are waiting for the services to be ready..."

# miniblue waiting
until curl -s http://localhost:4566/_health > /dev/null; do
    echo "Waiting for miniblue..."
    sleep 2
done

echo "✅ miniblue is ready"

# start the setup
echo "Setting up Azure services..."
docker-compose up miniblue-setup

# .NET projects running
echo "🚀 Running .NET services..."
dotnet run --project src/FeedbackBoard.Api

echo "The environment is ready!"
echo "API: http://localhost:5000"
echo "Swagger: http://localhost:5000/swagger"