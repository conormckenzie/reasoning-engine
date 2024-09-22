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
                Console.WriteLine($"Attempting to delete node: {nodeId}");
                Console.WriteLine($"Node file path: {nodeFilePath}");

                if (File.Exists(nodeFilePath))
                {
                    // Delete the node file
                    File.Delete(nodeFilePath);
                    Console.WriteLine("Node file deleted");

                    // Delete outgoing edges
                    string outgoingEdgeDir = GetEdgeDirPath(nodeId, true);
                    Console.WriteLine($"Outgoing edge directory: {outgoingEdgeDir}");
                    if (Directory.Exists(outgoingEdgeDir))
                    {
                        Directory.Delete(outgoingEdgeDir, true);
                        Console.WriteLine("Outgoing edge directory deleted");
                    }
                    else
                    {
                        Console.WriteLine("Outgoing edge directory does not exist");
                    }

                    // Delete incoming edges
                    string incomingEdgeDir = GetEdgeDirPath(nodeId, false);
                    Console.WriteLine($"Incoming edge directory: {incomingEdgeDir}");
                    if (Directory.Exists(incomingEdgeDir))
                    {
                        Directory.Delete(incomingEdgeDir, true);
                        Console.WriteLine("Incoming edge directory deleted");
                    }
                    else
                    {
                        Console.WriteLine("Incoming edge directory does not exist");
                    }

                    // Remove incoming edges from other nodes
                    var allNodeIds = GetAllNodeIds();
                    Console.WriteLine($"Total nodes to check for incoming edges: {allNodeIds.Count}");
                    foreach (var otherNodeId in allNodeIds)
                    {
                        if (otherNodeId != nodeId)
                        {
                            Console.WriteLine($"Checking for edge from {otherNodeId} to {nodeId}");
                            DeleteEdge(otherNodeId, nodeId);
                        }
                    }

                    // Remove the node from the index
                    indexManager.RemoveNode(nodeId);
                    Console.WriteLine("Node removed from index");

                    return true;
                }
                DebugWriter.DebugWriteLine("#00DEL1#", $"Node file {nodeFilePath} does not exist.");
                Console.WriteLine($"Node file {nodeFilePath} does not exist.");
                return false;
            }
            catch (Exception ex)
            {
                DebugWriter.DebugWriteLine("#00DEL2#", $"Error deleting node {nodeId}: {ex.Message}");
                Console.WriteLine($"Error deleting node {nodeId}: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return false;
            }
        }

        private void UpdateEdgeCountsForConnectedNodes(long deletedNodeId)
        {
            // Update counts for nodes that had outgoing edges to the deleted node
            string incomingEdgeDir = GetEdgeDirPath(deletedNodeId, false);
            if (Directory.Exists(incomingEdgeDir))
            {
                foreach (var filePath in Directory.EnumerateFiles(incomingEdgeDir, "*.json", SearchOption.AllDirectories))
                {
                    string fileName = Path.GetFileNameWithoutExtension(filePath);
                    string[] parts = fileName.Split('-');
                    if (parts.Length == 2 && long.TryParse(parts[0], out long connectedNodeId))
                    {
                        indexManager.AddOrUpdateNode(connectedNodeId, GetNodeFilePath(connectedNodeId), GetEdgeCount(connectedNodeId, true));
                    }
                }
            }
        }

        public bool SaveEdge(EdgeBase edge)
        {
            try
            {
                // Save outgoing edge
                string outgoingEdgeFilePath = GetEdgeFilePath(edge.FromNode, edge.ToNode, true);
                SaveEdgeToFile(edge, outgoingEdgeFilePath);

                // Save incoming edge
                string incomingEdgeFilePath = GetEdgeFilePath(edge.ToNode, edge.FromNode, false);
                SaveEdgeToFile(edge, incomingEdgeFilePath);

                // Update edge count for both nodes
                indexManager.AddOrUpdateNode(edge.FromNode, GetNodeFilePath(edge.FromNode), GetEdgeCount(edge.FromNode, true));
                indexManager.AddOrUpdateNode(edge.ToNode, GetNodeFilePath(edge.ToNode), GetEdgeCount(edge.ToNode, false));

                return true;
            }
            catch (Exception ex)
            {
                DebugWriter.DebugWriteLine("#00SAV2#", $"Error saving edge from {edge.FromNode} to {edge.ToNode}: {ex.Message}");
                return false;
            }
        }

        private void SaveEdgeToFile(EdgeBase edge, string filePath)
        {
            EnsureDirectoryExists(filePath);
            var edgeData = new
            {
                Version = edge.Version,
                Edge = edge
            };
            string jsonData = JsonConvert.SerializeObject(edgeData, Formatting.Indented);
            File.WriteAllText(filePath, jsonData);
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

                foreach (string filePath in Directory.EnumerateFiles(edgeDir, "*.json", SearchOption.AllDirectories))
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
                Console.WriteLine($"Attempting to delete edge: {fromNodeId} -> {toNodeId}");
                
                // Delete from outgoing edges
                string outgoingEdgeFilePath = GetEdgeFilePath(fromNodeId, toNodeId, true);
                Console.WriteLine($"Outgoing edge file path: {outgoingEdgeFilePath}");
                if (File.Exists(outgoingEdgeFilePath))
                {
                    File.Delete(outgoingEdgeFilePath);
                    Console.WriteLine("Outgoing edge file deleted");
                }
                else
                {
                    Console.WriteLine("Outgoing edge file does not exist");
                }

                // Delete from incoming edges
                string incomingEdgeFilePath = GetEdgeFilePath(toNodeId, fromNodeId, false);
                Console.WriteLine($"Incoming edge file path: {incomingEdgeFilePath}");
                if (File.Exists(incomingEdgeFilePath))
                {
                    File.Delete(incomingEdgeFilePath);
                    Console.WriteLine("Incoming edge file deleted");
                }
                else
                {
                    Console.WriteLine("Incoming edge file does not exist");
                }

                // Update edge count for both nodes
                UpdateNodeEdgeCount(fromNodeId, true);
                UpdateNodeEdgeCount(toNodeId, false);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting edge from {fromNodeId} to {toNodeId}: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return false;
            }
        }

        private void UpdateNodeEdgeCount(long nodeId, bool outgoing)
        {
            string edgeDir = GetEdgeDirPath(nodeId, outgoing);
            int edgeCount = Directory.Exists(edgeDir) 
                ? Directory.EnumerateFiles(edgeDir, "*.json", SearchOption.AllDirectories).Count() 
                : 0;
            Console.WriteLine($"Updating {(outgoing ? "outgoing" : "incoming")} edge count for node {nodeId}: {edgeCount}");
            indexManager.AddOrUpdateNode(nodeId, GetNodeFilePath(nodeId), edgeCount);
        }

        public List<EdgeBase> FindEdgesByDestinationNode(long destinationNodeId)
        {
            return LoadEdges(destinationNodeId, false);
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

        private string GetEdgeFilePath(long fromNodeId, long toNodeId, bool outgoing)
        {
            string direction = outgoing ? "outgoing" : "incoming";
            string fromNodeIdStr = fromNodeId.ToString("D16");
            string toNodeIdStr = toNodeId.ToString("D16");
            string[] pathSegments = new string[8];
            pathSegments[0] = "edges";
            pathSegments[1] = direction;
            pathSegments[2] = fromNodeIdStr.Substring(0, 4);
            pathSegments[3] = fromNodeIdStr.Substring(0, 8);
            pathSegments[4] = fromNodeIdStr.Substring(0, 12);
            pathSegments[5] = fromNodeIdStr;
            pathSegments[6] = $"{fromNodeIdStr}-{toNodeIdStr.Substring(0, 4)}";
            pathSegments[7] = $"{fromNodeIdStr}-{toNodeIdStr}.json";
            return Path.Combine(baseDir, Path.Combine(pathSegments));
        }

        private string GetEdgeDirPath(long nodeId, bool outgoing)
        {
            string direction = outgoing ? "outgoing" : "incoming";
            string nodeIdStr = nodeId.ToString("D16");
            string[] pathSegments = new string[6];
            pathSegments[0] = "edges";
            pathSegments[1] = direction;
            pathSegments[2] = nodeIdStr.Substring(0, 4);
            pathSegments[3] = nodeIdStr.Substring(0, 8);
            pathSegments[4] = nodeIdStr.Substring(0, 12);
            pathSegments[5] = nodeIdStr;
            return Path.Combine(baseDir, Path.Combine(pathSegments));
        }

        private int GetEdgeCount(long nodeId, bool outgoing)
        {
            string edgeDir = GetEdgeDirPath(nodeId, outgoing);
            return Directory.Exists(edgeDir) 
                ? Directory.EnumerateFiles(edgeDir, "*.json", SearchOption.AllDirectories).Count() 
                : 0;
        }
    }
}