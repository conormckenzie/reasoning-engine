// File: GraphFileManager.cs

using Newtonsoft.Json;
using DebugUtils;
using System.Collections.Concurrent;

namespace ReasoningEngine.GraphFileHandling
{
    public class GraphFileManager
    {
        private readonly string baseDir;
        private readonly IndexManager indexManager;

        public GraphFileManager(string baseDir)
        {
            this.baseDir = baseDir;
            string indexFilePath = Path.Combine(baseDir, "index.json");
            this.indexManager = new IndexManager(indexFilePath);
        }

        public List<long> GetAllNodeIds()
        {
            return indexManager.GetNodeIds();
        }

        public bool SaveNode(NodeBase node)
        {
            try
            {
                string nodeFilePath = GetNodeFilePath(node.Id);
                EnsureDirectoryExists(nodeFilePath);

                var nodeData = new
                {
                    Version = node.Version,
                    Node = node
                };

                string jsonData = JsonConvert.SerializeObject(nodeData, Formatting.Indented);
                File.WriteAllText(nodeFilePath, jsonData);
                indexManager.AddOrUpdateNode(node.Id, nodeFilePath, 0);
                return true;
            }
            catch (Exception ex)
            {
                DebugWriter.DebugWriteLine("#00SAV1#", $"Error saving node {node.Id}: {ex.Message}");
                return false;
            }
        }

        public NodeBase? LoadNode(long nodeId)
        {
            try
            {
                string nodeFilePath = GetNodeFilePath(nodeId);

                if (!File.Exists(nodeFilePath))
                {
                    DebugWriter.DebugWriteLine("#00LOD1#", $"Node file {nodeFilePath} does not exist.");
                    return null;
                }

                string jsonData = File.ReadAllText(nodeFilePath);
                var nodeData = JsonConvert.DeserializeObject<dynamic>(jsonData);

                if (nodeData == null)
                {
                    DebugWriter.DebugWriteLine("#00LOD3#", $"Failed to deserialize node data for node {nodeId}.");
                    return null;
                }

                int version = (int)nodeData.Version;
                NodeBase? node = null; // Initialize as nullable

                // Extract the node part of the JSON to inspect its Type
                string nodeJson = nodeData.Node.ToString();
                var nodeObject = JsonConvert.DeserializeObject<dynamic>(nodeJson);
                
                if (nodeObject == null)
                {
                     DebugWriter.DebugWriteLine("#LOAD_ERR_NODE_OBJ#", $"Failed to deserialize inner node object for node {nodeId}.");
                     return null;
                }

                NodeType nodeType = (NodeType)(int)nodeObject.Type; // Get the type

                switch (version)
                {
                    case 1:
                        // Assuming V1 only had Standard nodes, or needs specific handling if not
                        node = JsonConvert.DeserializeObject<NodeV1>(nodeJson);
                        break;
                    case 2:
                        // Deserialize based on Type for V2
                        switch (nodeType)
                        {
                            case NodeType.SIMO:
                                node = JsonConvert.DeserializeObject<SIMONode>(nodeJson);
                                break;
                            case NodeType.MISO:
                                node = JsonConvert.DeserializeObject<MISONode>(nodeJson);
                                break;
                            case NodeType.Standard:
                            default: // Fallback to standard NodeV2
                                node = JsonConvert.DeserializeObject<NodeV2>(nodeJson);
                                break;
                        }
                        break;
                    default:
                         DebugWriter.DebugWriteLine("#LOAD_ERR_VERSION#", $"Node version {version} is not supported for node {nodeId}.");
                        throw new NotSupportedException($"Node version {version} is not supported.");
                }

                // UpgradeToLatest should handle null if deserialization failed
                return node?.UpgradeToLatest();
            }
            catch (Exception ex)
            {
                DebugWriter.DebugWriteLine("#00LOD2#", $"Error loading node {nodeId}: {ex.Message}");
                return null;
            }
        }

