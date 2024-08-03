using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GraphFileHandling
{
    public class GraphFileManager
    {
        private readonly string baseDir;
        private IndexManager indexManager;
        private const int WarningEdgeCount = 5000;
        private const int MaxEdgeCount = 10000;
        private const int WarningTotalCount = 50000;
        private const int MaxTotalCount = 100000;

        // Constructor: initializes the base directory where graph files are stored
        public GraphFileManager(string baseDir)
        {
            this.baseDir = baseDir;
            this.indexManager = new IndexManager(Path.Combine(baseDir, "index.json"));
        }

        public List<long> GetNodeIds() => indexManager.GetNodeIds();

        public int GetTotalNodes() => indexManager.GetTotalNodes();

        public int GetTotalEdges() => indexManager.GetTotalEdges();

        // Method to generate the file path for a given node ID
        private string GetNodePath(long nodeId)
        {
            // Convert the node ID to a 16-digit string with leading zeros
            string nodeStr = nodeId.ToString("D16");
            
            // Determine the hierarchical folder structure
            string firstLayer = nodeStr.Substring(0, 4);
            string secondLayer = nodeStr.Substring(0, 8);
            string thirdLayer = nodeStr.Substring(0, 12);
            
            // Generate the file name
            string fileName = $"{nodeStr}.node";
            
            // Combine the base directory and hierarchical folder structure to get the full path
            return Path.Combine(baseDir, firstLayer, secondLayer, thirdLayer, fileName);
        }

        // Method to save a node's data to a file
        public bool SaveNode(long nodeId, string nodeContent, List<Edge> edges)
        {
            // Check if the number of edges exceeds the maximum allowed count
            if (edges.Count > MaxEdgeCount || edges.Count + 1 > MaxTotalCount)
            {
                Console.WriteLine($"Failed to save node {nodeId}: Exceeded maximum count of edges or total nodes + edges.");
                return false;
            }
            
            // Log a warning if the number of edges exceeds the warning count
            if (edges.Count > WarningEdgeCount)
            {
                Console.WriteLine($"Warning: Node {nodeId} has {edges.Count} edges, which exceeds the warning limit of {WarningEdgeCount}.");
            }

            // Get the file path for the node
            string nodePath = GetNodePath(nodeId);
            
            // Ensure the directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(nodePath));

            // Create a JSON object to hold the node data
            var data = new JObject
            {
                ["nodeID"] = nodeId,
                ["nodeContent"] = nodeContent,
                ["edges"] = JArray.FromObject(edges)
            };

            // Try to write the JSON data to the file
            try
            {
                File.WriteAllText(nodePath, data.ToString(Formatting.Indented));
                indexManager.AddOrUpdateNode(nodeId, nodePath, edges.Count);
            }
            catch (Exception ex)
            {
                // Log an error if the file write fails
                Console.WriteLine($"Failed to save node {nodeId}: {ex.Message}");
                return false;
            }

            return true;
        }

        // Method to load a node's data from a file
        public (bool, long, string, List<Edge>) LoadNode(long nodeId)
        {
            // Get the file path for the node
            string nodePath = GetNodePath(nodeId);

            // Check if the file exists
            if (!File.Exists(nodePath))
            {
                Console.WriteLine($"File not found: {nodePath}");
                return (false, nodeId, null, null);
            }

            // Try to read and parse the JSON data from the file
            JObject data;
            try
            {
                data = JObject.Parse(File.ReadAllText(nodePath));
            }
            catch (Exception ex)
            {
                // Log an error if the file read fails
                Console.WriteLine($"Failed to read node {nodeId}: {ex.Message}");
                return (false, nodeId, null, null);
            }

            // Extract the node data from the JSON object
            long loadedNodeId = data["nodeID"].Value<long>();
            string nodeContent = data["nodeContent"].Value<string>();
            var edges = data["edges"].ToObject<List<Edge>>();

            // Check if the number of edges exceeds the maximum allowed count
            if (edges.Count > MaxEdgeCount || edges.Count + 1 > MaxTotalCount)
            {
                Console.WriteLine($"Failed to load node {nodeId}: Exceeded maximum count of edges or total nodes + edges.");
                return (false, nodeId, null, null);
            }
            
            // Log a warning if the number of edges exceeds the warning count
            if (edges.Count > WarningEdgeCount)
            {
                Console.WriteLine($"Warning: Node {nodeId} has {edges.Count} edges, which exceeds the warning limit of {WarningEdgeCount}.");
            }

            return (true, loadedNodeId, nodeContent, edges);
        }

        
    }

    
    // Example usage of the GraphFileManager class
    public class SaveLoadTest
    {
        public static void RunTest(GraphFileManager manager, bool hidden = true)
        {
            // Example data
            long nodeId = 0;
            string nodeContent = "Node 0 content";
            var edges = new List<Edge>
            {
                new Edge { FromNode = 0, ToNode = 1, Weight = 0.9, EdgeContent = "edge to node 2" },
                new Edge { FromNode = 2, ToNode = 0, Weight = -0.3, EdgeContent = "edge from node 3" }
            };

            // Save the node data to a file
            if (manager.SaveNode(nodeId, nodeContent, edges))
            {
                if (!hidden)
                {
                    Console.WriteLine($"Successfully saved node {nodeId}.");
                }
            }

            // Load the node data from the file
            var result = manager.LoadNode(nodeId);
            if (result.Item1)
            {
                Console.WriteLine($"Node ID: {result.Item2}");
                Console.WriteLine($"Node Content: {result.Item3}");
                Console.WriteLine($"Edges: {JsonConvert.SerializeObject(result.Item4, Formatting.Indented)}");
            }
        }
    }
}