using Newtonsoft.Json;
using DebugUtils;

namespace ReasoningEngine.GraphFileHandling
{
    public class GraphFileManager
    {
        private readonly string baseDir;

        public GraphFileManager(string baseDir)
        {
            this.baseDir = baseDir;
        }

        // Save a node without any edge data
        public bool SaveNode(long nodeId, string nodeContent)
        {
            try
            {
                string nodeFilePath = GetNodeFilePath(nodeId);
                EnsureDirectoryExists(nodeFilePath);

                var node = new Node(nodeId, nodeContent);
                string jsonData = JsonConvert.SerializeObject(node, Formatting.Indented);

                File.WriteAllText(nodeFilePath, jsonData);
                return true;
            }
            catch (Exception ex)
            {
                DebugWriter.DebugWriteLine("#SAV1#", $"Error saving node {nodeId}: {ex.Message}");
                return false;
            }
        }

        // Load a node without any edge data
        public Node? LoadNode(long nodeId)
        {
            try
            {
                string nodeFilePath = GetNodeFilePath(nodeId);

                if (!File.Exists(nodeFilePath))
                {
                    DebugWriter.DebugWriteLine("#LOD1#", $"Node file {nodeFilePath} does not exist.");
                    return null;
                }

                string jsonData = File.ReadAllText(nodeFilePath);
                var node = JsonConvert.DeserializeObject<Node>(jsonData);

                return node;
            }
            catch (Exception ex)
            {
                DebugWriter.DebugWriteLine("#LOD2#", $"Error loading node {nodeId}: {ex.Message}");
                return null;
            }
        }

        // Save an edge in both source and destination node directories
        public bool SaveEdge(Edge edge)
        {
            try
            {
                string sourceEdgeFilePath = GetEdgeFilePath(edge.FromNode, edge.ToNode);
                string destEdgeFilePath = GetEdgeFilePath(edge.ToNode, edge.FromNode);

                EnsureDirectoryExists(sourceEdgeFilePath);
                EnsureDirectoryExists(destEdgeFilePath);

                var edgeData = JsonConvert.SerializeObject(edge, Formatting.Indented);

                File.WriteAllText(sourceEdgeFilePath, edgeData);
                File.WriteAllText(destEdgeFilePath, edgeData);
                return true;
            }
            catch (Exception ex)
            {
                DebugWriter.DebugWriteLine("#SAV2#", $"Error saving edge from {edge.FromNode} to {edge.ToNode}: {ex.Message}");
                return false;
            }
        }

        // Load all edges for a given node (both incoming and outgoing)
        public List<Edge> LoadEdges(long nodeId)
        {
            var edges = new List<Edge>();

            try
            {
                string nodeDir = GetNodeDirPath(nodeId);
                if (!Directory.Exists(nodeDir))
                {
                    DebugWriter.DebugWriteLine("#LOD3#", $"Node directory {nodeDir} does not exist.");
                    return edges;
                }

                foreach (string filePath in Directory.EnumerateFiles(nodeDir, "*.json", SearchOption.AllDirectories))
                {
                    if (Path.GetFileName(filePath).StartsWith(nodeId.ToString("D16") + "-"))
                    {
                        string edgeData = File.ReadAllText(filePath);
                        var edge = JsonConvert.DeserializeObject<Edge>(edgeData);
                        if (edge != null)
                        {
                            edges.Add(edge);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DebugWriter.DebugWriteLine("#LOD4#", $"Error loading edges for node {nodeId}: {ex.Message}");
            }

            return edges;
        }

        // Ensure that the directory structure exists for the given file path
        private void EnsureDirectoryExists(string filePath)
        {
            string? directoryPath = Path.GetDirectoryName(filePath);
            if (directoryPath != null && !Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }

        // Get the file path for a node based on its ID
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

        // Get the directory path for a node based on its ID
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

        // Get the file path for an edge based on source and destination node IDs
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
