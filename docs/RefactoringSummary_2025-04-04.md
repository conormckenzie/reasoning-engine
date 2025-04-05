# Refactoring Summary & State (2025-04-04)

This document summarizes the discussion and refactoring efforts undertaken on April 4th, 2025, regarding the Reasoning Engine project, focusing on understanding the codebase, addressing data loading issues, and evolving the core knowledge representation framework.

## 1. Summary of Discussion & Actions

1.  **Initial Codebase Exploration:** Analyzed the project structure (`ReasoningEngine`, `Core`, `GraphFileHandling`, `GraphOperations`, `Utils`, `Tests`) using `list_code_definition_names` to understand components like Nodes, Edges, Probability Distributions, Command Processor, Persistence, Scenarios, etc. Identified `GraphAlgorithms` as a stub.
2.  **Data Investigation:** Examined the `data/` directory. Found node/edge JSON files and index files. Identified that `SIMONode` JSON files (e.g., node 1, 2) were missing the actual probability distribution data.
3.  **Debugging Scenario Script:** Investigated `PopulateWeatherScenarioData.cs`. Found that the script correctly created nodes/edges via `CommandProcessor` but failed to populate distributions because:
    *   `LoadSIMONodes` was a placeholder returning an empty list.
    *   Code to call `AddDistributionPoint`/`AddRange` was commented out.
    *   No logic existed to save the modified nodes back.
4.  **Debugging Persistence Layer:**
    *   Identified that `GraphFileManager.LoadNode` was not correctly deserializing derived node types (SIMO/MISO), always returning `NodeV2`. Fixed this by checking the `Type` property in the JSON.
    *   Identified that verbose output during scenario runs (even with `--verbosity Minimal`) was due to `DebugWriter` calls in `GraphFileManager` defaulting to `Minimal`. Fixed this by setting relevant calls to `Detailed`.
5.  **`ProbabilityDistribution` Analysis:**
    *   Reviewed the logic for handling `Truth`, `DiscreteInteger`, and `Continuous` domains.
    *   Identified a conflict: `AddRange` validation (`Math.Abs(b1 - b2) < EPSILON`) prevented creating perfectly adjacent ranges (gap=0), while `IsComplete` allowed small gaps (`<= 1.9 * EPSILON`), including zero gaps. This prevented the scenario script from adding contiguous ranges.
    *   Discussed determinism near boundaries (`b +/- delta`) and the role of `GetContainingRange`. Acknowledged that sensitivity to calculation errors near `boundary +/- EPSILON` exists even with deterministic tie-breaking.
6.  **Knowledge Representation Refactoring (V3 Framework):** Based on the limitations identified (especially lack of CPT support) and future goals (scalability, GPU), decided to refactor the core representation:
    *   **Unified Node (`NodeV3`):** Replaced `SIMONode`/`MISONode`/`NodeType` with a single `NodeV3` class using `NodeRole` enum (`Variable`, `Function`).
    *   **Functional Dependencies:** Adopted a model where `Variable` nodes hold belief `P(Self)`, and `Function` nodes compute outputs that *influence* these beliefs, rather than storing `P(Child | Parents)` directly.
    *   **Simplified `FunctionType`:** Reduced to { `Linear`, `DefinedOp`, `NeuralNet` }.
    *   **Edge IDs:** Added `Guid EdgeId` to edges.
    *   **Removed V1/V2:** Eliminated `NodeV1`, `NodeV2`, `EdgeV1`, and related upgrade logic/compatibility concerns.
    *   **`NodeFactory`:** Introduced to handle V3 node creation/updates based on payloads, decoupling `CommandProcessor`.
    *   **Payload Format:** Switched from pipe-delimited strings to `Dictionary<string, object>` for `NodeFactory` input to improve flexibility over specific DTOs.
7.  **Persistence Decoupling:** Discussed separating storage mechanics from serialization logic. Defined an `IGraphStorageProvider` interface and started refactoring `GraphFileManager` into `FileGraphStorageProvider` implementing this interface.
8.  **Documentation:** Created `docs/KnowledgeRepresentationV3.md` and updated `issues.md` with TODOs.

