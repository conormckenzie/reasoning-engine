# Efficient Graph I/O Operations Wishlist

This list outlines common graph data operations that are desirable to perform efficiently in a storage provider, particularly for a file-based system.

## Node Operations

1.  **Get Node by ID:** Retrieve node data given its `long` ID.
    *   *Efficiency Goal:* Direct access (e.g., path calculation based on ID).
2.  **Save/Update Node by ID:** Write/overwrite node data given its `long` ID.
    *   *Efficiency Goal:* Direct access.
3.  **Delete Node by ID:** Remove node data given its `long` ID.
    *   *Efficiency Goal:* Direct access.
4.  **Node Exists Check by ID:** Check existence given `long` ID.
    *   *Efficiency Goal:* Direct check (e.g., file existence).
5.  **Get All Node IDs:** Retrieve a list/iterator of all node IDs.
    *   *Efficiency Goal:* Read from a central index without scanning all node files.

## Edge Operations

6.  **Get Edge by ID (Guid):** Retrieve edge data given its unique `Guid` (`EdgeId`).
    *   *Efficiency Goal:* Direct access or efficient index lookup (without needing node IDs).
7.  **Save/Update Edge:** Write/overwrite edge data (identified by `EdgeId`, requires `sourceNodeId`, `destNodeId` for context/pathing).
    *   *Efficiency Goal:* Direct access or single write operation to a predictable location.
8.  **Delete Edge by ID (Guid):** Remove edge data given its `Guid`.
    *   *Efficiency Goal:* Direct access or efficient index lookup (without needing node IDs).
9.  **Get All Outgoing Edges for a Node:** Retrieve IDs or data for all edges originating from a given `sourceNodeId`.
    *   *Efficiency Goal:* Read from a node-specific index or directory listing without scanning unrelated nodes/edges.
10. **Get All Incoming Edges for a Node:** Retrieve IDs or data for all edges pointing to a given `destinationNodeId`.
    *   *Efficiency Goal:* Read from a node-specific index or dedicated incoming edge index without scanning unrelated nodes/edges.
11. **(Optional) Delete Edge by Nodes:** Remove edge(s) given `sourceNodeId` and `destNodeId`. Efficiency depends on whether multiple edges are allowed and how they are identified (e.g., requires Guid if multiple).

## Notes on Proposed Simplification

The proposed simplified edge storage structure (`edges/{source_hierarchy}/{edgeId_Guid}.json`) achieves efficiency for:

*   Node Operations (#1-5) - Unchanged.
*   Save/Update Edge (#7) - Efficient (single write to a predictable path).
*   Get All Outgoing Edges (#9) - Efficient (read source node's edge directory index/listing).
*   Get/Delete Edge by ID (#6, #8) - Efficient *if* the `sourceNodeId` is also known (allows direct path calculation). Inefficient if only the `Guid` is known (requires scanning all source node directories or a global Guid index).

The proposed simplification makes the following inefficient without additional indexing:

*   Get All Incoming Edges (#10) - Requires scanning all edge directories or a dedicated incoming edge index.
*   Get/Delete Edge by ID (#6, #8) - When *only* the `Guid` is known.
