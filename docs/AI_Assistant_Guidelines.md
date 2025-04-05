# AI Assistant Guidelines & Context Transfer (version 1.0)

This document provides guidelines for AI assistants working on the Reasoning Engine project, particularly focusing on information needed for smooth context transfer between conversations.

## General Instructions & Conventions:

*   Refer to `CONTRIBUTING.md` for general development workflow guidelines (branching, basic coding standards).
*   **Error Handling:** Use specific exception types where appropriate. Log errors using `DebugUtils.DebugWriter`.
*   **Debugging:** Use `DebugUtils.DebugWriter` for logging. Debug messages should follow the format `#XXXXXX#` (unique 6-char tag) followed by the message.
*   **Serialization:** The project primarily uses `Newtonsoft.Json` for serialization/deserialization.
*   **Architecture:** Be aware of the V3 functional graph approach (Nodes as Variables/Functions) and the decoupled persistence layer (`IGraphStorageProvider` for raw storage, `GraphObjectMapper` for object mapping).
*   **Tool Usage:**
    *   Handle potentially long command outputs (like `git diff`) by redirecting to a temporary file (`> output.txt.~`) and then reading the file. This way it does not get halted due to requiring user interaction.
    *   Use `dotnet build reasoning-engine.sln` to check for compilation errors/warnings. Do not rely solely on IDE linter feedback, but address build warnings when identified.
*   **Git Workflow:** Aim for small, logical commits with descriptive messages (e.g., using conventional commit prefixes like `feat:`, `fix:`, `refactor:`, `docs:`, `style:`, `chore:`).
*   **User Preferences & Custom Instructions:**
    *   Always seek explicit confirmation from the user before marking tasks or sub-tasks as complete. State what was done and ask if it's complete and what to do next.
    *   Justify and document the use of the null-forgiving operator (`!`) if its use is necessary (e.g., explain *why* the value is known to be non-null in that context). Prefer explicit null checks otherwise.
    *   Be aware of other custom instructions provided in the system prompt (e.g., re-reading files on repeated failures, handling potentially malformed linter line numbers, context window limits).

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
