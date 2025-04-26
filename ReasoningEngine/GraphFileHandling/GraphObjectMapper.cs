using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Json; // Using System.Text.Json for serialization
using ReasoningEngine; // For Node, Edge etc.
using DebugUtils;
using System.Linq; // Added for Where, Contains

namespace ReasoningEngine.GraphFileHandling
{
    /// <summary>
    /// Handles the mapping between graph objects (Node, Edge) and their serialized representation
    /// stored via an IGraphStorageProvider. It encapsulates serialization/deserialization logic.
    /// </summary>
    public class GraphObjectMapper
    {
        private readonly IGraphStorageProvider storageProvider;

        /// <summary>
        /// Initializes a new instance of the GraphObjectMapper.
        /// </summary>
        /// <param name="storageProvider">The storage provider to use for raw data operations.</param>
        /// <exception cref="ArgumentNullException">Thrown if the storageProvider is null.</exception>
        public GraphObjectMapper(IGraphStorageProvider storageProvider)
        {
            this.storageProvider = storageProvider ?? throw new ArgumentNullException(nameof(storageProvider));
        }

        // --- Node Operations ---

        /// <summary>
        /// Asynchronously retrieves a Node object by its ID.
        /// </summary>
        /// <param name="nodeId">The ID of the node to retrieve.</param>
        /// <returns>A Task containing the NodeV3 object if found, otherwise null.</returns>
        public async Task<NodeV3?> GetNodeAsync(long nodeId) // Changed return type to NodeV3?
        {
            string? nodeData = await storageProvider.GetNodeDataAsync(nodeId);
            if (string.IsNullOrEmpty(nodeData))
            {
                return null;
            }

            try
            {
                // Log the JSON being read before deserialization
                DebugWriter.DebugWriteLine("#MAP_GETNODE_JSON#", $"Reading Node {nodeId} JSON:\n{nodeData}"); // Removed incorrect verbosityLevel
                // TODO: Implement robust deserialization, potentially checking a version/type field
                // Deserialize directly into NodeV3, which has the JsonConstructor attribute
                NodeV3? node = JsonSerializer.Deserialize<NodeV3>(nodeData);
                return node; // Return type is Node? which is compatible with NodeV3?
            }
            catch (System.Text.Json.JsonException ex)
            {
                DebugWriter.DebugWriteLine("#MAP_GETNODE_ERR#", $"Error deserializing node {nodeId}: {ex.Message}");
                return null; // Or throw custom exception
            }
            // Removed duplicate generic catch block
            catch (Exception ex)
            {
                 DebugWriter.DebugWriteLine("#MAP_GETNODE_GEN_ERR#", $"Unexpected error getting node {nodeId}: {ex.Message}");
                 return null; // Or throw
             }
        }

        /// <summary>
        /// Asynchronously saves or updates a Node object in the storage.
        /// </summary>
        /// <param name="node">The NodeV3 object to save.</param>
        /// <returns>A Task containing true if the operation was successful, false otherwise.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the node is null.</exception>
        public async Task<bool> SaveNodeAsync(NodeV3 node) // Changed parameter type to NodeV3
        {
             if (node == null) throw new ArgumentNullException(nameof(node));

             try
             {
                 // TODO: Implement robust serialization
                 var options = new JsonSerializerOptions { WriteIndented = true };
                 string nodeData = JsonSerializer.Serialize(node, options);
                 // Log the JSON being saved (Removed incorrect verbosityLevel parameter)
                 DebugWriter.DebugWriteLine("#MAP_SAVNODE_JSON#", $"Saving Node {node.Id} JSON:\n{nodeData}");
                 return await storageProvider.SaveNodeDataAsync(node.Id, nodeData);
             }
             catch (System.Text.Json.JsonException ex)
             {
                 DebugWriter.DebugWriteLine("#MAP_SAVNODE_ERR#", $"Error serializing node {node.Id}: {ex.Message}");
                 return false; // Or throw custom exception
             }
             catch (Exception ex)
             {
                  DebugWriter.DebugWriteLine("#MAP_SAVNODE_GEN_ERR#", $"Unexpected error saving node {node.Id}: {ex.Message}");
                  return false; // Or throw
             }
        }

