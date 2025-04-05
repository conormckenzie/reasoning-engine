using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json; // Assuming Newtonsoft.Json for serialization
using ReasoningEngine; // For Node, Edge etc.
using DebugUtils;

namespace ReasoningEngine.GraphFileHandling
{
    /// <summary>
    /// Handles the mapping between graph objects (Node, Edge) and their serialized representation 
    /// stored via an IGraphStorageProvider. It encapsulates serialization/deserialization logic.
    /// </summary>
    public class GraphObjectMapper
    {
        private readonly IGraphStorageProvider storageProvider;

        public GraphObjectMapper(IGraphStorageProvider storageProvider)
        {
            this.storageProvider = storageProvider ?? throw new ArgumentNullException(nameof(storageProvider));
        }

        // --- Node Operations ---

        public async Task<Node?> GetNodeAsync(long nodeId)
        {
            string? nodeData = await storageProvider.GetNodeDataAsync(nodeId);
            if (string.IsNullOrEmpty(nodeData))
            {
                return null;
            }

            try
            {
                // TODO: Implement robust deserialization, potentially checking a version/type field
                // For now, assuming direct deserialization to Node (which is NodeV3 alias)
                Node? node = JsonConvert.DeserializeObject<Node>(nodeData); 
                return node;
            }
            catch (JsonException ex)
            {
                DebugWriter.DebugWriteLine("#MAP_GETNODE_ERR#", $"Error deserializing node {nodeId}: {ex.Message}");
                return null; // Or throw custom exception
            }
            catch (Exception ex)
            {
                 DebugWriter.DebugWriteLine("#MAP_GETNODE_GEN_ERR#", $"Unexpected error getting node {nodeId}: {ex.Message}");
                 return null; // Or throw
            }
        }

        public async Task<bool> SaveNodeAsync(Node node)
        {
             if (node == null) throw new ArgumentNullException(nameof(node));

             try
             {
                 // TODO: Implement robust serialization
                 string nodeData = JsonConvert.SerializeObject(node, Formatting.Indented);
                 return await storageProvider.SaveNodeDataAsync(node.Id, nodeData);
             }
             catch (JsonException ex)
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
                         DebugWriter.DebugWriteLine("#MAP_DELNODE_EDGEDEL#", $"Deleted associated edge {edgeId} for node {nodeId}.", true, VerbosityLevel.Detailed);
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
            catch (Exception ex)
            {
                 DebugWriter.DebugWriteLine("#MAP_DELNODE_ERR#", $"Unexpected error deleting node {nodeId} and associated edges: {ex.Message}");
                 return false; // Or throw
            }
        }

        // --- Edge Operations ---

        public async Task<Edge?> GetEdgeAsync(Guid edgeId)
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
                // For now, assuming direct deserialization to Edge (which is EdgeV2 alias)
                Edge? edge = JsonConvert.DeserializeObject<Edge>(edgeData); 
                return edge;
            }
            catch (JsonException ex)
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
         
        public async Task<Edge?> GetEdgeAsync(long fromNodeId, long toNodeId)
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
                Edge? edge = JsonConvert.DeserializeObject<Edge>(edgeData); 
                return edge;
            }
            catch (JsonException ex)
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

        public async Task<List<Edge>> GetOutgoingEdgesAsync(long nodeId)
        {
            var edges = new List<Edge>();
            try
            {
                List<Guid> edgeIds = await storageProvider.GetOutgoingEdgeIdsAsync(nodeId);
                foreach (Guid edgeId in edgeIds)
                {
                    // Use the already implemented GetEdgeAsync(Guid)
                    Edge? edge = await GetEdgeAsync(edgeId); 
                    if (edge != null)
                    {
                        edges.Add(edge);
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

        public async Task<List<Edge>> GetIncomingEdgesAsync(long nodeId)
        {
            var edges = new List<Edge>();
            try
            {
                List<Guid> edgeIds = await storageProvider.GetIncomingEdgeIdsAsync(nodeId);
                foreach (Guid edgeId in edgeIds)
                {
                    // Use the already implemented GetEdgeAsync(Guid)
                    Edge? edge = await GetEdgeAsync(edgeId); 
                    if (edge != null)
                    {
                        edges.Add(edge);
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

        public async Task<bool> SaveEdgeAsync(Edge edge)
        {
            if (edge == null) throw new ArgumentNullException(nameof(edge));

            try
            {
                // TODO: Implement robust serialization, maybe wrap edge like node data?
                string edgeData = JsonConvert.SerializeObject(edge, Formatting.Indented);
                // Use the provider's method which handles saving both outgoing/incoming representations
                return await storageProvider.SaveEdgeDataAsync(edge.EdgeId, edge.FromNode, edge.ToNode, edgeData);
            }
            catch (JsonException ex)
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

        public async Task<List<long>> GetAllNodeIdsAsync()
        {
            return await storageProvider.GetAllNodeIdsAsync();
        }

        // Add other methods as needed (e.g., bulk operations)
    }
}
