#!/bin/bash

# Get the script's directory
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
# Get the project root directory (3 levels up from the script)
PROJECT_ROOT="$( cd "$SCRIPT_DIR/../../.." && pwd )"

# Build the project
echo "Building the project..."
cd "$PROJECT_ROOT"
dotnet build reasoning-engine.sln

# Run the weather scenario
echo "Running the weather scenario..."
dotnet run --project "$PROJECT_ROOT/ReasoningEngine" -- --run-scenario weather

echo "Done!"
