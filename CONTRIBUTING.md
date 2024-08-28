## Development Workflow

### Unit Testing
- **Framework**: The project uses NUnit for unit testing.
- **Current Coverage**: Unit tests currently cover the `GraphFileManager` class.
- **Running Tests**:
  1. Navigate to the project directory.
  2. Run the tests using the following command:
     ```bash
     dotnet test
     ```
- **Expanding Coverage**: Contributors are encouraged to write unit tests for other components such as `IndexManager`, `Node`, and `Edge`.
- **Test Structure**:
  - Follow the Arrange-Act-Assert pattern.
  - Ensure tests are isolated and do not rely on external resources.

### Coding Standards
- **Naming Conventions**:
  - `PascalCase` for class names, method names, and properties.
  - `camelCase` for local variables and parameters.
- **Code Structure**:
  - Organize code into meaningful namespaces (e.g., `ReasoningEngine.GraphFileHandling`).
  - Group related classes logically within namespaces.
- **Comments and Documentation**:
  - Use XML comments for public methods and classes.
  - Include inline comments only where necessary for clarity.
- **Error Handling**:
  - Use `try-catch` blocks for operations that may fail.
  - Log errors using `DebugWriter` or a similar consistent mechanism.

### Branching Strategy
- Use the branch naming scheme `<name-of-contributor>/<feature-branch-name>` for feature branches.
