using Newtonsoft.Json;
using DebugUtils;

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
                indexManager.AddOrUpdateNode(node.Id, nodeFilePath, 0); // Assuming 0 edges initially
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
                    File.Delete(nodeFilePath);
                    indexManager.RemoveNode(nodeId);

                    // Delete all edges associated with this node
                    string nodeDir = GetNodeDirPath(nodeId);
                    if (Directory.Exists(nodeDir))
                    {
                        Directory.Delete(nodeDir, true);
                    }

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

        public bool SaveEdge(EdgeBase edge)
        {
            try
            {
                string edgeFilePath = GetEdgeFilePath(edge.FromNode, edge.ToNode);
                EnsureDirectoryExists(edgeFilePath);

                List<dynamic> edges;
                if (File.Exists(edgeFilePath))
                {
                    string existingData = File.ReadAllText(edgeFilePath);
                    edges = JsonConvert.DeserializeObject<List<dynamic>>(existingData) ?? new List<dynamic>();
                    
                    // Remove the old edge if it exists
                    edges.RemoveAll(e => (long)e.Edge.FromNode == edge.FromNode && (long)e.Edge.ToNode == edge.ToNode);
                }
                else
                {
                    edges = new List<dynamic>();
                }

                // Add the new edge
                edges.Add(new
                {
                    Version = edge.Version,
                    Edge = edge
                });

                string edgeData = JsonConvert.SerializeObject(edges, Formatting.Indented);
                File.WriteAllText(edgeFilePath, edgeData);

                // Update edge count for the source node
                indexManager.AddOrUpdateNode(edge.FromNode, GetNodeFilePath(edge.FromNode), edges.Count);

                return true;
            }
            catch (Exception ex)
            {
                DebugWriter.DebugWriteLine("#00SAV2#", $"Error saving edge from {edge.FromNode} to {edge.ToNode}: {ex.Message}");
                return false;
            }
        }

        public List<EdgeBase> LoadEdges(long nodeId)
        {
            var edges = new List<EdgeBase>();

            try
            {
                string nodeDir = GetNodeDirPath(nodeId);
                if (!Directory.Exists(nodeDir))
                {
                    DebugWriter.DebugWriteLine("#00LOD3#", $"Node directory {nodeDir} does not exist.");
                    return edges;
                }

                string edgeFilePattern = nodeId.ToString("D16") + "-*.json";
                foreach (string filePath in Directory.EnumerateFiles(nodeDir, edgeFilePattern, SearchOption.AllDirectories))
                {
                    string fileContent = File.ReadAllText(filePath);
                    var edgesInFile = JsonConvert.DeserializeObject<List<dynamic>>(fileContent);
                    if (edgesInFile != null)
                    {
                        foreach (var edgeItem in edgesInFile)
                        {
                            int version = edgeItem.Version;
                            EdgeBase edge;

                            switch (version)
                            {
                                case 1:
                                    edge = JsonConvert.DeserializeObject<EdgeV1>(edgeItem.Edge.ToString());
                                    break;
                                case 2:
                                    edge = JsonConvert.DeserializeObject<EdgeV2>(edgeItem.Edge.ToString());
                                    break;
                                default:
                                    throw new NotSupportedException($"Edge version {version} is not supported.");
                            }

                            edges.Add(edge.UpgradeToLatest());
                        }
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
                string edgeFilePath = GetEdgeFilePath(fromNodeId, toNodeId);
                if (File.Exists(edgeFilePath))
                {
                    string existingData = File.ReadAllText(edgeFilePath);
                    var edges = JsonConvert.DeserializeObject<List<dynamic>>(existingData) ?? new List<dynamic>();

                    int initialCount = edges.Count;
                    edges.RemoveAll(e => (long)e.Edge.FromNode == fromNodeId && (long)e.Edge.ToNode == toNodeId);

                    if (edges.Count < initialCount)
                    {
                        if (edges.Count == 0)
                        {
                            // If no edges left, delete the file
                            File.Delete(edgeFilePath);
                        }
                        else
                        {
                            // Otherwise, save the updated edge list
                            string edgeData = JsonConvert.SerializeObject(edges, Formatting.Indented);
                            File.WriteAllText(edgeFilePath, edgeData);
                        }

                        // Update edge count for the source node
                        indexManager.AddOrUpdateNode(fromNodeId, GetNodeFilePath(fromNodeId), edges.Count);

                        return true;
                    }
                    DebugWriter.DebugWriteLine("#00DEL3#", $"Edge from {fromNodeId} to {toNodeId} not found in file {edgeFilePath}.");
                    return false;
                }
                DebugWriter.DebugWriteLine("#00DEL4#", $"Edge file {edgeFilePath} does not exist.");
                return false;
            }
            catch (Exception ex)
            {
                DebugWriter.DebugWriteLine("#00DEL5#", $"Error deleting edge from {fromNodeId} to {toNodeId}: {ex.Message}");
                return false;
            }
        }

        public List<EdgeBase> FindEdgesByDestinationNode(long destinationNodeId)
        {
            var result = new List<EdgeBase>();
            var allNodeIds = indexManager.GetNodeIds();

            foreach (var nodeId in allNodeIds)
            {
                var edges = LoadEdges(nodeId);
                result.AddRange(edges.Where(e => e.ToNode == destinationNodeId));
            }

            return result;
        }

        private void EnsureDirectoryExists(string filePath)
        {
            string? directoryPath = Path.GetDirectoryName(filePath);
            if (directoryPath != null && !Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }

        private string GetNodeFilePath(long nodeId)
        {
            string nodeIdStr = nodeId.ToString("D16");
            string[] pathSegments = new string[4];
            pathSegments[0] = nodeIdStr.Substring(0, 4);
            pathSegments[1] = nodeIdStr.Substring(0, 8);
            pathSegments[2] = nodeIdStr.Substring(0, 12);
            pathSegments[3] = nodeIdStr + "/" + nodeIdStr + ".json";
            return Path.Combine(baseDir, Path.Combine(pathSegments));
        }

        private string GetNodeDirPath(long nodeId)
        {
            string nodeIdStr = nodeId.ToString("D16");
            string[] pathSegments = new string[4];
            pathSegments[0] = nodeIdStr.Substring(0, 4);
            pathSegments[1] = nodeIdStr.Substring(0, 8);
            pathSegments[2] = nodeIdStr.Substring(0, 12);
            pathSegments[3] = nodeIdStr;
            return Path.Combine(baseDir, Path.Combine(pathSegments));
        }

        private string GetEdgeFilePath(long fromNodeId, long toNodeId)
        {
            string fromNodeIdStr = fromNodeId.ToString("D16");
            string toNodeIdStr = toNodeId.ToString("D16");
            string[] pathSegments = new string[6];
            pathSegments[0] = fromNodeIdStr.Substring(0, 4);
            pathSegments[1] = fromNodeIdStr.Substring(0, 8);
            pathSegments[2] = fromNodeIdStr.Substring(0, 12);
            pathSegments[3] = fromNodeIdStr;
            pathSegments[4] = fromNodeIdStr + "-" + toNodeIdStr.Substring(0, 4);
            pathSegments[5] = fromNodeIdStr + "-" + toNodeIdStr.Substring(0, 8) + "/" + fromNodeIdStr + "-" + toNodeIdStr.Substring(0, 12) + ".json";
            return Path.Combine(baseDir, Path.Combine(pathSegments));
        }
    }
}