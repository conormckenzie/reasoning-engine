// File: GraphFileManager.cs

using System; // Added for Guid
using System.Text.Json; // Changed from Newtonsoft.Json
using DebugUtils;
using System.Collections.Concurrent;
using System.IO; // Added for Path, File, Directory
using System.Linq; // Added for Union, ToList
using System.Threading.Tasks; // Added for Task

namespace ReasoningEngine.GraphFileHandling
{
    /// <summary>
    /// Provides a file-system based implementation of the IGraphStorageProvider interface.
    /// Stores graph data (Nodes and Edges) as JSON files in a hierarchical directory structure.
    /// </summary>
    // Renamed and now implements IGraphStorageProvider
    public class FileGraphStorageProvider : IGraphStorageProvider
    {
        private readonly string baseDir;
        private readonly IndexManager indexManager; // For main node index
        private readonly EdgeIndexFileHandler _edgeIndexHandler; // Handler for edge index files

        /// <summary>
        /// Initializes a new instance of the FileGraphStorageProvider.
        /// </summary>
        /// <param name="baseDir">The base directory path where graph data will be stored.</param>
        public FileGraphStorageProvider(string baseDir) // Renamed constructor
        {
            this.baseDir = baseDir;
            string indexFilePath = Path.Combine(baseDir, "index.json");
            this.indexManager = new IndexManager(indexFilePath);
            this._edgeIndexHandler = new EdgeIndexFileHandler(); // Instantiate the handler
        }

        // --- IGraphStorageProvider Implementation ---

        /// <summary>
        /// Asynchronously retrieves a list of all node IDs from the storage.
        /// </summary>
        /// <returns>A Task containing a list of all node IDs.</returns>
        public Task<List<long>> GetAllNodeIdsAsync()
        {
            // IndexManager provides this synchronously for the file system
            return Task.FromResult(indexManager.GetNodeIds());
        }

        /// <summary>
        /// Asynchronously saves or updates the data for a specific node.
        /// </summary>
        /// <param name="nodeId">The ID of the node.</param>
        /// <param name="nodeData">The raw string data (JSON) of the node.</param>
        /// <returns>A Task containing true if the operation was successful, false otherwise.</returns>
        public Task<bool> SaveNodeDataAsync(long nodeId, string nodeData)
        {
             try
            {
                string nodeFilePath = FilePathHelper.GetNodeFilePath(baseDir, nodeId); // Use helper
                FilePathHelper.EnsureDirectoryExists(nodeFilePath); // Use static helper

                // Directly write the provided string data
                File.WriteAllText(nodeFilePath, nodeData);

                // Update index with node ID and file path
                indexManager.AddOrUpdateNode(nodeId, nodeFilePath); // Removed edgeCount argument
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                DebugWriter.DebugWriteLine("#00SAV1#", $"Error saving node data for {nodeId}: {ex.Message}");
                return Task.FromResult(false);
            }
        }

        /// <summary>
        /// Asynchronously retrieves the raw data for a specific node.
        /// </summary>
        /// <param name="nodeId">The ID of the node.</param>
        /// <returns>A Task containing the raw string data (JSON) of the node, or null if the node is not found.</returns>
       public Task<string?> GetNodeDataAsync(long nodeId)
        {
             try
            {
                string nodeFilePath = FilePathHelper.GetNodeFilePath(baseDir, nodeId); // Use helper

                if (!File.Exists(nodeFilePath))
                {
                    DebugWriter.DebugWriteLine("#00LOD1#", $"Node file {nodeFilePath} does not exist.");
                    return Task.FromResult<string?>(null);
                }

                // Read the raw JSON data as a string
                string jsonData = File.ReadAllText(nodeFilePath);
                return Task.FromResult<string?>(jsonData);
            }
            catch (Exception ex)
            {
                DebugWriter.DebugWriteLine("#00LOD2#", $"Error loading node {nodeId}: {ex.Message}");
                return Task.FromResult<string?>(null); // Corrected return type
            }
        }

