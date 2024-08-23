# Reasoning Engine: Transparent and Auditable AI Reasoning with LLM Support

## Overview
An AI system designed to exceed human-level problem-solving in general contexts by:
1. Encoding knowledge in a human-readable graph,
2. Performing explicit and auditable reasoning,
3. Incorporating new information via data ingestion algorithms assisted by LLMs, and
4. Enhancing knowledge accuracy and predictive power through refinement algorithms.

## Status
This project is still in development and has no current releases.

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
   \`\`\`bash
   dotnet build
   \`\`\`

3. Run the project:
   \`\`\`bash
   dotnet run --project reasoningEngine
   \`\`\`

### For Developers
Welcome to the Reasoning Engine project! To get started with contributing to or reviewing the project, please check out the [`dev`](https://github.com/your-repo/repository-name/tree/dev) branch.

## Contributing

We welcome contributions from the community! Before you begin, please take a moment to review our [CONTRIBUTING.md](CONTRIBUTING.md) file for detailed guidelines on the development workflow, coding standards, and testing practices.

### Steps to Contribute:

1. **Clone the repository**:
   \`\`\`bash
   git clone https://github.com/your-repo/repository-name.git
   \`\`\`
2. **Checkout the `dev` branch**:
   \`\`\`bash
   git checkout dev
   \`\`\`
3. **Create a new branch for your changes following the naming scheme `<name-of-contributor>/<feature-branch-name>`**:
   \`\`\`bash
   git checkout -b yourname/your-feature-branch
   \`\`\`
4. **Make your changes and commit them**:
   \`\`\`bash
   git add .
   git commit -m "Your commit message"
   \`\`\`
5. **Push your changes to your branch**:
   \`\`\`bash
   git push origin yourname/your-feature-branch
   \`\`\`
6. **Create a pull request** on GitHub targeting the `dev` branch.

## Project Structure

The Reasoning Engine project is organized into several key directories and files, each serving a specific purpose. Below is an overview of the structure:

### Directory Layout

\```
ReasoningEngine/
│
├── Core/
│   ├── Edge.cs              # Defines the Edge class representing connections in the graph
│   └── Node.cs              # Defines the Node class representing nodes in the graph
│
├── GraphFileHandling/
│   ├── GraphFileManager.cs   # Manages file operations related to nodes and edges
│   ├── IndexManager.cs       # Handles the index of nodes and edges
│   └── OneTimeSetup.cs       # Performs one-time setup tasks like creating directories
│
├── Utils/
│   ├── DebugUtils/
│   │   ├── DebugOptions.cs   # Provides options for debug settings
│   │   └── DebugWriter.cs    # Provides methods for writing debug and regular messages
│   └── AI Template.md        # Template for AI chatbots interaction prompts
│
├── Program.cs                # The main entry point of the application
├── ReasoningEngine.csproj    # Project file defining dependencies and build settings
└── .env                      # Environment variables configuration file
\```

### Core Components

- **Node.cs**: Defines the `Node` class, which represents individual nodes within the graph. Each node has an ID and associated content.
  
- **Edge.cs**: Defines the `Edge` class, which represents connections between nodes in the graph. Each edge has a source node, a destination node, a weight, and additional content.

- **GraphFileManager.cs**: Responsible for managing file operations related to nodes and edges. This includes saving and loading nodes and edges from disk.

- **IndexManager.cs**: Manages the index of nodes and edges, keeping track of where each node and edge is stored on disk.

- **OneTimeSetup.cs**: Handles initial setup tasks that need to be performed before the application starts, such as creating necessary directories and files.

- **DebugOptions.cs**: Provides options for enabling or disabling debug mode in the application.

- **DebugWriter.cs**: Utility class for writing debug and regular messages to the console, with options for inline or newline output.

- **Program.cs**: The main entry point of the application. It handles environment variable loading, initializing core components, and providing a user interface through a menu.

### Additional Components

- **.env**: Contains environment variables required by the application, such as `DATA_FOLDER_PATH`.

- **ReasoningEngine.csproj**: The project file that defines the project's dependencies, build settings, and target framework.

## Future Development

The Reasoning Engine is an evolving project, with many exciting features and improvements planned for the future. There's nothing here right now, but filling out this area is in progress—check back soon for updates!

## License

This project is licensed under the [MIT License](LICENSE).

## Acknowledgments

- [Original reasoning-engine repository](https://github.com/conormckenzie/reasoning-engine)
- Thanks to the contributors and open-source community for their support and tools.

## Contact

For any questions or issues, please reach out to [your contact email or issue tracker link].
