// File: GraphFileManager.cs

using Newtonsoft.Json;
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

                // Update index (assuming index stores file paths, not edge counts directly now)
                // TODO: Revisit IndexManager logic - does it still need edge count?
                indexManager.AddOrUpdateNode(nodeId, nodeFilePath, 0); 
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
                return null;
            }
        }

        // --- IGraphStorageProvider Implementation (Continued) ---

        public Task<bool> DeleteNodeDataAsync(long nodeId)
        {
             try
            {
                string nodeFilePath = GetNodeFilePath(nodeId);
                if (File.Exists(nodeFilePath))
                {
                    File.Delete(nodeFilePath);
                    indexManager.RemoveNode(nodeId);

                    // Also remove the potentially large edge directories for this node
                    // Note: This assumes edges related ONLY to this node are stored here.
                    // A more robust system might require iterating edges first.
                    string outgoingEdgeDir = GetEdgeDirPath(nodeId, true);
                    if (Directory.Exists(outgoingEdgeDir)) Directory.Delete(outgoingEdgeDir, true);
                    string incomingEdgeDir = GetEdgeDirPath(nodeId, false);
                    if (Directory.Exists(incomingEdgeDir)) Directory.Delete(incomingEdgeDir, true);

                    return Task.FromResult(true);
                }
                DebugWriter.DebugWriteLine("#00DEL1#", $"Node file {nodeFilePath} does not exist for deletion.");
                return Task.FromResult(false);
            }
            catch (Exception ex)
            {
                DebugWriter.DebugWriteLine("#00DEL2#", $"Error deleting node data for {nodeId}: {ex.Message}");
                return Task.FromResult(false);
            }
        }

        // --- Stubs for remaining IGraphStorageProvider methods ---
        // TODO: Implement these methods properly using file system logic

        public Task<string?> GetEdgeDataAsync(Guid edgeId)
        {
            // Need a way to map Guid back to file path (e.g., an index or naming convention)
            DebugWriter.DebugWriteLine("#EDGE_TODO#", $"GetEdgeDataAsync(Guid) not implemented for file storage.");
            return Task.FromResult<string?>(null); 
        }
        
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
                EnsureDirectoryExists(outgoingEdgeFilePath);
                File.WriteAllText(outgoingEdgeFilePath, edgeData);
                UpdateEdgeIndex(outgoingEdgeFilePath, true); // Update outgoing index

                // Save incoming representation
                string incomingEdgeFilePath = GetEdgeFilePath(toNodeId, fromNodeId, false);
                 EnsureDirectoryExists(incomingEdgeFilePath);
                File.WriteAllText(incomingEdgeFilePath, edgeData); // Save same data
                UpdateEdgeIndex(incomingEdgeFilePath, true); // Update incoming index

                // TODO: Update node edge counts? IndexManager needs rework.
                // UpdateNodeEdgeCount(fromNodeId, true);
                // UpdateNodeEdgeCount(toNodeId, false);

                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                DebugWriter.DebugWriteLine("#SAVE_EDGE_ERR#", $"Error saving edge data for {edgeId} ({fromNodeId}->{toNodeId}): {ex.Message}");
                return Task.FromResult(false);
            }
        }

        public Task<bool> DeleteEdgeDataAsync(Guid edgeId)
        {
             // Need a way to map Guid back to file path(s)
             DebugWriter.DebugWriteLine("#EDGE_TODO#", $"DeleteEdgeDataAsync(Guid) not implemented for file storage.");
             return Task.FromResult(false);
        }

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

                // TODO: Update node edge counts? IndexManager needs rework.
                // UpdateNodeEdgeCount(fromNodeId, true);
                // UpdateNodeEdgeCount(toNodeId, false);

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


        // --- Existing Methods (May need removal/refactoring) ---

        // LoadNode is now replaced by GetNodeDataAsync + deserialization layer above
        /*
        public NodeBase? LoadNode(long nodeId) { ... } 
        */

        // SaveNode is now replaced by serialization layer above + SaveNodeDataAsync
        /*
        public bool SaveNode(NodeBase node) { ... }
        */

        // DeleteNode is now replaced by DeleteNodeDataAsync + logic layer above
        /*
        public bool DeleteNode(long nodeId) { ... }
        */

        // DeleteEdgesForNode logic moved/commented out
        /*
        private void DeleteEdgesForNode(long nodeId, bool outgoing) { ... }
        */

        // SaveEdge is now replaced by serialization layer above + SaveEdgeDataAsync
        /*
        public bool SaveEdge(EdgeBase edge) { ... }
        */

        // LoadEdges is now replaced by GetEdgeIdsAsync + GetEdgeDataAsync + deserialization layer above
        /*
        public List<EdgeBase> LoadEdges(long nodeId, bool outgoing = true) { ... }
        */

        // DeleteEdge is now replaced by DeleteEdgeDataAsync
        /*
        public bool DeleteEdge(long fromNodeId, long toNodeId) { ... }
        */


        // --- Helper methods used by the interface implementations ---

        private bool NodeExists(long nodeId)
        {
            string nodeFilePath = GetNodeFilePath(nodeId);
            return File.Exists(nodeFilePath);
        }

        private bool SaveEdgeToFile(EdgeBase edge, string filePath)
        {
            try
            {
                // Changed to Detailed
                DebugWriter.DebugWriteLine("#R5TZE3#", $"Saving edge to file: {filePath}", true, VerbosityLevel.Detailed); 
                EnsureDirectoryExists(filePath);
                var edgeData = new
                {
                    Version = edge.Version,
                    Edge = edge
                };
                string jsonData = JsonConvert.SerializeObject(edgeData, Formatting.Indented);
                File.WriteAllText(filePath, jsonData);

                // Update index files
                bool indexUpdated = UpdateEdgeIndex(filePath, true);
                // Changed to Detailed
                DebugWriter.DebugWriteLine("#33RNRG#", $"Index update result for {filePath}: {indexUpdated}", true, VerbosityLevel.Detailed); 
                return indexUpdated;
            }
            catch (Exception ex)
            {
                DebugWriter.DebugWriteLine("#00SAV6#", $"Error saving edge to file {filePath}: {ex.Message}");
                return false;
            }
        }

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

                foreach (string filePath in edgeFiles)
                {
                    string fileContent = File.ReadAllText(filePath);
                    var edgeData = JsonConvert.DeserializeObject<dynamic>(fileContent);
                    
                    if (edgeData == null)
                    {
                        DebugWriter.DebugWriteLine("#XKIRIV#", $"Failed to deserialize edge data from file: {filePath}");
                        continue;
                    }

                    int version = (int)edgeData.Version;
                    EdgeBase? edge;

                    // Since EdgeV1 is removed, we only expect V2+ for edges
                    switch (version) 
                    {
                        // case 1: // Removed V1 handling
                        //    edge = JsonConvert.DeserializeObject<EdgeV1>(edgeData.Edge.ToString());
                        //    break; 
                        case 2:
                            edge = JsonConvert.DeserializeObject<EdgeV2>(edgeData.Edge.ToString()); // Assumes EdgeV2 is still the latest edge version
                            break;
                        default:
                            throw new NotSupportedException($"Edge version {version} is not supported.");
                    }

                    if (edge != null)
                    {
                        edges.Add(edge.UpgradeToLatest());
                    }
                }
            }
            catch (Exception ex)
            {
                DebugWriter.DebugWriteLine("#00LOD4#", $"Error loading edges for node {nodeId}: {ex.Message}");
            }

            return edges;
        }

        public bool DeleteEdge(long fromNodeId, long toNodeId)
        {
            try
            {
                // Delete from outgoing edges
                string outgoingEdgeFilePath = GetEdgeFilePath(fromNodeId, toNodeId, true);
                if (File.Exists(outgoingEdgeFilePath))
                {
                    File.Delete(outgoingEdgeFilePath);
                    RemoveEdgeFromIndex(outgoingEdgeFilePath);
                }

                // Delete from incoming edges
                string incomingEdgeFilePath = GetEdgeFilePath(toNodeId, fromNodeId, false);
                if (File.Exists(incomingEdgeFilePath))
                {
                    File.Delete(incomingEdgeFilePath);
                    RemoveEdgeFromIndex(incomingEdgeFilePath);
                }

                // Update edge counts for nodes
                UpdateNodeEdgeCount(fromNodeId, true);
                UpdateNodeEdgeCount(toNodeId, false);

                return true;
            }
            catch (Exception ex)
            {
                DebugWriter.DebugWriteLine("#00DEL3#", $"Error deleting edge from {fromNodeId} to {toNodeId}: {ex.Message}");
                return false;
            }
        }

        private bool UpdateNodeEdgeCount(long nodeId, bool outgoing)
        {
            int edgeCount = GetEdgeCount(nodeId, outgoing);
            indexManager.AddOrUpdateNode(nodeId, GetNodeFilePath(nodeId), edgeCount);
            return true;
        }

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

            return Path.Combine(pathSegments.ToArray());
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
                return JsonConvert.DeserializeObject<IndexFile>(json) ?? new IndexFile();
            }
            else
            {
                return new IndexFile();
            }
        }

        private void SaveIndexFile(string indexFilePath, IndexFile indexFile)
        {
            string json = JsonConvert.SerializeObject(indexFile, Formatting.Indented);
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