        /// <summary>
        /// Asynchronously checks if a node with the specified ID exists in the storage.
        /// </summary>
        /// <param name="nodeId">The ID of the node.</param>
        /// <returns>A Task containing true if the node exists, false otherwise.</returns>
        public async Task<bool> NodeExistsAsync(long nodeId)
        {
            string nodeFilePath = FilePathHelper.GetNodeFilePath(baseDir, nodeId);
            // File.Exists is synchronous, wrap in Task.FromResult
            return await Task.FromResult(File.Exists(nodeFilePath));
        }


        // --- IGraphStorageProvider Implementation (Continued) ---

        /// <summary>
        /// Asynchronously deletes the data for a specific node and its associated edges.
        /// </summary>
        /// <param name="nodeId">The ID of the node to delete.</param>
        /// <returns>A Task containing true if the operation was successful, false otherwise.</returns>
        public async Task<bool> DeleteNodeDataAsync(long nodeId) // Changed to async Task
        {
             try
            {
                string nodeFilePath = FilePathHelper.GetNodeFilePath(baseDir, nodeId);
                if (!File.Exists(nodeFilePath))
                {
                    DebugWriter.DebugWriteLine("#00DEL1#", $"Node file {nodeFilePath} does not exist for deletion.");
                    return false; // Return false directly
                }

                // 1. Get all edge IDs connected to this node BEFORE deleting the node file
                var outgoingEdgeIds = await GetOutgoingEdgeIdsAsync(nodeId);
                var incomingEdgeIds = await GetIncomingEdgeIdsAsync(nodeId);
                var allEdgeIds = outgoingEdgeIds.Union(incomingEdgeIds).ToList(); // Combine and remove duplicates

                DebugWriter.DebugWriteLine("#DEL_NODE_EDGES#", $"Found {allEdgeIds.Count} unique edges connected to node {nodeId} for deletion.", true, VerbosityLevel.Detailed);

                // 2. Delete each associated edge (this handles both file representations and index updates)
                // Use a separate options instance for deserializing edge info
                var optionsCI = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                foreach (var edgeId in allEdgeIds)
                {
                    // Need edge data to know From/To nodes for DeleteEdgeDataAsync(long, long)
                    string? edgeData = await GetEdgeDataAsync(edgeId); // Use the (inefficient) Guid scan
                    if (edgeData != null)
                    {
                        var edgeInfo = JsonSerializer.Deserialize<EdgeFromToHelper>(edgeData, optionsCI);
                        if (edgeInfo != null)
                        {
                            DebugWriter.DebugWriteLine("#DEL_NODE_EDGE_CLEANUP#", $"Deleting edge {edgeId} ({edgeInfo.FromNode}->{edgeInfo.ToNode}) as part of node {nodeId} deletion.", true, VerbosityLevel.Detailed);
                            // Call DeleteEdgeDataAsync(from, to) which uses the handler
                            await DeleteEdgeDataAsync(edgeInfo.FromNode, edgeInfo.ToNode);
                        }
                        else
                        {
                             DebugWriter.DebugWriteLine("#DEL_NODE_EDGE_DESER_ERR#", $"Could not deserialize From/To for edge {edgeId} during node {nodeId} deletion.", true, VerbosityLevel.Minimal);
                             // Continue trying to delete other edges
                        }
                    }
                    else
                    {
                         DebugWriter.DebugWriteLine("#DEL_NODE_EDGE_NOTFOUND#", $"Edge {edgeId} data not found during node {nodeId} deletion (might have been deleted already).", true, VerbosityLevel.Normal);
                         // Continue trying to delete other edges
                    }
                }

                // 3. Delete the node file itself
                File.Delete(nodeFilePath);
                indexManager.RemoveNode(nodeId); // Remove node from the main index

                // 4. Remove the primary edge directories for this node (should be empty now, but remove for cleanliness)
                string outgoingEdgeDir = FilePathHelper.GetEdgeDirPath(baseDir, nodeId, true);
                if (Directory.Exists(outgoingEdgeDir)) Directory.Delete(outgoingEdgeDir, true);
                string incomingEdgeDir = FilePathHelper.GetEdgeDirPath(baseDir, nodeId, false);
                if (Directory.Exists(incomingEdgeDir)) Directory.Delete(incomingEdgeDir, true);

                return true; // Return true directly
            }
            catch (Exception ex)
            {
                DebugWriter.DebugWriteLine("#00DEL2#", $"Error deleting node data for {nodeId}: {ex.Message}");
                return false; // Corrected return for async Task<bool>
            }
        }

