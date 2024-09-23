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

                int version = nodeData.Version;
                NodeBase node;

                switch (version)
                {
                    case 1:
                        node = JsonConvert.DeserializeObject<NodeV1>(nodeData.Node.ToString());
                        break;
                    case 2:
                        node = JsonConvert.DeserializeObject<NodeV2>(nodeData.Node.ToString());
                        break;
                    default:
                        throw new NotSupportedException($"Node version {version} is not supported.");
                }

                return node.UpgradeToLatest();
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
                DebugWriter.DebugWriteLine("#00SAV9#", $"Starting to save edge from {edge.FromNode} to {edge.ToNode}");

                // Check if both nodes exist
                if (!NodeExists(edge.FromNode))
                {
                    DebugWriter.DebugWriteLine("#00SAV19#", $"Cannot save edge: source node {edge.FromNode} does not exist.");
                    return false;
                }
                if (!NodeExists(edge.ToNode))
                {
                    DebugWriter.DebugWriteLine("#00SAV20#", $"Cannot save edge: destination node {edge.ToNode} does not exist.");
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

                DebugWriter.DebugWriteLine("#00SAV10#", $"Successfully saved edge from {edge.FromNode} to {edge.ToNode}");
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
                DebugWriter.DebugWriteLine("#00SAV11#", $"Saving edge to file: {filePath}");
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
                DebugWriter.DebugWriteLine("#00SAV12#", $"Index update result for {filePath}: {indexUpdated}");
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
                DebugWriter.DebugWriteLine("#00SAV13#", $"Updating edge index for {edgeFilePath}, isAdding: {isAdding}");
                string indexFilePath = Path.Combine(Path.GetDirectoryName(edgeFilePath), "index.json");
                
                EnsureDirectoryExists(indexFilePath);

                IndexFile indexFile = LoadIndexFile(indexFilePath);
                DebugWriter.DebugWriteLine("#00SAV14#", $"Loaded index file: {indexFilePath}, current edge count: {indexFile.EdgeFiles.Count}");

                string edgeFileName = Path.GetFileName(edgeFilePath);

                if (isAdding)
                {
                    if (!indexFile.EdgeFiles.Contains(edgeFileName))
                    {
                        indexFile.EdgeFiles.Add(edgeFileName);
                        DebugWriter.DebugWriteLine("#00SAV15#", $"Added {edgeFileName} to index");
                    }
                    else
                    {
                        DebugWriter.DebugWriteLine("#00SAV16#", $"{edgeFileName} already exists in index");
                    }
                }
                else
                {
                    indexFile.EdgeFiles.Remove(edgeFileName);
                    DebugWriter.DebugWriteLine("#00SAV17#", $"Removed {edgeFileName} from index");
                }

                SaveIndexFile(indexFilePath, indexFile);
                DebugWriter.DebugWriteLine("#00SAV18#", $"Saved updated index file: {indexFilePath}, new edge count: {indexFile.EdgeFiles.Count}");
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
                    int version = edgeData.Version;
                    EdgeBase edge;

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

                    edges.Add(edge.UpgradeToLatest());
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
            if (directoryPath != null && !Directory.Exists(directoryPath))
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

            DebugWriter.DebugWriteLine("#00LOD5#", $"Getting all edge files for node {nodeId}, outgoing: {outgoing}");
            DebugWriter.DebugWriteLine("#00LOD6#", $"Edge directory: {edgeDir}");

            if (!Directory.Exists(edgeDir))
            {
                DebugWriter.DebugWriteLine("#00LOD7#", $"Edge directory does not exist: {edgeDir}");
                return edgeFiles;
            }

            // Recursively search for index files
            SearchDirectoryForEdges(edgeDir, edgeFiles);

            DebugWriter.DebugWriteLine("#00LOD12#", $"Total edge files found: {edgeFiles.Count}");
            return edgeFiles;
        }

        private void SearchDirectoryForEdges(string directory, List<string> edgeFiles)
        {
            string indexFilePath = Path.Combine(directory, "index.json");
            DebugWriter.DebugWriteLine("#00LOD8#", $"Checking index file: {indexFilePath}");

            if (File.Exists(indexFilePath))
            {
                IndexFile indexFile = LoadIndexFile(indexFilePath);

                // Add edge files
                foreach (var fileName in indexFile.EdgeFiles)
                {
                    string fullPath = Path.Combine(directory, fileName);
                    edgeFiles.Add(fullPath);
                    DebugWriter.DebugWriteLine("#00LOD9#", $"Added edge file: {fullPath}");
                }
            }
            else
            {
                DebugWriter.DebugWriteLine("#00LOD11#", $"Index file not found: {indexFilePath}");
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
