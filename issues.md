KNOWN ISSUES:

- `issues.md` is not an ideal place to put known issues (this is a quick-and-dirty solution in lieu of proper issue tracking)
- Tests:
    - Unit test `TestLoadEdgeThatDoesNotExist` assumes node 9999999999999999 has no edges
    - Other unit tests MAY have unspecified underlying assumptions
- Instructions for usage and contribution to this repo are lacking

## Active TODOs

Context: Ongoing refinements for V3 architecture.

1.  **(Verify)** **`ProbabilityDistribution`: Sorted Inserts:** Verify that `AddPoint` and `AddRange` correctly maintain sorted order by `LowerBound` via `FindInsertionIndex`. Consider adding specific tests.
2.  **(Refine)** **`ProbabilityDistribution`: Interpolation Logic:** Review the linear interpolation logic in `GetProbability` for points near boundaries. Ensure it's robust and consider alternative interpolation methods if needed. (Replaces original TODO #4).
3.  **`NodeFactory`: Parameter Parsing:** Implement robust parsing for `FunctionParams` in `CreateNodeFromPayload` and `UpdateNodeFromPayload`, converting string values to appropriate types based on `FunctionType`. (Original TODO #5).
4.  **Persistence:** Adapt persistence layer (`GraphObjectMapper`, `FileGraphStorageProvider`) to fully handle serialization/deserialization of the complete `NodeV3` structure (including `Distribution`, `Function`, `FunctionParams`) and `EdgeV2`. Verify current partial implementation. (Original TODO #8).
5.  **`ProbabilityDistribution`: Disallow `AddPoint` for `Truth` Domain:** Modify `AddPoint` to throw an `InvalidOperationException` if `DomainType` is `Truth`. The `Truth` domain represents a continuous probability value between [0, 1] and should only use `AddRange`. Point probabilities within this domain can be approximated using very narrow ranges if needed. (Original TODO #12).

## General Future Improvements

- **API Enhancements (if WebServer is used):**
    - Add API rate limiting.
    - Add authentication/authorization.
- **Performance:** Implement response caching (e.g., in `GraphObjectMapper` or application layer) for frequently accessed nodes and edges.
- **Indexing:** Improve indexing in `FileGraphStorageProvider` for efficient Guid lookups and edge retrieval by node.
- **Data Import:** Develop the planned Data Import System to replace `ScenarioManager`. (See `README.md`)
- **ProbabilityDistribution:** Consider adding an explicit check for full domain coverage (e.g., ensuring no gaps > `2*EPSILON` exist across the entire conceptual domain, not just between defined ranges).
- **ProbabilityDistribution:** Add methods for modifying/removing existing points or ranges (e.g., `RemovePoint`, `RemoveRange`, `ModifyProbability`, `CombineRanges`). Ensure these methods maintain internal consistency (sorted, non-overlapping, etc.).
- **UI/Tooling:** Implement a metadata-driven approach for UI components (like `ConsoleMenu` or future GUIs) to dynamically discover and prompt for node/edge parameters based on type/version, improving separation of concerns and generalizability.
- **Code Quality:** Review codebase for non-conforming debug IDs (should be `#XXXXXX#` format with 6 alphanumeric chars) in `DebugWriter` calls and standardize them.

---
*Completed (Current Session - 2025-04-11):*
*   **`ProbabilityDistribution`: Relax `AddRange` Validation:** Commented out "too close" check to allow adjacent ranges. (Original TODO #2).
*   **`PopulateWeatherScenarioData`: Update to V3 & Rerun:** Updated script payload format and successfully ran the scenario. (Original TODOs #6, #7).
*   **`CommandProcessor`: `edit_node` Behavior Review:** Reviewed logic; state handling seems correct, `ExtendedProperties` not preserved is known. (Original TODO #9).
*   **Build & Test:** Performed builds and test runs, fixed failures in web tests and probability tests (by aligning assertions with interpolation logic). (Original TODO #10).
*   **`CommandProcessor`: `edit_edge` Behavior:** Updated logic to load existing edge and preserve `EdgeId`. (Original TODO #11).
*   **(Obsolete)** **`ProbabilityDistribution`: Deterministic `GetContainingRange`:** Method removed; ambiguity handled by interpolation in `GetProbability`. (Original TODO #3).

*Completed (Previous Session - 2025-04-09):*
*   Addressed specific test failures related to edge serialization and deletion logic (`GraphFileHandlingUnitTests`, `CommandProcessorTests`).
*   Corrected `GraphObjectMapper` and `FileGraphStorageProvider` for edge handling.

*Completed (Refactoring - 2025-04-04):*
*   *`GraphFileManager`: `LoadNode` Fix:* Partially updated `LoadNode` to recognize V3 structure based on Role. Needs further refinement for parameter/distribution deserialization.
*   *`GraphFileManager`: Verbosity Fix:* Adjusted `DebugWriter` calls for file operations to use `VerbosityLevel.Detailed`.
*   *Core Structure:* Defined `NodeRole`, `FunctionType`. Updated `NodeBase`, `EdgeBase`, `Edge`. Removed `NodeV1`/`V2`. Updated `NodeV3` and `Node` alias. Created `NodeFactory`. Refactored `CommandProcessor` to use `NodeFactory`.