        // --- Edge Operations ---

        /// <summary>
        /// Asynchronously retrieves the raw data for a specific edge by its unique identifier (Guid).
        /// Note: This implementation is currently inefficient as it requires scanning all edge files.
        /// </summary>
        /// <param name="edgeId">The unique identifier (Guid) of the edge.</param>
        /// <returns>A Task containing the raw string data (JSON) of the edge, or null if the edge is not found.</returns>
        public Task<string?> GetEdgeDataAsync(Guid edgeId)
        {
            // Inefficient implementation: Scan all edge files.
            // TODO: Implement a more efficient lookup mechanism (e.g., Guid index).
            DebugWriter.DebugWriteLine("#GET_EDGE_GUID_SCAN#", $"Performing inefficient scan for Edge Guid {edgeId}.", true, VerbosityLevel.Normal);
            try
            {
                string edgesBasePath = Path.Combine(baseDir, "edges");
                if (!Directory.Exists(edgesBasePath)) return Task.FromResult<string?>(null);

                // Scan both outgoing and incoming directories
                foreach (var directionDir in Directory.GetDirectories(edgesBasePath)) // outgoing, incoming
                {
                    foreach (var nodeDir in Directory.GetDirectories(directionDir, "*", SearchOption.AllDirectories))
                    {
                        string indexFilePath = Path.Combine(nodeDir, "index.json");
                        if (File.Exists(indexFilePath))
                        {
                            // Use the handler to load the index file
                            EdgeIndexFileHandler.IndexFile indexFile = _edgeIndexHandler.LoadIndexFile(indexFilePath); // Use handler's internal method
                            foreach (var edgeFileName in indexFile.EdgeFiles)
                            {
                                string edgeFilePath = Path.Combine(nodeDir, edgeFileName);
                                if (File.Exists(edgeFilePath))
                                {
                                    string jsonData = File.ReadAllText(edgeFilePath);
                                    // Partially deserialize to check Guid without loading the full object
                                     var optionsCI = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                                     var edgeInfo = JsonSerializer.Deserialize<EdgeIdHelper>(jsonData, optionsCI);
                                     if (edgeInfo?.EdgeId == edgeId)
                                     {
                                         DebugWriter.DebugWriteLine("#GET_EDGE_GUID_FOUND#", $"Found edge file {edgeFilePath} for Guid {edgeId}.", true, VerbosityLevel.Detailed);
                                         return Task.FromResult<string?>(jsonData);
                                    }
                                }
                            }
                        }
                    }
                }
                 DebugWriter.DebugWriteLine("#GET_EDGE_GUID_NOTFOUND#", $"Edge Guid {edgeId} not found after scan.", true, VerbosityLevel.Normal);
                 return Task.FromResult<string?>(null); // Not found
            }
            catch (Exception ex)
            {
                 DebugWriter.DebugWriteLine("#GET_EDGE_GUID_ERR#", $"Error scanning for edge Guid {edgeId}: {ex.Message}");
                 return Task.FromResult<string?>(null);
            }
        }
        // Helper class for partial deserialization
        private class EdgeIdHelper { public Guid EdgeId { get; set; } }

        /// <summary>
        /// Asynchronously retrieves the raw data for a specific edge by its source and destination node IDs.
        /// </summary>
        /// <param name="fromNodeId">The ID of the source node.</param>
        /// <param name="toNodeId">The ID of the destination node.</param>
        /// <returns>A Task containing the raw string data (JSON) of the edge, or null if the edge is not found.</returns>
        public Task<string?> GetEdgeDataAsync(long fromNodeId, long toNodeId)
        {
             try
            {
                // For outgoing check, primary is fromNodeId, secondary is toNodeId
                string edgeFilePath = FilePathHelper.GetEdgeFilePath(baseDir, fromNodeId, toNodeId, true); // Check outgoing path
                if (!File.Exists(edgeFilePath))
                {
                    // Maybe check incoming path too? Or assume caller knows direction?
                    // For now, just check outgoing.
                    DebugWriter.DebugWriteLine("#GET_EDGE_FNF#", $"Edge file {edgeFilePath} does not exist.");
                    return Task.FromResult<string?>(null);
                }
                string jsonData = File.ReadAllText(edgeFilePath);
                return Task.FromResult<string?>(jsonData);
            }
            catch (Exception ex)
            {
                 DebugWriter.DebugWriteLine("#GET_EDGE_ERR#", $"Error loading edge data for {fromNodeId}->{toNodeId}: {ex.Message}");
                 return Task.FromResult<string?>(null);
            }
        }