        /// <summary>
        /// Asynchronously deletes a Node object and its associated edges from the storage.
        /// </summary>
        /// <param name="nodeId">The ID of the node to delete.</param>
        /// <returns>A Task containing true if the node deletion was successful (even if some edge deletions failed), false if the node deletion itself failed.</returns>
        public async Task<bool> DeleteNodeAsync(long nodeId)
        {
            bool success = true;
            List<Guid> edgeIdsToDelete = new List<Guid>();

            try
            {
                // Get all edge IDs associated with the node
                var outgoingEdgeIds = await storageProvider.GetOutgoingEdgeIdsAsync(nodeId);
                var incomingEdgeIds = await storageProvider.GetIncomingEdgeIdsAsync(nodeId);
                edgeIdsToDelete.AddRange(outgoingEdgeIds);
                // Add incoming only if not already present from outgoing (avoid double delete attempts)
                edgeIdsToDelete.AddRange(incomingEdgeIds.Where(id => !edgeIdsToDelete.Contains(id)));

                DebugWriter.DebugWriteLine("#MAP_DELNODE_EDGES#", $"Found {edgeIdsToDelete.Count} unique edges associated with node {nodeId} for deletion.", true, VerbosityLevel.Detailed);

                // Attempt to delete each associated edge
                // Note: This relies on the provider's DeleteEdgeDataAsync(Guid) which might be a stub.
                foreach (var edgeId in edgeIdsToDelete)
                {
                    bool edgeDeleted = await storageProvider.DeleteEdgeDataAsync(edgeId);
                    if (!edgeDeleted)
                    {
                        // Log warning but continue trying to delete others and the node
                        DebugWriter.DebugWriteLine("#MAP_DELNODE_EDGEFAIL#", $"Failed to delete associated edge {edgeId} for node {nodeId}. Provider returned false.", true, VerbosityLevel.Minimal);
                        success = false; // Mark overall operation as potentially incomplete
                    } else {
                         DebugWriter.DebugWriteLine("#MAP_DELNODE_EDGEDEL#", $"Deleted associated edge {edgeId} for node {nodeId}.", true, VerbosityLevel.Detailed); // Corrected VerbosityLevel access
                    }
                }

                // Finally, delete the node data itself
                bool nodeDeleted = await storageProvider.DeleteNodeDataAsync(nodeId);
                if (!nodeDeleted) {
                    DebugWriter.DebugWriteLine("#MAP_DELNODE_NODEFAIL#", $"Provider failed to delete node data for {nodeId} after attempting edge deletion.", true, VerbosityLevel.Minimal);
                    success = false; // Node deletion failed
                }
                 return success && nodeDeleted; // Return true only if node deletion itself succeeded
            }
            catch (Exception ex) // Catch block for the outer try in DeleteNodeAsync
            {
                 DebugWriter.DebugWriteLine("#MAP_DELNODE_ERR#", $"Unexpected error deleting node {nodeId} and associated edges: {ex.Message}");
                 return false; // Or throw
            }
        }

        // --- Edge Operations ---

        /// <summary>
        /// Asynchronously retrieves an Edge object by its unique identifier (Guid).
        /// </summary>
        /// <param name="edgeId">The unique identifier (Guid) of the edge to retrieve.</param>
        /// <returns>A Task containing the EdgeV2 object if found, otherwise null.</returns>
        public async Task<EdgeV2?> GetEdgeAsync(Guid edgeId) // Changed return type
        {
            string? edgeData = await storageProvider.GetEdgeDataAsync(edgeId);
            if (string.IsNullOrEmpty(edgeData))
            {
                // The provider might return null if not found, or the underlying file provider stub returns null.
                DebugWriter.DebugWriteLine("#MAP_GETEDGE_GUID_NF#", $"Edge data not found for Guid {edgeId}.");
                return null;
            }

            try
            {
                // TODO: Implement robust deserialization, potentially checking a version/type field
                // Deserialize directly into EdgeV2, which has the correct [JsonConstructor]
                EdgeV2? edge = JsonSerializer.Deserialize<EdgeV2>(edgeData);
                return edge; // Return type changed to EdgeV2?
            }
            catch (System.Text.Json.JsonException ex)
            {
                DebugWriter.DebugWriteLine("#MAP_GETEDGE_GUID_ERR#", $"Error deserializing edge {edgeId}: {ex.Message}");
                return null; // Or throw custom exception
            }
            catch (Exception ex)
            {
                 DebugWriter.DebugWriteLine("#MAP_GETEDGE_GUID_GEN_ERR#", $"Unexpected error getting edge {edgeId}: {ex.Message}");
                 return null; // Or throw
            }
        }
         
