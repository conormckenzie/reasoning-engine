# Reasoning Engine Persistence Layer Documentation (V3 Architecture)

## Table of Contents
1. [High-Level Overview](#1-high-level-overview)
2. [System Architecture](#2-system-architecture)
3. [File Path Generation Philosophy](#3-file-path-generation-philosophy)
4. [Interaction Model](#4-interaction-model)
5. [File and Directory Structure (FileGraphStorageProvider)](#5-file-and-directory-structure-filegraphstorageprovider)
6. [Performance Considerations](#6-performance-considerations)
7. [Future Improvements](#7-future-improvements)

## 1. High-Level Overview

The Reasoning Engine uses a decoupled persistence layer to store and retrieve graph data (Nodes and Edges). This system is designed to be flexible, allowing different storage backends while maintaining a consistent interface for the rest of the application. The default implementation uses a file-based storage system.

Key components of the persistence layer:
1. **`IGraphStorageProvider` Interface:** Defines the contract for raw data storage operations (CRUD for node/edge data).
2. **`FileGraphStorageProvider` (in `GraphFileManager.cs`):** The default implementation of `IGraphStorageProvider`, using a hierarchical file system structure to store node and edge data as JSON files.
3. **`GraphObjectMapper`:** Acts as a layer between the application logic (e.g., `CommandProcessor`) and the storage provider. It handles object serialization/deserialization, mapping between domain objects (`Node`, `Edge`) and raw data, and orchestrates calls to the `IGraphStorageProvider`.

## 2. System Architecture

### 2.1 `IGraphStorageProvider` Interface
This interface (`ReasoningEngine/GraphFileHandling/IGraphStorageProvider.cs`) defines methods for basic data operations like:
- `GetNodeDataAsync(long nodeId)`
- `SaveNodeDataAsync(long nodeId, string data)`
- `DeleteNodeDataAsync(long nodeId)`
- `NodeExistsAsync(long nodeId)`
- `GetEdgeDataAsync(Guid edgeId)`
- `SaveEdgeDataAsync(Guid edgeId, string data)`
- `DeleteEdgeDataAsync(Guid edgeId)`
- `GetOutgoingEdgeIdsAsync(long fromNodeId)`
- `GetIncomingEdgeIdsAsync(long toNodeId)`
- `GetAllNodeIdsAsync()`

Implementations of this interface handle the specifics of the storage mechanism (e.g., file system, database).

### 2.2 `FileGraphStorageProvider` Implementation
Located in `ReasoningEngine/GraphFileHandling/GraphFileManager.cs`, this class implements `IGraphStorageProvider` using the local file system.
- **Node Storage:** Stores individual nodes as JSON files in a hierarchical directory structure based on their IDs (see Section 5).
- **Edge Storage:** Stores individual edges as JSON files, potentially using a bidirectional structure (see Section 5). Relies on scanning directories for retrieving edge lists by node ID and currently uses inefficient scanning for Guid-based lookups.
- **Indexing:** Currently relies on directory structure and file existence checks. A more robust indexing mechanism (e.g., for Guid lookups) is a potential future improvement.

### 2.3 `GraphObjectMapper`
Located in `ReasoningEngine/GraphFileHandling/GraphObjectMapper.cs`, this class uses an instance of `IGraphStorageProvider` to perform higher-level operations:
- Takes domain objects (`Node`, `Edge`) as input.
- Serializes objects to JSON strings.
- Calls the appropriate `IGraphStorageProvider` methods to save/delete raw data.
- Retrieves raw data using `IGraphStorageProvider`.
- Deserializes JSON strings back into domain objects.
- Handles logic like ensuring nodes exist before adding edges, preserving `EdgeId` during edge updates, and deleting associated edges when deleting a node.

## 3. File Path Generation Philosophy (FileGraphStorageProvider)

The `FileGraphStorageProvider` uses a specific file system structure designed with the following principles in mind:

1.  **Self-documentation**: Each directory in the path should contain enough information to identify its exact position in the hierarchy, even when viewed in isolation. This uses cumulative prefixes of IDs.
2.  **Error resistance**: The redundancy in the path makes it immediately obvious if a file or directory is misplaced.
3.  **Consistency**: The same logic is applied to both node and edge structures (where applicable).

### 3.1 Implementation Guidelines

When generating paths, the `FileGraphStorageProvider` should always use the full prefix of the ID for each directory level, not just the incremental part. This approach enhances the self-documenting nature of the file structure and improves error resistance.

*(See Section 5 for specific structure examples)*

## 4. Interaction Model

Application components (like `CommandProcessor` or `ScenarioManager`) interact with the persistence layer primarily through the `GraphObjectMapper`.

1.  **Adding/Updating Nodes/Edges:**
    *   The application creates or modifies a `Node` or `Edge` object.
    *   It calls a method on `GraphObjectMapper` (e.g., `SaveNodeAsync`, `SaveEdgeAsync`).
    *   `GraphObjectMapper` serializes the object to JSON.
    *   `GraphObjectMapper` calls the corresponding method on the injected `IGraphStorageProvider` (e.g., `SaveNodeDataAsync`, `SaveEdgeDataAsync`) with the ID and JSON data.
    *   The `IGraphStorageProvider` implementation handles writing the data to the underlying storage (e.g., creating/updating files). Note: Edge updates preserve the original `EdgeId`.
2.  **Retrieving Nodes/Edges:**
    *   The application requests a node or edge via `GraphObjectMapper` (e.g., `GetNodeAsync`, `GetEdgeAsync`, `GetOutgoingEdgesAsync`).
    *   `GraphObjectMapper` calls the appropriate method(s) on `IGraphStorageProvider` to fetch the raw data (e.g., `GetNodeDataAsync`, `GetOutgoingEdgeIdsAsync` followed by `GetEdgeDataAsync`).
    *   The `IGraphStorageProvider` retrieves the data from storage.
    *   `GraphObjectMapper` deserializes the raw JSON data into `Node` or `Edge` objects and returns them.
3.  **Deleting Nodes/Edges:**
    *   The application calls a deletion method on `GraphObjectMapper` (e.g., `DeleteNodeAsync`, `DeleteEdgeAsync`).
    *   `GraphObjectMapper` may perform related actions (like finding associated edges when deleting a node).
    *   `GraphObjectMapper` calls the corresponding deletion method(s) on `IGraphStorageProvider` (e.g., `DeleteNodeDataAsync`, `DeleteEdgeDataAsync`).
    *   The `IGraphStorageProvider` removes the data from storage.

## 5. File and Directory Structure (FileGraphStorageProvider)

This section details the specific file and directory structures used by the default `FileGraphStorageProvider` implementation, following the philosophy described in [Section 3](#3-file-path-generation-philosophy).

*(Note: This reflects the intended design; the actual implementation in GraphFileManager.cs should be verified against this.)*

### 5.1 Node File Structure
```
Base Directory/
  - {nodeId_part1}/
    - {nodeId_part1}{nodeId_part2}/
      - {nodeId_part1}{nodeId_part2}{nodeId_part3}/
        - {nodeId_full}.json
```
*Example for Node ID "1234567890123456":*
```
Base Directory/
  - 1234/
    - 12345678/
      - 123456789012/
        - 1234567890123456.json
```

### 5.2 Edge File Structure

The original design specified a complex bidirectional structure. The current `FileGraphStorageProvider` implementation might store edges differently or only partially implement this. A simplified view might be:

```
Base Directory/
  - edges/
    - {edgeId}.json
```
Or potentially organized by node (reflecting current implementation):
```
Base Directory/
  - nodes/
    - {sourceNodeId_part1}/
      - {sourceNodeId_part1}{sourceNodeId_part2}/
        - {sourceNodeId_part1}{sourceNodeId_part2}{sourceNodeId_part3}/
          - {sourceNodeId_full}/
            - edges/
              - outgoing/
                - {edgeId}.json  // EdgeId is the Guid
  - nodes/
    - {destNodeId_part1}/
      - {destNodeId_part1}{destNodeId_part2}/
        - {destNodeId_part1}{destNodeId_part2}{destNodeId_part3}/
          - {destNodeId_full}/
            - edges/
              - incoming/
                - {edgeId}.json  // EdgeId is the Guid
```
*   **Edge Storage:** Edges are stored twice: once under the source node's `outgoing` directory and once under the destination node's `incoming` directory. The filename in both locations is the edge's unique `Guid` (`EdgeId`). This allows efficient retrieval of outgoing/incoming edges by scanning the respective directories under a node.
*   **Guid Lookups:** Retrieving/deleting an edge solely by its `Guid` still requires an inefficient scan across node directories.

## 6. Performance Considerations

- **Hierarchical Structure:** Limits the number of files/directories per level, potentially avoiding filesystem performance issues with very large numbers of nodes/edges.
- **Direct Access:** Deterministic file path generation (for nodes, at least) allows direct access without scanning, assuming the path structure is known.
- **Edge Retrieval:** Retrieving edges by Node ID (`GetOutgoingEdgeIdsAsync`, `GetIncomingEdgeIdsAsync`) currently relies on directory scanning in `FileGraphStorageProvider`, which can be inefficient for nodes with many edges.
- **Guid Lookups:** Retrieving or deleting edges by `Guid` (`GetEdgeDataAsync(Guid)`, `DeleteEdgeDataAsync(Guid)`) is currently highly inefficient in `FileGraphStorageProvider`, requiring a full scan.
- **Serialization Overhead:** JSON serialization/deserialization adds overhead compared to binary formats.

## 7. Future Improvements

- Implement efficient indexing for edge retrieval by Node ID and Edge Guid within `FileGraphStorageProvider`.
- Consider alternative `IGraphStorageProvider` implementations (e.g., using a database like SQLite, PostgreSQL, or a dedicated graph database).
- Implement caching mechanisms in `GraphObjectMapper` or at the application level to reduce redundant storage access.
- Implement compression for JSON data to reduce disk usage.
- Develop a system for managing graph versions or snapshots at the storage level.