        /// <summary>
        /// Asynchronously saves or updates the data for a specific edge.
        /// Saves two representations of the edge data (outgoing and incoming) for efficient retrieval by node ID.
        /// </summary>
        /// <param name="edgeId">The unique identifier (Guid) of the edge.</param>
        /// <param name="fromNodeId">The ID of the source node.</param>
        /// <param name="toNodeId">The ID of the destination node.</param>
        /// <param name="edgeData">The raw string data (JSON) of the edge.</param>
        /// <returns>A Task containing true if the operation was successful, false otherwise.</returns>
        public Task<bool> SaveEdgeDataAsync(Guid edgeId, long fromNodeId, long toNodeId, string edgeData)
        {
             try
            {
                // Save outgoing representation (primary=from, secondary=to, outgoing=true)
                string outgoingEdgeFilePath = FilePathHelper.GetEdgeFilePath(baseDir, fromNodeId, toNodeId, true);
                DebugWriter.DebugWriteLine("#SAVE_EDGE_PATH_OUT#", $"Saving outgoing edge {edgeId} to: {outgoingEdgeFilePath}", true, VerbosityLevel.Detailed);
                FilePathHelper.EnsureDirectoryExists(outgoingEdgeFilePath);
                File.WriteAllText(outgoingEdgeFilePath, edgeData);
                _edgeIndexHandler.UpdateEdgeIndex(outgoingEdgeFilePath, true); // Use handler

                // Save incoming representation (primary=to, secondary=from, outgoing=false)
                string incomingEdgeFilePath = FilePathHelper.GetEdgeFilePath(baseDir, toNodeId, fromNodeId, false);
                DebugWriter.DebugWriteLine("#SAVE_EDGE_PATH_IN#", $"Saving incoming edge {edgeId} to: {incomingEdgeFilePath}", true, VerbosityLevel.Detailed);
                FilePathHelper.EnsureDirectoryExists(incomingEdgeFilePath);
                File.WriteAllText(incomingEdgeFilePath, edgeData); // Save same data
                _edgeIndexHandler.UpdateEdgeIndex(incomingEdgeFilePath, true);

                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                DebugWriter.DebugWriteLine("#SAVE_EDGE_ERR#", $"Error saving edge data for {edgeId} ({fromNodeId}->{toNodeId}): {ex.Message}");
                return Task.FromResult(false);
            }
        }

        /// <summary>
        /// Asynchronously deletes the data for a specific edge by its unique identifier (Guid).
        /// Note: This implementation is currently inefficient as it requires scanning all edge files to find the edge.
        /// </summary>
        /// <param name="edgeId">The unique identifier (Guid) of the edge to delete.</param>
        /// <returns>A Task containing true if the operation was successful, false otherwise.</returns>
        public async Task<bool> DeleteEdgeDataAsync(Guid edgeId)
        {
            // Inefficient implementation: Scan to find the edge, then delete both representations.
            // TODO: Implement a more efficient lookup mechanism (e.g., Guid index).
             DebugWriter.DebugWriteLine("#DEL_EDGE_GUID_SCAN#", $"Performing inefficient scan to delete Edge Guid {edgeId}.", true, VerbosityLevel.Normal);
            try
            {
                string? edgeData = await GetEdgeDataAsync(edgeId); // Use the scan method above to find the data first
                if (edgeData == null) {
                     DebugWriter.DebugWriteLine("#DEL_EDGE_GUID_NOTFOUND#", $"Edge Guid {edgeId} not found for deletion.", true, VerbosityLevel.Normal);
                    return false; // Edge not found
                 }

                 // Deserialize to get FromNode and ToNode to delete both files
                 var optionsCI = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                 var edgeInfo = JsonSerializer.Deserialize<EdgeFromToHelper>(edgeData, optionsCI);
                 if (edgeInfo == null) {
                     DebugWriter.DebugWriteLine("#DEL_EDGE_GUID_DESER_ERR#", $"Could not deserialize From/To nodes from edge data for Guid {edgeId}.", true, VerbosityLevel.Minimal);
                     return false; // Could not determine file paths
                 }

                // Use the existing DeleteEdgeDataAsync(from, to) which handles both files and index updates
                return await DeleteEdgeDataAsync(edgeInfo.FromNode, edgeInfo.ToNode);
            }
            catch (Exception ex)
            {
                 DebugWriter.DebugWriteLine("#DEL_EDGE_GUID_ERR#", $"Error deleting edge Guid {edgeId}: {ex.Message}");
                 return false;
            }
        }
        // Helper class for partial deserialization
        private class EdgeFromToHelper { public long FromNode { get; set; } public long ToNode { get; set; } }

