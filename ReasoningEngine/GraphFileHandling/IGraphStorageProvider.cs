using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ReasoningEngine.GraphFileHandling
{
    /// <summary>
    /// Interface defining the contract for raw graph data storage and retrieval.
    /// This abstraction separates the mechanism of storing/retrieving raw node/edge data 
    /// (e.g., file system, database) from the logic of serializing/deserializing 
    /// C# graph objects (which will be handled by a separate GraphObjectMapper class).
    /// Implementations handle the specific storage medium (e.g., files, database).
    /// This interface deals with raw serialized data (strings) and identifiers (long, Guid).
    /// </summary>
    public interface IGraphStorageProvider
    {
        /// <summary>
        /// Retrieves the raw serialized data for a specific node.
        /// </summary>
        /// <param name="nodeId">The ID of the node.</param>
        /// <returns>A task representing the asynchronous operation, containing the raw node data string or null if not found.</returns>
        Task<string?> GetNodeDataAsync(long nodeId);

        /// <summary>
        /// Saves the raw serialized data for a node.
        /// </summary>
        /// <param name="nodeId">The ID of the node.</param>
        /// <param name="nodeData">The raw serialized node data (e.g., JSON string).</param>
        /// <returns>A task representing the asynchronous save operation. Returns true on success, false otherwise.</returns>
        Task<bool> SaveNodeDataAsync(long nodeId, string nodeData);

        /// <summary>
        /// Deletes the data for a specific node.
        /// </summary>
        /// <param name="nodeId">The ID of the node to delete.</param>
        /// <returns>A task representing the asynchronous delete operation. Returns true on success, false otherwise.</returns>
        Task<bool> DeleteNodeDataAsync(long nodeId);

        /// <summary>
        /// Retrieves the raw serialized data for a specific edge using its unique ID.
        /// </summary>
        /// <param name="edgeId">The unique ID of the edge.</param>
        /// <returns>A task representing the asynchronous operation, containing the raw edge data string or null if not found.</returns>
        Task<string?> GetEdgeDataAsync(Guid edgeId);
        
        /// <summary>
        /// Retrieves the raw serialized data for a specific edge using its connecting nodes.
        /// Note: Implementations might need indexing to support this efficiently without EdgeId.
        /// </summary>
        /// <param name="fromNodeId">The ID of the source node.</param>
        /// <param name="toNodeId">The ID of the target node.</param>
        /// <returns>A task representing the asynchronous operation, containing the raw edge data string or null if not found.</returns>
        Task<string?> GetEdgeDataAsync(long fromNodeId, long toNodeId); // Overload for convenience? Or rely on GetEdgeIds?

        /// <summary>
        /// Saves the raw serialized data for an edge.
        /// </summary>
        /// <param name="edgeId">The unique ID of the edge.</param>
        /// <param name="fromNodeId">The ID of the source node.</param>
        /// <param name="toNodeId">The ID of the target node.</param>
        /// <param name="edgeData">The raw serialized edge data (e.g., JSON string).</param>
        /// <returns>A task representing the asynchronous save operation. Returns true on success, false otherwise.</returns>
        Task<bool> SaveEdgeDataAsync(Guid edgeId, long fromNodeId, long toNodeId, string edgeData);

        /// <summary>
        /// Deletes the data for a specific edge using its unique ID.
        /// </summary>
        /// <param name="edgeId">The unique ID of the edge to delete.</param>
        /// <returns>A task representing the asynchronous delete operation. Returns true on success, false otherwise.</returns>
        Task<bool> DeleteEdgeDataAsync(Guid edgeId);

         /// <summary>
        /// Deletes the data for a specific edge using its connecting nodes.
        /// Note: Implementations might need indexing to support this efficiently without EdgeId.
        /// </summary>
        /// <param name="fromNodeId">The ID of the source node.</param>
        /// <param name="toNodeId">The ID of the target node.</param>
        /// <returns>A task representing the asynchronous delete operation. Returns true on success, false otherwise.</returns>
        Task<bool> DeleteEdgeDataAsync(long fromNodeId, long toNodeId); // Overload for convenience?

        /// <summary>
        /// Retrieves a list of all node IDs present in the storage.
        /// </summary>
        /// <returns>A task representing the asynchronous operation, containing a list of node IDs.</returns>
        Task<List<long>> GetAllNodeIdsAsync();

        /// <summary>
        /// Retrieves the unique IDs of all outgoing edges for a given node.
        /// </summary>
        /// <param name="nodeId">The ID of the source node.</param>
        /// <returns>A task representing the asynchronous operation, containing a list of outgoing edge Guids.</returns>
        Task<List<Guid>> GetOutgoingEdgeIdsAsync(long nodeId);

        /// <summary>
        /// Retrieves the unique IDs of all incoming edges for a given node.
        /// </summary>
        /// <param name="nodeId">The ID of the target node.</param>
        /// <returns>A task representing the asynchronous operation, containing a list of incoming edge Guids.</returns>
        Task<List<Guid>> GetIncomingEdgeIdsAsync(long nodeId);

        // Consider adding methods for bulk operations or more complex queries if needed later.
    }
}
