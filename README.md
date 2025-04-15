# Reasoning Engine: Transparent and Auditable AI Reasoning with LLM Support

## Overview
An AI system designed to exceed human-level problem-solving in general contexts by:
1. Encoding knowledge in a human-readable graph,
2. Performing explicit and auditable reasoning, focusing on logical inference with transparency and auditability,
3. Incorporating new information via data ingestion algorithms assisted by LLMs, and
4. Enhancing knowledge accuracy and predictive power through refinement algorithms.

## Status
This project is under active development and currently undergoing a major refactoring towards a V3 knowledge representation framework (see `docs/KnowledgeRepresentationV3.md`). There are no stable releases yet.

## Configuration
- **Environment Variables**: The application relies on the following environment variable, which should be set in the `.env` file:
   - `DATA_FOLDER_PATH`: The path to the main data directory where graph data and other files will be stored. Example:
     ```plaintext
     DATA_FOLDER_PATH=/path/to/your/data/folder
     ```
   - Ensure this environment variable is set before running the application.

## Getting Started

### Quick Start
1. Ensure that the necessary environment variables are set in the `.env` file.
   - Example `.env` file:
     ```plaintext
     DATA_FOLDER_PATH=/path/to/your/data/folder
     ```
2. Build the project using the .NET CLI:
   ```bash
   dotnet build
   ```

3. Run the project:
   ```bash
   dotnet run --project ReasoningEngine/ReasoningEngine.csproj
   ```
   (This starts the interactive console menu).

4. **Run Scenarios (Optional):**
   ```bash
   # Example: Run the weather scenario with minimal output
   dotnet run --project ReasoningEngine/ReasoningEngine.csproj --run-scenario weather --verbosity Minimal 
   ```
   (See `dotnet run --project ReasoningEngine/ReasoningEngine.csproj --help` for more options).

5. **Run Tests:**
   ```bash
   dotnet test
   ```
   (Note: Tests may be broken during active refactoring).