        /// <summary>
        /// Asynchronously deletes the data for a specific edge by its source and destination node IDs.
        /// Deletes both the outgoing and incoming representations of the edge data.
        /// </summary>
        /// <param name="fromNodeId">The ID of the source node.</param>
        /// <param name="toNodeId">The ID of the destination node.</param>
        /// <returns>A Task containing true if at least one representation of the edge was deleted, false otherwise.</returns>
        public Task<bool> DeleteEdgeDataAsync(long fromNodeId, long toNodeId)
        {
            try
            {
                bool deletedOutgoing = false;
                bool deletedIncoming = false;

                // Delete from outgoing edges (primary=from, secondary=to, outgoing=true)
                string outgoingEdgeFilePath = FilePathHelper.GetEdgeFilePath(baseDir, fromNodeId, toNodeId, true);
                if (File.Exists(outgoingEdgeFilePath))
                {
                    File.Delete(outgoingEdgeFilePath);
                    _edgeIndexHandler.RemoveEdgeFromIndex(outgoingEdgeFilePath); // Use handler
                    deletedOutgoing = true;
                }

                // Delete from incoming edges (primary=to, secondary=from, outgoing=false)
                string incomingEdgeFilePath = FilePathHelper.GetEdgeFilePath(baseDir, toNodeId, fromNodeId, false);
                if (File.Exists(incomingEdgeFilePath))
                {
                    File.Delete(incomingEdgeFilePath);
                    _edgeIndexHandler.RemoveEdgeFromIndex(incomingEdgeFilePath); // Use handler
                    deletedIncoming = true;
                }

                // Removed calls to UpdateNodeEdgeCount as EdgeCount is no longer tracked in IndexManager

                return Task.FromResult(deletedOutgoing || deletedIncoming); // Return true if at least one file was deleted
            }
            catch (Exception ex)
            {
                DebugWriter.DebugWriteLine("#DEL_EDGE_ERR#", $"Error deleting edge data for {fromNodeId}->{toNodeId}: {ex.Message}");
                return Task.FromResult(false);
            }
        }

        /// <summary>
        /// Asynchronously retrieves a list of unique identifiers (Guids) for all outgoing edges from a specific node.
        /// Note: This implementation is potentially inefficient as it loads all outgoing edge data to extract Guids.
        /// </summary>
        /// <param name="nodeId">The ID of the source node.</param>
        /// <returns>A Task containing a list of outgoing edge Guids.</returns>
        public Task<List<Guid>> GetOutgoingEdgeIdsAsync(long nodeId)
        {
            // Implementation uses existing LoadEdges which reads all edge files for the node.
            // This is potentially inefficient for large numbers of edges but fulfills the interface contract.
            // A more efficient implementation would require changes to the indexing or file storage structure.
            var edges = LoadEdges(nodeId, true); // Load all outgoing EdgeBase objects
            var edgeIds = edges.Select(e => e.EdgeId).ToList(); // Extract Guids
            return Task.FromResult(edgeIds);
        }