        public bool DeleteNode(long nodeId)
        {
            try
            {
                string nodeFilePath = GetNodeFilePath(nodeId);
                if (File.Exists(nodeFilePath))
                {
                    // Delete the node file
                    File.Delete(nodeFilePath);

                    // Delete outgoing edges
                    DeleteEdgesForNode(nodeId, true);

                    // Delete incoming edges
                    DeleteEdgesForNode(nodeId, false);

                    // Remove the node from the index
                    indexManager.RemoveNode(nodeId);

                    return true;
                }
                DebugWriter.DebugWriteLine("#00DEL1#", $"Node file {nodeFilePath} does not exist.");
                return false;
            }
            catch (Exception ex)
            {
                DebugWriter.DebugWriteLine("#00DEL2#", $"Error deleting node {nodeId}: {ex.Message}");
                return false;
            }
        }

        private void DeleteEdgesForNode(long nodeId, bool outgoing)
        {
            var edges = LoadEdges(nodeId, outgoing);
            foreach (var edge in edges)
            {
                long fromNodeId = outgoing ? nodeId : edge.FromNode;
                long toNodeId = outgoing ? edge.ToNode : nodeId;
                DeleteEdge(fromNodeId, toNodeId);
            }

            // Clear the index file for this node's edges
            string edgeDirPath = GetEdgeDirPath(nodeId, outgoing);
            string indexFilePath = Path.Combine(edgeDirPath, "index.json");
            if (File.Exists(indexFilePath))
            {
                File.Delete(indexFilePath);
            }
        }

        public bool SaveEdge(EdgeBase edge)
        {
            try
            {
                // Changed to Detailed
                DebugWriter.DebugWriteLine("#00SAV9#", $"Starting to save edge from {edge.FromNode} to {edge.ToNode}", true, VerbosityLevel.Detailed); 

                // Check if both nodes exist
                if (!NodeExists(edge.FromNode))
                {
                    DebugWriter.DebugWriteLine("#30QLDO#", $"Cannot save edge: source node {edge.FromNode} does not exist.");
                    return false;
                }
                if (!NodeExists(edge.ToNode))
                {
                    DebugWriter.DebugWriteLine("#B830YV#", $"Cannot save edge: destination node {edge.ToNode} does not exist.");
                    return false;
                }

                // Save outgoing edge
                string outgoingEdgeFilePath = GetEdgeFilePath(edge.FromNode, edge.ToNode, true);
                if (!SaveEdgeToFile(edge, outgoingEdgeFilePath))
                {
                    DebugWriter.DebugWriteLine("#00SAV3#", $"Failed to save outgoing edge file: {outgoingEdgeFilePath}");
                    return false;
                }

                // Save incoming edge
                string incomingEdgeFilePath = GetEdgeFilePath(edge.ToNode, edge.FromNode, false);
                if (!SaveEdgeToFile(edge, incomingEdgeFilePath))
                {
                    DebugWriter.DebugWriteLine("#00SAV4#", $"Failed to save incoming edge file: {incomingEdgeFilePath}");
                    return false;
                }

                // Update edge counts for nodes
                if (!UpdateNodeEdgeCount(edge.FromNode, true) || !UpdateNodeEdgeCount(edge.ToNode, false))
                {
                    DebugWriter.DebugWriteLine("#00SAV5#", $"Failed to update node edge counts for edge: {edge.FromNode} -> {edge.ToNode}");
                    return false;
                }

                // Changed to Detailed
                DebugWriter.DebugWriteLine("#O3ULSB#", $"Successfully saved edge from {edge.FromNode} to {edge.ToNode}", true, VerbosityLevel.Detailed); 
                return true;
            }
            catch (Exception ex)
            {
                DebugWriter.DebugWriteLine("#00SAV2#", $"Error saving edge from {edge.FromNode} to {edge.ToNode}: {ex.Message}");
                return false;
            }
        }

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

                    switch (version)
                    {
                        case 1:
                            edge = JsonConvert.DeserializeObject<EdgeV1>(edgeData.Edge.ToString());
                            break;
                        case 2:
                            edge = JsonConvert.DeserializeObject<EdgeV2>(edgeData.Edge.ToString());
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