## 2. Current State & Work-in-Progress

*   **Files Created:**
    *   `ReasoningEngine/Core/NodeRoles.cs`
    *   `ReasoningEngine/Core/FunctionTypes.cs`
    *   `ReasoningEngine/Core/NodeFactory.cs`
    *   `ReasoningEngine/GraphFileHandling/IGraphStorageProvider.cs`
    *   `docs/KnowledgeRepresentationV3.md`
*   **Files Modified:**
    *   `ReasoningEngine/Core/EdgeBase.cs` (Added EdgeId, constructors)
    *   `ReasoningEngine/Core/Edge.cs` (Removed V1, updated constructors)
    *   `ReasoningEngine/Core/NodeBase.cs` (Removed NodeType, updated constructor)
    *   `ReasoningEngine/Core/Node.cs` (Removed V1/V2, updated V3, updated alias)
    *   `ReasoningEngine/GraphOperations/CommandProcessor.cs` (Refactored AddNode/EditNode to use NodeFactory with Dictionary payload)
    *   `ReasoningEngine/GraphFileHandling/GraphFileManager.cs` (Renamed to `FileGraphStorageProvider`, implements `IGraphStorageProvider`, updated LoadNode for V3, updated DebugWriter verbosity, started implementing interface methods)
    *   `ReasoningEngine/Utils/Scenarios/PopulateWeatherScenarioData.cs` (Updated node loading/distribution adding logic for V3 Node type)
    *   `ReasoningEngineTests/GraphFileHandingUnitTests.cs` (Updated Node constructor calls)
    *   `issues.md` (Added V3 TODOs)
    *   `Program.cs` (Added comment about verbosity)
    *   `ReasoningEngine/Utils/Scenarios/ScenarioManager.cs` (Added comment about verbosity, updated Populator constructor call)
*   **Files Deleted:**
    *   `ReasoningEngine/Core/SIMONode.cs`
    *   `ReasoningEngine/Core/MISONode.cs`
    *   `ReasoningEngineTests/NodeTypeTests.cs`
    *   `ReasoningEngine/DTOs/AddNodeRequest.cs`
*   **Current Task:** Refactoring `FileGraphStorageProvider` (formerly `GraphFileManager`) to fully implement the `IGraphStorageProvider` interface. We were in the middle of implementing the edge-related methods (`GetEdgeDataAsync`, `SaveEdgeDataAsync`, `DeleteEdgeDataAsync`, `GetOutgoingEdgeIdsAsync`, `GetIncomingEdgeIdsAsync`). The last attempt resulted in file corruption and was reverted.

## 3. Current TODO List (from issues.md)

1.  **`ProbabilityDistribution`: Sorted Inserts:** Modify `AddPoint` and `AddRange` to insert new elements while maintaining sorted order by `LowerBound`.
2.  **`ProbabilityDistribution`: Relax `AddRange` Validation:** Remove the "too close" check (`Math.Abs(lowerBound - d.UpperBound) < EPSILON || ...`). Keep significant overlap check.
3.  **`ProbabilityDistribution`: Deterministic `GetContainingRange`:** Ensure tie-breaking relies on the sorted list (as per point 1).
4.  **`ProbabilityDistribution`: Interpolation (Medium Term):** Consider modifying `GetProbability` for smoother boundary transitions.
5.  **`NodeFactory`: Parameter Parsing:** Implement robust parsing/conversion for `FunctionParams`.
6.  **`PopulateWeatherScenarioData`: Update to V3:** Modify script to use V3 payload format (Dictionary) for `add_node`.
7.  **`PopulateWeatherScenarioData`: Rerun:** Rerun scenario after V3 framework and ProbabilityDistribution fixes are complete.
8.  **Persistence:** Finish implementing `IGraphStorageProvider` in `FileGraphStorageProvider`. Adapt methods to handle `EdgeId` persistence/lookup. Create the `GraphObjectMapper` layer to handle serialization/deserialization using the storage provider.
9.  **`CommandProcessor`: `edit_node` Behavior:** Review `NodeFactory.UpdateNodeFromPayload` logic for state preservation.
10. **Build & Test:** Perform full build and update/run tests for V3.