        /// <summary>
        /// Asynchronously retrieves a list of unique identifiers (Guids) for all incoming edges to a specific node.
        /// Note: This implementation is potentially inefficient as it loads all incoming edge data to extract Guids.
        /// </summary>
        /// <param name="nodeId">The ID of the destination node.</param>
        /// <returns>A Task containing a list of incoming edge Guids.</returns>
        public Task<List<Guid>> GetIncomingEdgeIdsAsync(long nodeId)
        {
             DebugWriter.DebugWriteLine("#EDGE_TODO#", $"GetIncomingEdgeIdsAsync not efficiently implemented for file storage.");
             // Placeholder: Load edges and extract Guids (inefficient)
             var edges = LoadEdges(nodeId, false);
             return Task.FromResult(edges.Select(e => e.EdgeId).ToList());
        }

        // --- Helper methods used by the interface implementations ---

        // LoadEdges method remains as it's used by GetOutgoing/IncomingEdgeIdsAsync
        // It now relies on the EdgeIndexFileHandler
        public List<EdgeBase> LoadEdges(long nodeId, bool outgoing = true)
        {
            var edges = new List<EdgeBase>();
            try
            {
                // Traverse the directory hierarchy using the handler
                var edgeFiles = _edgeIndexHandler.GetAllEdgeFiles(baseDir, nodeId, outgoing); // Use handler

                // Prepare options once, include case-insensitivity for robustness
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                foreach (string filePath in edgeFiles)
                {
                    string fileContent = File.ReadAllText(filePath);
                    EdgeBase? edge = null;
                    // Changed to Detailed
                    DebugWriter.DebugWriteLine("#LOAD_EDGE_ATTEMPT#", $"Attempting to deserialize edge from: {filePath}", true, VerbosityLevel.Detailed);
                    try
                    {
                        // Deserialize the entire content directly into EdgeV2
                        edge = JsonSerializer.Deserialize<EdgeV2>(fileContent, options);
                        // Changed to Detailed
                        DebugWriter.DebugWriteLine("#LOAD_EDGE_SUCCESS#", $"Successfully deserialized edge {edge?.EdgeId} from: {filePath}", true, VerbosityLevel.Detailed);

                        // Optional: Check version after deserialization if needed
                        if (edge != null && edge.Version != 2)
                        {
                             DebugWriter.DebugWriteLine("#EDGE_VER_WARN#", $"Loaded edge from {filePath} has unexpected version {edge.Version}.", true, VerbosityLevel.Minimal);
                             // Decide whether to discard or handle older versions if they reappear
                             edge = null; // Discard for now if version mismatch
                        }
                    }
                    catch (JsonException jsonEx)
                    {
                         DebugWriter.DebugWriteLine("#XKIRIV#", $"JSON Error deserializing edge data from file: {filePath}. Error: {jsonEx.Message}");
                         edge = null; // Ensure edge is null on error
                    }
                    catch (Exception ex) // Catch other potential errors during deserialization
                    {
                         DebugWriter.DebugWriteLine("#XKIRIV#", $"General Error deserializing edge data from file: {filePath}. Error: {ex.Message}");
                         edge = null; // Ensure edge is null on error
                    }

                    // Add the edge if deserialization was successful and version is okay (or version check removed)
                    if (edge != null)
                    {
                        edges.Add(edge.UpgradeToLatest()); // UpgradeToLatest might be redundant if only V2 exists
                    }
                    else
                    {
                        // Log and continue if deserialization failed
                        DebugWriter.DebugWriteLine("#XKIRIV#", $"Failed to deserialize edge data from file: {filePath}");
                        continue;
                    }
                }
                // Removed duplicate Newtonsoft.Json block from previous merge error
            }
            catch (Exception ex)
            {
                DebugWriter.DebugWriteLine("#00LOD4#", $"Error loading edges for node {nodeId}: {ex.Message}");
            }

            return edges;
        }

        // GetEdgeCount remains as a private helper, now using the handler
         private int GetEdgeCount(long nodeId, bool outgoing)
         {
             // Use handler to get edge files
             var edgeFiles = _edgeIndexHandler.GetAllEdgeFiles(baseDir, nodeId, outgoing); // Use handler
             return edgeFiles.Count;
         }

        // All other private helper methods related to edge index files are removed.
    }
}
