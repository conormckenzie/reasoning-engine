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
            // This assumes we're using the IndexManager to keep track of all nodes
            return indexManager.GetNodeIds();
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
                DebugWriter.DebugWriteLine("#00SAV1#", $"Error saving node {nodeId}: {ex.Message}");
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
                    DebugWriter.DebugWriteLine("#00LOD1#", $"Node file {nodeFilePath} does not exist.");
                    return null;
                }

                string jsonData = File.ReadAllText(nodeFilePath);
                var node = JsonConvert.DeserializeObject<Node>(jsonData);

                return node;
            }
            catch (Exception ex)
            {
                DebugWriter.DebugWriteLine("#00LOD2#", $"Error loading node {nodeId}: {ex.Message}");
                return null;
            }
        }

        // Save an edge in both source and destination node directories
        public bool SaveEdge(Edge edge)
        {
            try
            {
                string edgeFilePath = GetEdgeFilePath(edge.FromNode, edge.ToNode);
                EnsureDirectoryExists(edgeFilePath);

                List<Edge> edges;
                if (File.Exists(edgeFilePath))
                {
                    string existingData = File.ReadAllText(edgeFilePath);
                    edges = JsonConvert.DeserializeObject<List<Edge>>(existingData) ?? new List<Edge>();
                    
                    // Remove the old edge if it exists
                    edges.RemoveAll(e => e.FromNode == edge.FromNode && e.ToNode == edge.ToNode);
                }
                else
                {
                    edges = new List<Edge>();
                }

                // Add the new edge
                edges.Add(edge);

                string edgeData = JsonConvert.SerializeObject(edges, Formatting.Indented);
                File.WriteAllText(edgeFilePath, edgeData);

                return true;
            }
            catch (Exception ex)
            {
                DebugWriter.DebugWriteLine("#00SAV2#", $"Error saving edge from {edge.FromNode} to {edge.ToNode}: {ex.Message}");
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
                    DebugWriter.DebugWriteLine("#00LOD3#", $"Node directory {nodeDir} does not exist.");
                    return edges;
                }

                string edgeFilePattern = nodeId.ToString("D16") + "-*.json";
                foreach (string filePath in Directory.EnumerateFiles(nodeDir, edgeFilePattern, SearchOption.AllDirectories))
                {
                    string edgeData = File.ReadAllText(filePath);
                    var edgesInFile = JsonConvert.DeserializeObject<List<Edge>>(edgeData);
                    if (edgesInFile != null)
                    {
                        edges.AddRange(edgesInFile);
                    }
                }
            }
            catch (Exception ex)
            {
                DebugWriter.DebugWriteLine("#00LOD4#", $"Error loading edges for node {nodeId}: {ex.Message}");
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
