KNOWN ISSUES:

- `issues.md` is not an ideal place to put known issues (this is a quick-and-dirty solution in lieu of proper issue tracking)
- Tests:
    - Unit test `TestLoadEdgeThatDoesNotExist` assumes node 9999999999999999 has no edges
    - Other unit tests MAY have unspecified underlying assumptions
- Instructions for usage and contribution to this repo are lacking

## TODO: Probability Distribution & Data Loading Refinements (from chat 2025-04-04)

Context: Issues identified during scenario data population and review of probability handling, leading to V3 framework refactoring. V1/V2 Nodes are removed.

1.  **`ProbabilityDistribution`: Sorted Inserts:** Modify `AddPoint` and `AddRange` to insert new elements while maintaining sorted order by `LowerBound`. (Improves consistency and enables deterministic tie-breaking).
2.  **`ProbabilityDistribution`: Relax `AddRange` Validation:** Remove the "too close" check (`Math.Abs(lowerBound - d.UpperBound) < EPSILON || ...`) to allow defining contiguous ranges necessary for `IsComplete`. Keep the significant overlap check.
3.  **`ProbabilityDistribution`: Deterministic `GetContainingRange`:** Ensure tie-breaking for points equidistant from boundaries is deterministic by relying on the list being sorted by `LowerBound` (as per point 1). The existing `ThenBy(r => r.Index)` should then suffice.
4.  **`ProbabilityDistribution`: Interpolation (Medium Term):** Consider modifying `GetProbability` (or adding a new method) to interpolate probability values for points near range boundaries to avoid discrete jumps in output probability.
5.  **`NodeFactory`: Parameter Parsing:** Implement robust parsing for `FunctionParams` in `CreateNodeFromPayload` and `UpdateNodeFromPayload`, converting string values to appropriate types based on `FunctionType`.
6.  **`PopulateWeatherScenarioData`: Update to V3:** Modify the script to use the new V3 payload format when calling `add_node` via `CommandProcessor` (e.g., specifying `Variable` or `Function` roles, `DomainType` for Variables, `FunctionType` and parameters for Functions). This is needed after the V3 framework changes are fully implemented.
7.  **`PopulateWeatherScenarioData`: Rerun:** After V3 framework changes and ProbabilityDistribution fixes (1-3, potentially 4) are implemented, and the scenario script is updated (6), rerun the scenario populator (`dotnet run --project ReasoningEngine/ReasoningEngine.csproj --run-scenario weather --verbosity Minimal`) to generate correct V3 data files.
8.  **Persistence:** Adapt `GraphFileManager` (`SaveNode`, `LoadNode`, `SaveEdge`, `LoadEdges`) to handle serialization/deserialization of `EdgeId` (Guid) and the `NodeV3` structure (including `Distribution`, `Function`, `FunctionParams`). Currently only `LoadNode` handles V3 structure partially.
9.  **`CommandProcessor`: `edit_node` Behavior:** Review `NodeFactory.UpdateNodeFromPayload` logic to ensure it correctly preserves state (like `Distribution` or `FunctionParams`) when only content is changed vs. when Role/FunctionType changes.
10. **Build & Test:** Perform a full build and run existing tests (updating them as necessary for V3) to catch compilation errors and regressions.

## General Future Improvements

- **API Enhancements (if WebServer is used):**
    - Add API rate limiting.
    - Add authentication/authorization.
- **Performance:** Implement response caching (e.g., in `GraphObjectMapper` or application layer) for frequently accessed nodes and edges.
- **Indexing:** Improve indexing in `FileGraphStorageProvider` for efficient Guid lookups and edge retrieval by node.
- **Data Import:** Develop the planned Data Import System to replace `ScenarioManager`. (See `README.md`)
- **ProbabilityDistribution:** Consider adding an explicit check for full domain coverage (e.g., ensuring no gaps > `2*EPSILON` exist across the entire conceptual domain, not just between defined ranges).
- **Code Quality:** Review codebase for non-conforming debug IDs (should be `#XXXXXX#` format with 6 alphanumeric chars) in `DebugWriter` calls and standardize them.

---
*Completed during refactoring (2025-04-04):*
*   *`GraphFileManager`: `LoadNode` Fix:* Partially updated `LoadNode` to recognize V3 structure based on Role. Needs further refinement for parameter/distribution deserialization.
*   *`GraphFileManager`: Verbosity Fix:* Adjusted `DebugWriter` calls for file operations to use `VerbosityLevel.Detailed`.
*   *Core Structure:* Defined `NodeRole`, `FunctionType`. Updated `NodeBase`, `EdgeBase`, `Edge`. Removed `NodeV1`/`V2`. Updated `NodeV3` and `Node` alias. Created `NodeFactory`. Refactored `CommandProcessor` to use `NodeFactory`.
