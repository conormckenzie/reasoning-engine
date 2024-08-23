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
   ```bash
   dotnet build
   ```

3. Run the project:
   ```bash
   dotnet run --project reasoningEngine
   ```

### For Developers
Welcome to the Reasoning Engine project! To get started with contributing to or reviewing the project, please check out the [`dev`](https://github.com/your-repo/repository-name/tree/dev) branch.

## Contributing

We welcome contributions from the community! Before you begin, please take a moment to review our [CONTRIBUTING.md](CONTRIBUTING.md) file for detailed guidelines on the development workflow, coding standards, and testing practices.

### Steps to Contribute:

1. **Clone the repository**:
   ```bash
   git clone https://github.com/your-repo/repository-name.git
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

## Features
(To be updated as the project develops)

## License

This project is licensed under the [MIT License](LICENSE).

## Acknowledgments

- [Original reasoning-engine repository](https://github.com/conormckenzie/reasoning-engine)
- Thanks to the contributors and open-source community for their support and tools.

## Contact

For any questions or issues, please reach out to [your contact email or issue tracker link].
