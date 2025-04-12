# AI Assistant Guidelines & Context Transfer (version 1.1)

This document provides guidelines for AI assistants working on the Reasoning Engine project, particularly focusing on information needed for smooth context transfer between conversations.

## General Instructions & Conventions:

*   Refer to `CONTRIBUTING.md` for general development workflow guidelines (branching, basic coding standards).
*   **Error Handling:** Use specific exception types where appropriate. Log errors using `DebugUtils.DebugWriter`.
*   **Debugging:** Use `DebugUtils.DebugWriter` for logging. Debug messages should follow the format `#XXXXXX#` (unique 6-char tag) followed by the message.
*   **Serialization:** The project primarily uses `System.Text.Json` for serialization/deserialization.
*   **Architecture:** Be aware of the V3 functional graph approach (Nodes as Variables/Functions) and the decoupled persistence layer (`IGraphStorageProvider` for raw storage, `GraphObjectMapper` for object mapping).
*   **Tool Usage:**
    *   Handle potentially long command outputs (like `git diff`) by redirecting to a temporary file (`> output.txt.~`) and then reading the file. This way it does not get halted due to requiring user interaction.
    *   Use `dotnet build reasoning-engine.sln` to check for compilation errors/warnings. Do not rely solely on IDE linter feedback, but address build warnings when identified.
*   **Git Workflow:** Aim for small, logical commits with descriptive messages (e.g., using conventional commit prefixes like `feat:`, `fix:`, `refactor:`, `docs:`, `style:`, `chore:`).
*   **User Preferences & Custom Instructions:**
    *   Always seek explicit confirmation from the user before marking tasks or sub-tasks as complete. State what was done and ask if it's complete and what to do next.
    *   Justify and document the use of the null-forgiving operator (`!`) if its use is necessary (e.g., explain *why* the value is known to be non-null in that context). Prefer explicit null checks otherwise.
    *   Be aware of other custom instructions provided in the system prompt (e.g., re-reading files on repeated failures, handling potentially malformed linter line numbers, context window limits).
*   **Testing:** Write both unit and integration tests in the `ReasoningEngineTests` project. Use appropriate NUnit test setup (`[SetUp]`, `[TearDown]`) and teardown attributes to ensure test isolation.
*   **Environment Configuration:** Use environment variables and `.env` files for configuration (see `.env.example`). Always document required environment variables in `README.md`.
*   **Input Validation:** Ensure proper error handling and input validation, especially in user-facing methods or API endpoints.
*   **Versioning:** When implementing new features affecting core data structures, consider version compatibility (`IVersioned.cs`, `VersionCompatibilityAttribute`, `VersionCompatibilityChecker`) and update attributes as necessary.
*   **Persistence Awareness:** When updating edge or node structures, ensure that the changes are reflected correctly in the persistence layer (`GraphObjectMapper` and `IGraphStorageProvider` implementation). Be aware of the file structure conventions outlined in `ReasoningEngine/GraphFileHandling/FileManagement.md` if working with `FileGraphStorageProvider`.
*   **Placeholders:** If certain sections of code or documentation are not yet fully developed, use clear and concise placeholders (e.g., `// TODO: Implement...`, `<!-- TODO: Add details... -->`).
*   **Future Development:** Maintain a section for future development or a roadmap (e.g., in `README.md` or `issues.md`) to show that the project is actively evolving.
*   **Formatting:** When documenting directory structures and code blocks, remember to escape special characters (e.g., backticks in markdown) if necessary.
*   **(Web Server Specific):** If working on `WebServer.cs` or related API features:
    *   **API Design:** Follow RESTful principles but prioritize clarity. Include operation names in endpoints (e.g., `/api/nodes/{id}/update`).
    *   **Response Format:** Always use the `ApiResponse<T>` wrapper for consistent responses.
    *   **API Documentation:** Keep OpenAPI/Swagger documentation up to date with clear descriptions and examples.
    *   **Error Handling:** Provide meaningful error messages and appropriate HTTP status codes via the `ApiResponse<T>` wrapper.

## AI Assistant Workflow & Interaction Tips:

*   **Highlight Potential Issues:** Point out potential errors or questionable assumptions in the user's reasoning or existing code, even if they seem minor.
*   **Standardize Debugging:** Ensure consistent use of `DebugUtils.DebugWriter` for logging across the project.
*   **System Impact:** When making changes, consider the implications on the entire system, including data structures, persistence, indexing (if applicable), tests, and user interaction.
*   **Persistence Layer Awareness:** Be specifically aware of the decoupled persistence layer design (`IGraphStorageProvider`, `GraphObjectMapper`, `FileGraphStorageProvider`) when discussing or implementing changes related to data storage or retrieval.
*   **File Structure Awareness:** Ensure that any proposed changes to the file structure or indexing system align with the design principles outlined in `ReasoningEngine/GraphFileHandling/FileManagement.md`.
*   **(Web Server Specific):** If working on `WebServer.cs` or related API features:
    *   Consider API response format consistency and proper error handling in all endpoints.
    *   Follow the established pattern of including explicit operation names in URLs when designing new endpoints.
    *   Remember to update both implementation and tests when making changes to the API.

## Context Transfer Checklist:

When ending a development session or if the context window limit is approached, ensure the following information is captured (ideally in a dated summary file like `docs/ConversationSummary_YYYY-MM-DD.md` and potentially a separate `Notes` file for nuances):

1.  **High-Level Goal:** Briefly state the overall objective of the current feature or refactoring effort being worked on.
2.  **Last Completed Step:** Describe the last significant action or sub-task that was successfully completed and **committed**.
3.  **Current State:**
    *   **Build Status:** Does the project currently build successfully (`dotnet build reasoning-engine.sln`)? Are there any known build errors or warnings?
    *   **Test Status:** Were tests run recently (`dotnet test`)? Are there known failures?
    *   **Working Directory Status:** Is the `git status` clean? If not, what files are modified or untracked, and what is the nature of those changes?
    *   **Branch:** Current git branch name.
4.  **Immediate Next Step(s):** What was the planned next action before the conversation ended?
5.  **Key Decisions/Deviations (If any during the last session):** Note any significant choices made that differ from initial plans or require specific awareness.
6.  **Open Questions/Blockers (If any):** Are there any unresolved questions or issues preventing progress?
7.  **Relevant TODOs:** List any specific TODO items (e.g., from `issues.md` or code comments) that are directly relevant to the current workstream.
8,  **Increment Docs Version**: If any changes have been made to this other other documentation, incrememnt the version number of the document, either by minor version or major version.

*(This file can be updated as needed with more specific project guidelines.)*
