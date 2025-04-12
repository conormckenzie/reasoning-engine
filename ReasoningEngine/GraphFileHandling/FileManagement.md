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
- **Indexing:** Relies on directory structure, file existence checks, and two types of index files:
    - **Main Index (`index.json` in base directory):** Managed by `IndexManager`. Stores a list of all known node IDs and their file paths (`NodeInfo`). Used by `FileGraphStorageProvider` for `GetAllNodeIdsAsync`. The stored `EdgeCount` per node is currently not reliably maintained or used effectively.
    - **Edge Directory Indexes (`index.json` within edge hierarchy):** Managed directly by `FileGraphStorageProvider`. Each `index.json` lists the edge filenames (`{sourceId}-{destId}.json` or `{destId}-{sourceId}.json`) present in that specific directory. Used by `GetAllEdgeFiles` (and thus `GetOutgoing/IncomingEdgeIdsAsync`) to avoid scanning all files when loading edges by node ID.
- **Limitations:** No index exists for efficient `EdgeId` (Guid) lookups.

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

### 5.2 Edge File Structure (Implemented)

The `FileGraphStorageProvider` implements a hierarchical, bidirectional structure for storing edge data:

```
Base Directory/
  - edges/
    - outgoing/
      - {sourceNodeId_part1}/
        - {sourceNodeId_part1}{sourceNodeId_part2}/
          - {sourceNodeId_part1}{sourceNodeId_part2}{sourceNodeId_part3}/
            - {sourceNodeId_full}/
              - {sourceNodeId_full}-{destNodeId_part1}/
                - {sourceNodeId_full}-{destNodeId_part1}{destNodeId_part2}/
                  - {sourceNodeId_full}-{destNodeId_part1}{destNodeId_part2}{destNodeId_part3}/
                    - {sourceNodeId_full}-{destNodeId_full}.json
                    - index.json  // Tracks edge files in this directory
    - incoming/
      - {destNodeId_part1}/
        - {destNodeId_part1}{destNodeId_part2}/
          - {destNodeId_part1}{destNodeId_part2}{destNodeId_part3}/
            - {destNodeId_full}/
              - {destNodeId_full}-{sourceNodeId_part1}/
                - {destNodeId_full}-{sourceNodeId_part1}{sourceNodeId_part2}/
                  - {destNodeId_full}-{sourceNodeId_part1}{sourceNodeId_part2}{sourceNodeId_part3}/
                    - {destNodeId_full}-{sourceNodeId_full}.json // Note: Filename uses dest-source order here
                    - index.json // Tracks edge files in this directory
```
*   **Edge Storage:** Edges are stored twice: once under the source node's hierarchy in `edges/outgoing/` and once under the destination node's hierarchy in `edges/incoming/`.
*   **Filename Convention:** The filename uses the format `{sourceNodeId_full}-{destNodeId_full}.json` in the `outgoing` path and `{destNodeId_full}-{sourceNodeId_full}.json` in the `incoming` path. The edge's unique `Guid` (`EdgeId`) is stored *within* the JSON file content.
*   **Directory Structure:** Both `outgoing` and `incoming` paths use a deep hierarchy based on both the primary node ID (source for outgoing, destination for incoming) and the secondary node ID (destination for outgoing, source for incoming) to distribute files.
*   **Edge Index Files (`index.json`):** Each directory potentially containing edge files also contains an `index.json` file listing the edge filenames within that specific directory. This is used by `GetAllEdgeFiles` to avoid scanning every file during edge loading by node ID.
*   **Guid Lookups:** Retrieving/deleting an edge solely by its `Guid` still requires an inefficient scan across the directory structure, as there is no index mapping Guids to file paths.

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