        /// <summary>
        /// Asynchronously retrieves an Edge object by its source and destination node IDs.
        /// </summary>
        /// <param name="fromNodeId">The ID of the source node.</param>
        /// <param name="toNodeId">The ID of the destination node.</param>
        /// <returns>A Task containing the EdgeV2 object if found, otherwise null.</returns>
        public async Task<EdgeV2?> GetEdgeAsync(long fromNodeId, long toNodeId) // Changed return type
        {
            string? edgeData = await storageProvider.GetEdgeDataAsync(fromNodeId, toNodeId);
            if (string.IsNullOrEmpty(edgeData))
            {
                DebugWriter.DebugWriteLine("#MAP_GETEDGE_FROMTO_NF#", $"Edge data not found for {fromNodeId}->{toNodeId}.");
                return null;
            }

            try
            {
                // TODO: Implement robust deserialization
                // Deserialize directly into EdgeV2, which has the correct [JsonConstructor]
                EdgeV2? edge = JsonSerializer.Deserialize<EdgeV2>(edgeData);
                return edge; // Return type changed to EdgeV2?
            }
            catch (System.Text.Json.JsonException ex)
            {
                DebugWriter.DebugWriteLine("#MAP_GETEDGE_FROMTO_ERR#", $"Error deserializing edge {fromNodeId}->{toNodeId}: {ex.Message}");
                return null; // Or throw custom exception
            }
            catch (Exception ex)
            {
                 DebugWriter.DebugWriteLine("#MAP_GETEDGE_FROMTO_GEN_ERR#", $"Unexpected error getting edge {fromNodeId}->{toNodeId}: {ex.Message}");
                 return null; // Or throw
            }
        }

        /// <summary>
        /// Asynchronously retrieves a list of all outgoing Edge objects from a specific node.
        /// </summary>
        /// <param name="nodeId">The ID of the source node.</param>
        /// <returns>A Task containing a list of outgoing EdgeV2 objects.</returns>
        public async Task<List<EdgeV2>> GetOutgoingEdgesAsync(long nodeId) // Changed list type
        {
            var edges = new List<EdgeV2>(); // Changed list type
            try
            {
                List<Guid> edgeIds = await storageProvider.GetOutgoingEdgeIdsAsync(nodeId);
                foreach (Guid edgeId in edgeIds)
                {
                    // Use the already implemented GetEdgeAsync(Guid)
                    EdgeV2? edge = await GetEdgeAsync(edgeId); // Changed variable type
                    if (edge != null)
                    {
                        edges.Add(edge); // Add EdgeV2 directly
                    }
                    else
                    {
                        // Log if an edge ID from the list couldn't be fetched (data inconsistency?)
                        DebugWriter.DebugWriteLine("#MAP_GETOUTEDGE_WARN#", $"Could not retrieve edge data for outgoing edge ID {edgeId} listed for node {nodeId}.");
                    }
                }
            }
            catch (Exception ex)
            {
                 DebugWriter.DebugWriteLine("#MAP_GETOUTEDGE_ERR#", $"Unexpected error getting outgoing edges for node {nodeId}: {ex.Message}");
                 // Return potentially partial list or empty list depending on desired error handling
            }
            return edges;
        }

        /// <summary>
        /// Asynchronously retrieves a list of all incoming Edge objects to a specific node.
        /// </summary>
        /// <param name="nodeId">The ID of the destination node.</param>
        /// <returns>A Task containing a list of incoming EdgeV2 objects.</returns>
        public async Task<List<EdgeV2>> GetIncomingEdgesAsync(long nodeId) // Changed list type
        {
            var edges = new List<EdgeV2>(); // Changed list type
            try
            {
                List<Guid> edgeIds = await storageProvider.GetIncomingEdgeIdsAsync(nodeId);
                foreach (Guid edgeId in edgeIds)
                {
                    // Use the already implemented GetEdgeAsync(Guid)
                    EdgeV2? edge = await GetEdgeAsync(edgeId); // Changed variable type
                    if (edge != null)
                    {
                        edges.Add(edge); // Add EdgeV2 directly
                    }
                    else
                    {
                        // Log if an edge ID from the list couldn't be fetched (data inconsistency?)
                        DebugWriter.DebugWriteLine("#MAP_GETINEDGE_WARN#", $"Could not retrieve edge data for incoming edge ID {edgeId} listed for node {nodeId}.");
                    }
                }
            }
            catch (Exception ex)
            {
                 DebugWriter.DebugWriteLine("#MAP_GETINEDGE_ERR#", $"Unexpected error getting incoming edges for node {nodeId}: {ex.Message}");
                 // Return potentially partial list or empty list depending on desired error handling
            }
            return edges;
        }

