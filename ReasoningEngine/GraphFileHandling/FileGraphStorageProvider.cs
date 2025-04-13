// File: GraphFileManager.cs

using System.Text.Json; // Changed from Newtonsoft.Json
using DebugUtils;
using System.Collections.Concurrent;

using System.Threading.Tasks; // Added for Task

namespace ReasoningEngine.GraphFileHandling
{
    // Renamed and now implements IGraphStorageProvider
    public class FileGraphStorageProvider : IGraphStorageProvider 
    {
        private readonly string baseDir;
        // IndexManager might still be needed here for file-based indexing
        private readonly IndexManager indexManager; 

        public FileGraphStorageProvider(string baseDir) // Renamed constructor
        {
            this.baseDir = baseDir;
            string indexFilePath = Path.Combine(baseDir, "index.json");
            this.indexManager = new IndexManager(indexFilePath); // Keep index manager for file provider
        }

        // --- IGraphStorageProvider Implementation ---

        public Task<List<long>> GetAllNodeIdsAsync()
        {
            // IndexManager provides this synchronously for the file system
            return Task.FromResult(indexManager.GetNodeIds()); 
        }

        public Task<bool> SaveNodeDataAsync(long nodeId, string nodeData)
        {
             try
            {
                string nodeFilePath = GetNodeFilePath(nodeId); // Use helper
                EnsureDirectoryExists(nodeFilePath); // Use helper

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

       public Task<string?> GetNodeDataAsync(long nodeId)
        {
             try
            {
                string nodeFilePath = GetNodeFilePath(nodeId); // Use helper

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

        // --- IGraphStorageProvider Implementation (Continued) ---

        public async Task<bool> DeleteNodeDataAsync(long nodeId) // Changed to async Task
        {
             try
            {
                string nodeFilePath = GetNodeFilePath(nodeId);
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
                string outgoingEdgeDir = GetEdgeDirPath(nodeId, true);
                if (Directory.Exists(outgoingEdgeDir)) Directory.Delete(outgoingEdgeDir, true);
                string incomingEdgeDir = GetEdgeDirPath(nodeId, false);
                if (Directory.Exists(incomingEdgeDir)) Directory.Delete(incomingEdgeDir, true);

                return true; // Return true directly
            }
            catch (Exception ex)
            {
                DebugWriter.DebugWriteLine("#00DEL2#", $"Error deleting node data for {nodeId}: {ex.Message}");
                return false; // Corrected return for async Task<bool>
            }
        }

        // --- Stubs for remaining IGraphStorageProvider methods ---
        // TODO: Implement these methods properly using file system logic

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
                            IndexFile indexFile = LoadIndexFile(indexFilePath);
                            foreach (var edgeFileName in indexFile.EdgeFiles)
                            {
                                string edgeFilePath = Path.Combine(nodeDir, edgeFileName);
                                if (File.Exists(edgeFilePath))
                                {
                                    string jsonData = File.ReadAllText(edgeFilePath);
                                    // Partially deserialize to check Guid without loading the full object
                                     // Assuming EdgeId is directly on the serialized object (might need adjustment if nested)
                                     // Changed from JsonConvert, added case-insensitive option
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
        
        public Task<string?> GetEdgeDataAsync(long fromNodeId, long toNodeId)
        {
             try
            {
                string edgeFilePath = GetEdgeFilePath(fromNodeId, toNodeId, true); // Check outgoing path
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

        public Task<bool> SaveEdgeDataAsync(Guid edgeId, long fromNodeId, long toNodeId, string edgeData)
        {
            // Need to save based on from/to for directory structure, potentially store Guid in JSON?
             try
            {
                // Save outgoing representation
                string outgoingEdgeFilePath = GetEdgeFilePath(fromNodeId, toNodeId, true);
                DebugWriter.DebugWriteLine("#SAVE_EDGE_PATH_OUT#", $"Saving outgoing edge {edgeId} to: {outgoingEdgeFilePath}", true, VerbosityLevel.Detailed); // Added Logging
                EnsureDirectoryExists(outgoingEdgeFilePath);
                File.WriteAllText(outgoingEdgeFilePath, edgeData);
                UpdateEdgeIndex(outgoingEdgeFilePath, true); // Update outgoing index

                // Save incoming representation
                string incomingEdgeFilePath = GetEdgeFilePath(toNodeId, fromNodeId, false);
                DebugWriter.DebugWriteLine("#SAVE_EDGE_PATH_IN#", $"Saving incoming edge {edgeId} to: {incomingEdgeFilePath}", true, VerbosityLevel.Detailed); // Added Logging
                 EnsureDirectoryExists(incomingEdgeFilePath);
                File.WriteAllText(incomingEdgeFilePath, edgeData); // Save same data
                UpdateEdgeIndex(incomingEdgeFilePath, true); // Update incoming index

                // Removed calls to UpdateNodeEdgeCount as EdgeCount is no longer tracked in IndexManager

                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                DebugWriter.DebugWriteLine("#SAVE_EDGE_ERR#", $"Error saving edge data for {edgeId} ({fromNodeId}->{toNodeId}): {ex.Message}");
                return Task.FromResult(false);
            }
        }

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
                 // Changed from JsonConvert, added case-insensitive option
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

        public Task<bool> DeleteEdgeDataAsync(long fromNodeId, long toNodeId)
        {
            try
            {
                bool deletedOutgoing = false;
                bool deletedIncoming = false;

                // Delete from outgoing edges
                string outgoingEdgeFilePath = GetEdgeFilePath(fromNodeId, toNodeId, true);
                if (File.Exists(outgoingEdgeFilePath))
                {
                    File.Delete(outgoingEdgeFilePath);
                    RemoveEdgeFromIndex(outgoingEdgeFilePath); // Update index
                    deletedOutgoing = true;
                }

                // Delete from incoming edges
                string incomingEdgeFilePath = GetEdgeFilePath(toNodeId, fromNodeId, false);
                if (File.Exists(incomingEdgeFilePath))
                {
                    File.Delete(incomingEdgeFilePath);
                    RemoveEdgeFromIndex(incomingEdgeFilePath); // Update index
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

        public Task<List<Guid>> GetOutgoingEdgeIdsAsync(long nodeId)
        {
            // Implementation uses existing LoadEdges which reads all edge files for the node.
            // This is potentially inefficient for large numbers of edges but fulfills the interface contract.
            // A more efficient implementation would require changes to the indexing or file storage structure.
            var edges = LoadEdges(nodeId, true); // Load all outgoing EdgeBase objects
            var edgeIds = edges.Select(e => e.EdgeId).ToList(); // Extract Guids
            return Task.FromResult(edgeIds);
        }

        public Task<List<Guid>> GetIncomingEdgeIdsAsync(long nodeId)
        {
             DebugWriter.DebugWriteLine("#EDGE_TODO#", $"GetIncomingEdgeIdsAsync not efficiently implemented for file storage.");
             // Placeholder: Load edges and extract Guids (inefficient)
             var edges = LoadEdges(nodeId, false);
             return Task.FromResult(edges.Select(e => e.EdgeId).ToList());
        }

        // --- Helper methods used by the interface implementations ---

        // Note: Old methods like LoadNode, SaveNode, DeleteNode, SaveEdge, DeleteEdge, DeleteEdgesForNode
        // have been removed as their functionality is now handled by the IGraphStorageProvider implementations
        // and the GraphObjectMapper layer. The LoadEdges method below is still used by GetOutgoing/IncomingEdgeIdsAsync.

        private bool NodeExists(long nodeId)
        {
            string nodeFilePath = GetNodeFilePath(nodeId);
            return File.Exists(nodeFilePath);
        }

        // Note: SaveEdgeToFile removed as SaveEdgeDataAsync now handles writing the pre-serialized data.

        private bool UpdateEdgeIndex(string edgeFilePath, bool isAdding)
        {
            try
            {
                // Changed to Detailed
                DebugWriter.DebugWriteLine("#4SZT2R#", $"Updating edge index for {edgeFilePath}, isAdding: {isAdding}", true, VerbosityLevel.Detailed); 
                string? directoryPath = Path.GetDirectoryName(edgeFilePath);
                if (string.IsNullOrEmpty(directoryPath))
                {
                    throw new InvalidOperationException("Unable to get directory path for edge file");
                }
                string indexFilePath = Path.Combine(directoryPath, "index.json");
                
                EnsureDirectoryExists(indexFilePath);

                IndexFile indexFile = LoadIndexFile(indexFilePath);
                 // Changed to Detailed
                DebugWriter.DebugWriteLine("#JM16CX#", $"Loaded index file: {indexFilePath}, current edge count: {indexFile.EdgeFiles.Count}", true, VerbosityLevel.Detailed);

                string edgeFileName = Path.GetFileName(edgeFilePath);

                if (isAdding)
                {
                    if (!indexFile.EdgeFiles.Contains(edgeFileName))
                    {
                        indexFile.EdgeFiles.Add(edgeFileName);
                         // Changed to Detailed
                        DebugWriter.DebugWriteLine("#21YE2B#", $"Added {edgeFileName} to index", true, VerbosityLevel.Detailed);
                    }
                    else
                    {
                         // Changed to Detailed
                        DebugWriter.DebugWriteLine("#C6B5G3#", $"{edgeFileName} already exists in index", true, VerbosityLevel.Detailed);
                    }
                }
                else
                {
                    indexFile.EdgeFiles.Remove(edgeFileName);
                     // Changed to Detailed
                    DebugWriter.DebugWriteLine("#50XXE9#", $"Removed {edgeFileName} from index", true, VerbosityLevel.Detailed);
                }

                SaveIndexFile(indexFilePath, indexFile);
                 // Changed to Detailed
                DebugWriter.DebugWriteLine("#0LU03E#", $"Saved updated index file: {indexFilePath}, new edge count: {indexFile.EdgeFiles.Count}", true, VerbosityLevel.Detailed);
                return true;
            }
            catch (Exception ex)
            {
                DebugWriter.DebugWriteLine("#00SAV8#", $"Error updating edge index: {ex.Message}");
                return false;
            }
        }

        public List<EdgeBase> LoadEdges(long nodeId, bool outgoing = true)
        {
            var edges = new List<EdgeBase>();
            try
            {
                string edgeDir = GetEdgeDirPath(nodeId, outgoing);
                if (!Directory.Exists(edgeDir))
                {
                    return edges;
                }

                // Traverse the directory hierarchy using index files
                var edgeFiles = GetAllEdgeFiles(nodeId, outgoing);

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

        // Note: DeleteEdge(long, long) removed as DeleteEdgeDataAsync(long, long) provides the same functionality.

        // Removed UpdateNodeEdgeCount method as EdgeCount is no longer tracked in IndexManager

        private void EnsureDirectoryExists(string filePath)
        {
            string? directoryPath = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }

        public string GetNodeFilePath(long nodeId)
        {
            string nodeIdStr = nodeId.ToString("D16");
            string[] pathSegments = new string[5];
            pathSegments[0] = baseDir;
            pathSegments[1] = nodeIdStr.Substring(0, 4);
            pathSegments[2] = nodeIdStr.Substring(0, 8);
            pathSegments[3] = nodeIdStr.Substring(0, 12);
            pathSegments[4] = nodeIdStr + ".json";
            return Path.Combine(pathSegments);
        }

        public string GetEdgeFilePath(long fromNodeId, long toNodeId, bool outgoing)
        {
            string direction = outgoing ? "outgoing" : "incoming";
            string fromNodeIdStr = fromNodeId.ToString("D16");
            string toNodeIdStr = toNodeId.ToString("D16");

            List<string> pathSegments = new List<string>
            {
                baseDir,
                "edges",
                direction,
                fromNodeIdStr.Substring(0, 4),
                fromNodeIdStr.Substring(0, 8),
                fromNodeIdStr.Substring(0, 12),
                fromNodeIdStr
            };

            // Include ToNodeId segments to further split directories
            pathSegments.Add($"{fromNodeIdStr}-{toNodeIdStr.Substring(0, 4)}");
            pathSegments.Add($"{fromNodeIdStr}-{toNodeIdStr.Substring(0, 8)}");
            pathSegments.Add($"{fromNodeIdStr}-{toNodeIdStr.Substring(0, 12)}");

            string fileName = $"{fromNodeIdStr}-{toNodeIdStr}.json";
            pathSegments.Add(fileName);

            string finalPath = Path.Combine(pathSegments.ToArray());
            // Log the generated path
            DebugWriter.DebugWriteLine("#GET_EDGE_PATH#", $"Generated edge path ({direction}): {finalPath}", true, VerbosityLevel.Detailed); // Added Logging
            return finalPath;
        }

        public string GetEdgeDirPath(long nodeId, bool outgoing)
        {
            string direction = outgoing ? "outgoing" : "incoming";
            string nodeIdStr = nodeId.ToString("D16");
            string[] pathSegments = new string[7];
            pathSegments[0] = baseDir;
            pathSegments[1] = "edges";
            pathSegments[2] = direction;
            pathSegments[3] = nodeIdStr.Substring(0, 4);
            pathSegments[4] = nodeIdStr.Substring(0, 8);
            pathSegments[5] = nodeIdStr.Substring(0, 12);
            pathSegments[6] = nodeIdStr;
            return Path.Combine(pathSegments);
        }

        private int GetEdgeCount(long nodeId, bool outgoing)
        {
            var edgeFiles = GetAllEdgeFiles(nodeId, outgoing);
            return edgeFiles.Count;
        }

        private void RemoveEdgeFromIndex(string edgeFilePath)
        {
            string? indexFilePath = GetIndexFilePath(edgeFilePath);
            if (indexFilePath != null)
            {
                UpdateEdgeIndex(edgeFilePath, false);
            }
        }

        private string? GetIndexFilePath(string edgeFilePath)
        {
            string? directoryPath = Path.GetDirectoryName(edgeFilePath);
            if (directoryPath == null)
            {
                return null;
            }
            return Path.Combine(directoryPath, "index.json");
        }

        private IndexFile LoadIndexFile(string indexFilePath)
        {
            if (File.Exists(indexFilePath))
            {
                string json = File.ReadAllText(indexFilePath);
                // Ensure System.Text.Json is used, add case-insensitive option
                var optionsCI = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                return JsonSerializer.Deserialize<IndexFile>(json, optionsCI) ?? new IndexFile(); 
            }
            else
            {
                return new IndexFile();
            }
        }

        private void SaveIndexFile(string indexFilePath, IndexFile indexFile)
        {
            var options = new JsonSerializerOptions { WriteIndented = true }; // Options for System.Text.Json
            // Ensure System.Text.Json is used
            string json = JsonSerializer.Serialize(indexFile, options); 
            File.WriteAllText(indexFilePath, json);
        }

        private List<string> GetAllEdgeFiles(long nodeId, bool outgoing)
        {
            List<string> edgeFiles = new List<string>();
            string edgeDir = GetEdgeDirPath(nodeId, outgoing);

             // Changed to Detailed
            DebugWriter.DebugWriteLine("#NLAIW7#", $"Getting all edge files for node {nodeId}, outgoing: {outgoing}", true, VerbosityLevel.Detailed);
             // Changed to Detailed
            DebugWriter.DebugWriteLine("#00LOD6#", $"Edge directory: {edgeDir}", true, VerbosityLevel.Detailed);

            if (!Directory.Exists(edgeDir))
            {
                 // Changed to Detailed
                DebugWriter.DebugWriteLine("#00LOD7#", $"Edge directory does not exist: {edgeDir}", true, VerbosityLevel.Detailed);
                return edgeFiles;
            }

            // Recursively search for index files
            SearchDirectoryForEdges(edgeDir, edgeFiles);

             // Changed to Detailed
            DebugWriter.DebugWriteLine("#QSI0XM#", $"Total edge files found: {edgeFiles.Count}", true, VerbosityLevel.Detailed);
            return edgeFiles;
        }

        private void SearchDirectoryForEdges(string directory, List<string> edgeFiles)
        {
            string indexFilePath = Path.Combine(directory, "index.json");
             // Changed to Detailed
            DebugWriter.DebugWriteLine("#00LOD8#", $"Checking index file: {indexFilePath}", true, VerbosityLevel.Detailed);

            if (File.Exists(indexFilePath))
            {
                IndexFile indexFile = LoadIndexFile(indexFilePath);

                // Add edge files
                foreach (var fileName in indexFile.EdgeFiles)
                {
                    string fullPath = Path.Combine(directory, fileName);
                    edgeFiles.Add(fullPath);
                     // Changed to Detailed
                    DebugWriter.DebugWriteLine("#00LOD9#", $"Added edge file: {fullPath}", true, VerbosityLevel.Detailed);
                }
            }
            else
            {
                 // Changed to Detailed
                DebugWriter.DebugWriteLine("#BCWITX#", $"Index file not found: {indexFilePath}", true, VerbosityLevel.Detailed);
            }

            // Recursively search subdirectories
            foreach (var subDir in Directory.GetDirectories(directory))
            {
                SearchDirectoryForEdges(subDir, edgeFiles);
            }
        }
    }

    public class IndexFile
    {
        public List<string> Subdirectories { get; set; } = new List<string>();
        public List<string> EdgeFiles { get; set; } = new List<string>();
    }
}