### For Developers
Welcome to the Reasoning Engine project! To get started with contributing to or reviewing the project, please check out the [`dev`](https://github.com/conormckenzie/reasoning-engine/tree/dev) branch.

## Contributing

We welcome contributions from the community! Before you begin, please take a moment to review our [CONTRIBUTING.md](CONTRIBUTING.md) file for detailed guidelines on the development workflow, coding standards, and testing practices.

### Steps to Contribute:

1. **Clone the repository**:
   ```bash
   git clone https://github.com/conormckenzie/reasoning-engine.git
   ```
2. **Checkout the `dev` branch**:
   ```bash
   git checkout dev
   ```
3. **Create a new branch for your changes following the naming scheme `<name-of-contributor>/<feature-branch-name>`**:
   ```bash
   git checkout -b yourname/your-feature-branch
   ```
4. **Make your changes and commit them**:
   ```bash
   git add .
   git commit -m "Your commit message"
   ```
5. **Push your changes to your branch**:
   ```bash
   git push origin yourname/your-feature-branch
   ```
6. **Create a pull request** on GitHub targeting the `dev` branch.

## Project Structure

The Reasoning Engine project is organized into several key directories and files, each serving a specific purpose. Below is an overview of the structure:

### Directory Layout

```
ReasoningEngine/
│
├── Core/                     # Core data structures (Nodes, Edges, Probabilities)
│   ├── NodeBase.cs           # Abstract base class for nodes
│   ├── Node.cs               # Defines NodeV3 and the Node alias
│   ├── EdgeBase.cs           # Abstract base class for edges
│   ├── Edge.cs               # Defines EdgeV2 and the Edge alias
│   ├── ProbabilityDistribution.cs # Class for handling probability distributions
│   ├── NodeRoles.cs          # Enum for NodeRole (Variable, Function)
│   ├── FunctionTypes.cs      # Enum for FunctionType (Linear, DefinedOp, NeuralNet)
│   ├── DomainTypes.cs        # Enums for DomainType, DomainInterpretation
│   ├── NodeFactory.cs        # Creates/updates Node objects from parameters
│   ├── IVersioned.cs         # Interface for versioned elements
│   ├── VersionCompatibilityAttribute.cs # Attribute for algorithm compatibility
│   └── VersionCompatibilityChecker.cs # Checks algorithm compatibility
│
├── GraphFileHandling/        # Persistence layer
│   ├── IGraphStorageProvider.cs # Interface for raw data storage
│   ├── FileGraphStorageProvider.cs # File-based implementation of IGraphStorageProvider (formerly GraphFileManager.cs)
│   ├── GraphObjectMapper.cs  # Handles object mapping & serialization using IGraphStorageProvider
│   └── IndexManager.cs       # Manages the main node index file (`index.json`) for node lookups
│
├── GraphOperations/          # High-level API and UI
│   ├── CommandProcessor.cs   # Processes string commands to manipulate the graph
│   └── ConsoleMenu.cs        # Interactive console UI
│
├── GraphAlgorithms/          # Reasoning algorithms (currently stub)
│   └── GraphAlgorithmsStub.cs
│
├── Utils/                    # Utility classes
│   ├── DebugUtils/           # Debugging helpers
│   └── Scenarios/            # Scenario loading/management (to be replaced by Data Import)
│
├── OneTimeSetup.cs           # Handles initial setup (e.g., creating data folder)
├── Program.cs                # Main application entry point (CLI args, Menu)
├── WebServer.cs              # ASP.NET Core web API (optional entry point)
├── ReasoningEngine.csproj    # Project file
└── .env                      # Environment variables (requires .env file based on .env.example)

ReasoningEngineTests/         # Unit and integration tests
│   └── ...
```

### Core Components (V3 Framework - In Progress)

See `docs/KnowledgeRepresentationV3.md` for full details.

- **Nodes (`Node.cs`, `NodeBase.cs`, `NodeRoles.cs`):** Nodes represent variables or functions. `NodeRole` determines behavior. `Variable` nodes hold a `ProbabilityDistribution`. Inherit from `NodeBase`.
- **Edges (`Edge.cs`, `EdgeBase.cs`):** Represent dependencies. Now include a `Guid EdgeId`. Inherit from `EdgeBase`.
- **Versioning (`IVersioned.cs`, `VersionCompatibility...`):** Core elements implement `IVersioned`. `VersionCompatibilityChecker` and `VersionCompatibilityAttribute` manage algorithm compatibility with different data versions.
- **Probability (`ProbabilityDistribution.cs`):** Handles uncertainty representation for `Variable` nodes using concepts like `EPSILON` uncertainty, specific domain types (`Continuous`, `DiscreteInteger`, `Truth`), and defined range/point operations. See `ReasoningEngine/Core/ProbabilityDistributions.md` for details.
- **Functions (`FunctionTypes.cs`):** Define operations (`Linear`, `DefinedOp`, `NeuralNet`) performed by `Function` nodes.
- **Node Factory (`NodeFactory.cs`):** Creates/updates `Node` objects based on input parameters (currently `Dictionary<string, object>`).
- **Persistence (`GraphFileHandling/`):** Decoupled persistence layer.
    - `IGraphStorageProvider`: Interface defining raw data storage operations.
    - `FileGraphStorageProvider` (in `FileGraphStorageProvider.cs`): Implements `IGraphStorageProvider` using the file system (see `ReasoningEngine/GraphFileHandling/FileManagement.md` for file structure details). Uses `IndexManager` for node lookups.
    - `GraphObjectMapper`: Handles serialization/deserialization and mapping between domain objects (`Node`, `Edge`) and the storage provider.
- **Command Processor (`GraphOperations/CommandProcessor.cs`):** Provides a string-based API for graph manipulation, using `NodeFactory` and `GraphObjectMapper`. The payload format is typically `id|content|param1=value1|param2=value2...`. Note that for `Function` nodes, the `FunctionParams` parameter expects a nested structure formatted as a single string: `"FunctionParams=Key1:Value1;Key2:Value2;..."`.
- **Entry Points (`Program.cs`, `WebServer.cs`):** Provide console and web API access. `Program.cs` also handles command-line arguments for tasks like running scenarios or setup.
    - `WebServer.cs` (Optional): Exposes functionality via an ASP.NET Core RESTful API. Uses `ApiResponse<T>` for consistent responses and explicit operation names in endpoints (e.g., `/api/nodes/{id}/update`). Includes OpenAPI/Swagger documentation.

### Additional Components

- **.env**: Contains environment variables required by the application, such as `DATA_FOLDER_PATH`.

- **ReasoningEngine.csproj**: The project file that defines the project's dependencies, build settings, and target framework.

## Future Development

The Reasoning Engine is an evolving project, with many exciting features and improvements planned for the future:

### Data Import System

The current scenario manager functionality (in `ReasoningEngine/Utils/Scenarios/`) is planned to be developed into a comprehensive data import feature that will:

- Allow users to define scenarios in external files (JSON, XML, YAML) rather than hardcoded C# classes
- Support importing from various data sources (files, databases, APIs)
- Provide validation and error handling for imported data
- Enable non-developers to create and modify scenarios without changing code
- Facilitate easier testing and demonstration of the reasoning engine with predefined datasets

This evolution will improve flexibility, maintainability, and user-friendliness by separating data from code and providing a standardized way to populate the reasoning graph.

### Long-Term Goals
- Expand the engine's reasoning capabilities to handle more complex queries.
- Refine the knowledge base dynamically through interaction with LLMs.
- Consider performance optimizations (e.g., caching, indexing improvements, alternative storage backends) and integration with large language models.
- Address concurrency and consistency concerns as the system scales.
- Implement comprehensive test coverage for both unit and integration tests.
- Develop API documentation and interactive exploration tools (especially if the Web API becomes a primary interface).

## License

This project is licensed under the Creative Commons Attribution-NonCommercial 4.0 International (CC BY-NC 4.0) License.

- **NonCommercial**: You may not use the material for commercial purposes.
- **Attribution**: You must give appropriate credit, provide a link to the license, and indicate if changes were made.

For more details, you can view the full license [here](http://creativecommons.org/licenses/by-nc/4.0/).

## Acknowledgments

- [Original reasoning-engine repository](https://github.com/conormckenzie/reasoning-engine)
- Thanks to the contributors and open-source community for their support and tools.

## Contact

For any questions or issues, please reach out to [conor.mckenzie314@protonmail.com].