        /// <summary>
        /// Asynchronously saves or updates an Edge object in the storage.
        /// Performs a check to ensure both source and destination nodes exist before saving.
        /// </summary>
        /// <param name="edge">The EdgeV2 object to save.</param>
        /// <returns>A Task containing true if the operation was successful, false otherwise (e.g., if a node does not exist).</returns>
        /// <exception cref="ArgumentNullException">Thrown if the edge is null.</exception>
        public async Task<bool> SaveEdgeAsync(EdgeV2 edge) // Changed parameter type to EdgeV2
        {
            if (edge == null) throw new ArgumentNullException(nameof(edge));

            try
            {
                // *** START: Add node existence check ***
                // Check if both source and destination nodes exist before attempting to save the edge
                var fromNodeExists = await storageProvider.GetNodeDataAsync(edge.FromNode) != null;
                var toNodeExists = await storageProvider.GetNodeDataAsync(edge.ToNode) != null;

                if (!fromNodeExists || !toNodeExists)
                {
                    string missingNode = !fromNodeExists ? $"Source node {edge.FromNode}" : $"Destination node {edge.ToNode}";
                    DebugWriter.DebugWriteLine("#MAP_SAVEDGE_NODE_NF#", $"Cannot save edge {edge.EdgeId}: {missingNode} does not exist.", true, VerbosityLevel.Minimal);
                    return false; // Indicate failure due to missing node
                }
                // *** END: Add node existence check ***

                // TODO: Implement robust serialization, maybe wrap edge like node data?
                var options = new JsonSerializerOptions { WriteIndented = true };
                string edgeData = JsonSerializer.Serialize(edge, options);
                // Log the JSON being passed to the provider
                DebugWriter.DebugWriteLine("#MAP_SAVEDGE_JSON#", $"Saving Edge {edge.EdgeId} ({edge.FromNode}->{edge.ToNode}) JSON:\n{edgeData}");
                // Use the provider's method which handles saving both outgoing/incoming representations
                return await storageProvider.SaveEdgeDataAsync(edge.EdgeId, edge.FromNode, edge.ToNode, edgeData);
            }
            catch (System.Text.Json.JsonException ex)
            {
                DebugWriter.DebugWriteLine("#MAP_SAVEDGE_ERR#", $"Error serializing edge {edge.EdgeId} ({edge.FromNode}->{edge.ToNode}): {ex.Message}");
                return false; // Or throw custom exception
            }
            catch (Exception ex)
            {
                 DebugWriter.DebugWriteLine("#MAP_SAVEDGE_GEN_ERR#", $"Unexpected error saving edge {edge.EdgeId} ({edge.FromNode}->{edge.ToNode}): {ex.Message}");
                 return false; // Or throw
            }
        }

        /// <summary>
        /// Asynchronously deletes an Edge object by its unique identifier (Guid).
        /// </summary>
        /// <param name="edgeId">The unique identifier (Guid) of the edge to delete.</param>
        /// <returns>A Task containing true if the operation was successful, false otherwise.</returns>
        public async Task<bool> DeleteEdgeAsync(Guid edgeId)
        {
            // Directly call the provider's method for deletion by Guid
            // Note: The FileGraphStorageProvider currently has a stub for this.
            try
            {
                return await storageProvider.DeleteEdgeDataAsync(edgeId);
            }
            catch (Exception ex)
            {
                 DebugWriter.DebugWriteLine("#MAP_DELEDGE_GUID_ERR#", $"Unexpected error deleting edge {edgeId}: {ex.Message}");
                 return false; // Or throw
            }
        }
        
        /// <summary>
        /// Asynchronously deletes an Edge object by its source and destination node IDs.
        /// </summary>
        /// <param name="fromNodeId">The ID of the source node.</param>
        /// <param name="toNodeId">The ID of the destination node.</param>
        /// <returns>A Task containing true if the operation was successful, false otherwise.</returns>
        public async Task<bool> DeleteEdgeAsync(long fromNodeId, long toNodeId)
        {
            // Directly call the provider's method for deletion by node IDs
            try
            {
                return await storageProvider.DeleteEdgeDataAsync(fromNodeId, toNodeId);
            }
            catch (Exception ex)
            {
                 DebugWriter.DebugWriteLine("#MAP_DELEDGE_FROMTO_ERR#", $"Unexpected error deleting edge {fromNodeId}->{toNodeId}: {ex.Message}");
                 return false; // Or throw
            }
        }

        // --- Utility / Other ---

        /// <summary>
        /// Asynchronously retrieves a list of all node IDs from the storage using the underlying storage provider.
        /// </summary>
        /// <returns>A Task containing a list of all node IDs.</returns>
        public async Task<List<long>> GetAllNodeIdsAsync()
        {
            return await storageProvider.GetAllNodeIdsAsync();
        }

        // Add other methods as needed (e.g., bulk operations)
    }
}
