# Reasoning Engine File Management System Documentation

## Table of Contents
1. [High-Level Overview](#1-high-level-overview)
2. [System Architecture](#2-system-architecture)
3. [Design Philosophy and Implementation Guidelines](#3-design-philosophy-and-implementation-guidelines)
4. [Detailed Procedures](#4-detailed-procedures)
5. [File and Directory Structure](#5-file-and-directory-structure)
6. [Performance Considerations](#6-performance-considerations)
7. [Future Improvements](#7-future-improvements)

## 1. High-Level Overview

The Reasoning Engine uses a file-based storage system to persist graph data. This system is designed to efficiently store and retrieve nodes and edges while maintaining the graph's structure and relationships. The file management system is built with scalability in mind, using a hierarchical directory structure to organize data and prevent performance degradation as the graph grows.

Key components of the file management system:
1. Node Storage
2. Edge Storage
3. Indexing System
4. File Paths Generation

## 2. System Architecture

### 2.1 Node Storage
Nodes are stored as individual JSON files in a hierarchical directory structure based on their IDs. This structure helps in efficiently locating and managing nodes, even as the graph grows to contain millions of nodes.

### 2.2 Edge Storage
Edges are stored bidirectionally, meaning each edge is represented twice in the file system:
1. As an outgoing edge from its source node
2. As an incoming edge to its destination node

This bidirectional storage allows for efficient querying of both incoming and outgoing edges for any given node.

### 2.3 Indexing System
An indexing system is used to keep track of nodes and edges. This system consists of:
1. A main index file for nodes (to be split into multiple index files in future development, to handle large numbers of nodes)
2. Multiple index files for edges, distributed across the directory structure

### 2.4 File Paths Generation
The system uses deterministic algorithms to generate file paths for nodes and edges based on their IDs. This ensures consistent and efficient access to data.

## 3. Design Philosophy and Implementation Guidelines

### 3.1 Core Principles

The file system structure is designed with the following principles in mind:
1. **Self-documentation**: Each directory in the path should contain enough information to identify its exact position in the hierarchy, even when viewed in isolation.
2. **Error resistance**: The redundancy in the path makes it immediately obvious if a file or directory is misplaced.
3. **Consistency**: The same logic is applied to both node and edge structures.

### 3.2 Implementation Guidelines

When generating paths, always use the full prefix of the ID for each directory level, not just the incremental part. This approach enhances the self-documenting nature of the file structure and improves error resistance.

### 3.3 Correct Implementation Examples

#### 3.3.1 Node Example
For a node with ID "0000000000000001":
```
Base Directory/0000/00000000/000000000000/0000000000000001.json
```

#### 3.3.2 Edge Example
For an outgoing edge from "0000000000000001" to "0000000000000002":
```
Base Directory/edges/outgoing/0000/00000000/000000000000/0000000000000001/0000000000000001-0000/0000000000000001-00000000/0000000000000001-000000000000/0000000000000001-0000000000000002.json
```

### 3.4 Common Incorrect Implementations

Implementers often make the following mistakes:

#### 3.4.1 Incorrect Node Structure
```
Base Directory/0000/0000/0000/0001/0000000000000001.json
```
This is incorrect because each directory should contain the cumulative prefix of the node ID, not just the next 4 digits.

#### 3.4.2 Incorrect Edge Structure
```
Base Directory/edges/outgoing/0000/0000/0000/0001/0000000000000001-0000/0000000000000001-0000/0000000000000001-0000/0000000000000001-0000000000000002.json
```
This is incorrect for two reasons:
a) The initial directories don't contain the cumulative prefix of the FromNodeID.
b) The latter directories don't properly build up the ToNodeID prefix.

Always ensure that each directory in the path contains the maximum available information about its position in the overall structure.

## 4. Detailed Procedures

### 4.1 Adding a Node
1. Generate the node's file path based on its ID (see [Section 5.1](#51-node-file-structure))
2. Create the necessary directory structure
3. Serialize the node data to JSON
4. Write the JSON data to the file
5. Update the main node index with the new node's information

### 4.2 Retrieving a Node
1. Generate the node's file path based on its ID
2. Check if the file exists
3. If it exists, read and deserialize the JSON data
4. Convert the deserialized data to the appropriate node object based on its version

### 4.3 Adding an Edge
1. Check if both the source and destination nodes exist
2. Generate file paths for both the outgoing and incoming representations of the edge (see [Section 5.2](#52-edge-file-structure))
3. Create the necessary directory structures
4. Serialize the edge data to JSON
5. Write the JSON data to both the outgoing and incoming edge files
6. Update the edge index files for both the outgoing and incoming directories
7. Update the edge counts for both the source and destination nodes in the main node index

### 4.4 Retrieving Edges
1. Generate the directory path for the node's edges (either outgoing or incoming)
2. Recursively search for index files in the directory structure
3. For each index file found, read the list of edge files
4. For each edge file, read and deserialize the JSON data
5. Convert the deserialized data to the appropriate edge object based on its version

### 4.5 Deleting an Edge
1. Generate file paths for both the outgoing and incoming representations of the edge
2. Delete both edge files if they exist
3. Update the edge index files for both the outgoing and incoming directories, removing references to the deleted edge files
4. Update the edge counts for both the source and destination nodes in the main node index

### 4.6 Deleting a Node
1. Generate the node's file path based on its ID
2. Delete the node file if it exists
3. Delete all outgoing edges associated with the node
    a. Generate the outgoing edges directory path
    b. Recursively delete all edge files and their corresponding incoming representations
    c. Update affected edge index files
4. Delete all incoming edges associated with the node
    a. Generate the incoming edges directory path
    b. Recursively delete all edge files and their corresponding outgoing representations
    c. Update affected edge index files
5. Remove the node from the main node index

## 5. File and Directory Structure

This section details the specific file and directory structures used for nodes and edges, implementing the design philosophy described in [Section 3](#3-design-philosophy-and-implementation-guidelines).

### 5.1 Node File Structure
```
Base Directory/
  - 0000/
    - 00000000/
      - 000000000000/
        - 0000000000000000.json
```

### 5.2 Edge File Structure

#### 5.2.1 Outgoing Edge File Structure
```
Base Directory/
  - edges/
    - outgoing/
      - 0000/ (first 4 digits of FromNodeID)
        - 00000000/ (first 8 digits of FromNodeID)
          - 000000000000/ (first 12 digits of FromNodeID)
            - 0000000000000000/ (full FromNodeID)
              - 0000000000000000-0000/ (FromNodeID-first 4 digits of ToNodeID)
                - 0000000000000000-00000000/ (FromNodeID-first 8 digits of ToNodeID)
                  - 0000000000000000-000000000000/ (FromNodeID-first 12 digits of ToNodeID)
                    - 0000000000000000-0000000000000000.json (FromNodeID-ToNodeID.json)
```

#### 5.2.2 Incoming Edge File Structure
```
Base Directory/
  - edges/
    - incoming/
      - 0000/ (first 4 digits of ToNodeID)
        - 00000000/ (first 8 digits of ToNodeID)
          - 000000000000/ (first 12 digits of ToNodeID)
            - 0000000000000000/ (full ToNodeID)
              - 0000-0000000000000000/ (first 4 digits of FromNodeID-ToNodeID)
                - 00000000-0000000000000000/ (first 8 digits of FromNodeID-ToNodeID)
                  - 000000000000-0000000000000000/ (first 12 digits of FromNodeID-ToNodeID)
                    - 0000000000000000-0000000000000000.json (FromNodeID-ToNodeID.json)
```

#### 5.2.3 Key Difference in Edge Structures
The key difference between outgoing and incoming edge structures is in the deeper levels of the directory structure:
- For outgoing edges, the FromNodeID comes first in the directory names.
- For incoming edges, the ToNodeID comes first in the directory names.

This structure allows for efficient retrieval of both outgoing and incoming edges for any given node.

### 5.3 Index File Structure
- Main node index: `index.json` in the base directory
- Edge index files: `index.json` in each subdirectory of the edge structure

## 6. Performance Considerations

- The hierarchical structure limits the number of files/directories in each directory, preventing filesystem limitations and performance degradation.
- Bidirectional edge storage allows for efficient querying of both incoming and outgoing edges.
- Distributed index files prevent any single index file from becoming too large and slow to process.
- The deterministic file path generation allows for direct access to files without needing to search through the directory structure.

## 7. Future Improvements

- Implement caching mechanisms to reduce disk I/O for frequently accessed nodes and edges.
- Consider using a database for indexing instead of file-based indexes for improved performance with very large graphs.
- Implement compression techniques for edge and node data to reduce disk usage.
- Develop a system for managing graph versions or snapshots.