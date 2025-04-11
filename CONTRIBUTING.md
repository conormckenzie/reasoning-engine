# Contributing to the Reasoning Engine

Thank you for your interest in contributing! Please take a moment to review these guidelines.

## Development Workflow

### Branches
- **`main`**: Represents the most stable version (although currently pre-release). Direct commits are discouraged.
- **`dev`**: The primary development branch where features are integrated. Target pull requests here.
- **Feature Branches**: Create branches off `dev` for new features or bug fixes using the naming scheme: `<your-name-or-github-handle>/<short-feature-description>` (e.g., `jdoe/fix-probability-interpolation`).

### Commits
- Write clear and concise commit messages.
- Use conventional commit prefixes if possible (e.g., `feat:`, `fix:`, `docs:`, `test:`, `refactor:`).
- Group related changes into logical commits.

### Pull Requests (PRs)
- Target PRs against the `dev` branch.
- Provide a clear description of the changes made and the problem being solved.
- Reference any relevant issues (e.g., from `issues.md` or a future tracking system).
- Ensure tests pass before submitting a PR (see Testing section).
- Expect code review and address feedback promptly.

## Testing
- **Framework**: The project uses NUnit (`v3`) for unit and integration testing.
- **Project Location**: Tests reside in the `ReasoningEngineTests/` project.
- **Current Coverage**: Tests cover various components including:
    - Core data structures (`ProbabilityDistribution`)
    - Persistence layer (`FileGraphStorageProvider`, `GraphObjectMapper`)
    - Graph operations (`CommandProcessor`)
    - Web API (`WebServerIntegrationTests`, `WebServerUnitTests`)
- **Running Tests**:
  - From the root directory:
    ```bash
    dotnet test reasoning-engine.sln 
    # Or simply:
    dotnet test 
    ```
  - To run specific tests, use filtering options (consult `dotnet test --help`).
- **Writing Tests**:
  - New features or bug fixes should ideally include corresponding tests.
  - Follow the Arrange-Act-Assert pattern.
  - Use descriptive test names (`PascalCase`).
  - Ensure unit tests are isolated (use mocking/stubs if necessary, though currently limited).
  - Integration tests (like `WebServerIntegrationTests` or tests involving `FileGraphStorageProvider`) may require setup/teardown to manage test data (e.g., in temporary directories).

## Coding Standards & Style
- **Language**: C# (targeting .NET 9 currently).
- **Style**: Follow standard .NET coding conventions (refer to Microsoft's C# Coding Conventions). Key points:
  - `PascalCase` for types, methods, properties, events, enums.
  - `camelCase` for local variables and method parameters.
  - Use `var` when the type is obvious from the right-hand side, otherwise use explicit types.
  - Prefer expression-bodied members for simple implementations.
- **Code Structure**:
  - Organize code into logical namespaces (e.g., `ReasoningEngine.Core`, `ReasoningEngine.GraphFileHandling`).
  - Keep classes focused on a single responsibility.
- **Comments and Documentation**:
  - Use XML documentation comments (`///`) for all public types and members. Explain purpose, parameters, and return values.
  - Use inline comments (`//`) sparingly for complex or non-obvious logic.
  - Update relevant markdown documentation (`README.md`, `docs/`, etc.) when making significant changes to architecture or functionality.
- **Error Handling**:
  - Use exceptions for exceptional circumstances.
  - Use `try-catch` for operations prone to external failures (e.g., file I/O).
  - Log errors using `DebugUtils.DebugWriter` with appropriate verbosity levels and unique IDs (`#XXXXXX#`).
- **Dependencies**: Minimize external dependencies where possible. Discuss adding new NuGet packages in an issue or PR.

## Documentation
- Keep `README.md` updated with project status, setup instructions, and high-level overview.
- Maintain detailed design documents in the `docs/` directory (e.g., `KnowledgeRepresentationV3.md`).
- Document core classes and methods using XML comments.
- Update `issues.md` or the designated issue tracker with TODOs, known issues, and completed tasks.

Thank you for contributing!
