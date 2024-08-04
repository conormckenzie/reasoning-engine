using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using DebugUtils;

namespace GraphFileHandling
{
    public class GraphFileManager
    {
        private readonly string baseDir;

        public GraphFileManager(string baseDir)
        {
            this.baseDir = baseDir;
        }

        /// <summary>
        /// Saves a node and its edges to a file.
        /// </summary>
        /// <param name="nodeId">The ID of the node to save.</param>
        /// <param name="nodeContent">The content of the node.</param>
        /// <param name="edges">The list of edges connected to the node.</param>
        /// <returns>True if the operation is successful, false otherwise.</returns>
        public bool SaveNode(long nodeId, string nodeContent, List<Edge> edges)
        {
            try
            {
                string nodeFileName = GetNodeFileName(nodeId);
                string nodeFilePath = Path.Combine(baseDir, nodeFileName);

                var node = new Node(nodeId, nodeContent);
                var nodeData = new NodeData(node, edges);
                string jsonData = JsonConvert.SerializeObject(nodeData, Formatting.Indented);

                File.WriteAllText(nodeFilePath, jsonData);
                return true;
            }
            catch (Exception ex)
            {
                DebugWriter.DebugWriteLine("#SAV1#", $"Error saving node {nodeId}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Loads a node and its edges from a file.
        /// </summary>
        /// <param name="nodeId">The ID of the node to load.</param>
        /// <returns>A tuple containing the success status, nodeId, nodeContent, and edges.</returns>
        public (bool, long, string, List<Edge>) LoadNode(long nodeId)
        {
            try
            {
                string nodeFileName = GetNodeFileName(nodeId);
                string nodeFilePath = Path.Combine(baseDir, nodeFileName);

                if (!File.Exists(nodeFilePath))
                {
                    DebugWriter.DebugWriteLine("#LOD1#", $"Node file {nodeFilePath} does not exist.");
                    return (false, nodeId, string.Empty, new List<Edge>());
                }

                string jsonData = File.ReadAllText(nodeFilePath);
                var nodeData = JsonConvert.DeserializeObject<NodeData>(jsonData);

                return (true, nodeData.Node.Id, nodeData.Node.Content, nodeData.Edges);
            }
            catch (Exception ex)
            {
                DebugWriter.DebugWriteLine("#LOD2#", $"Error loading node {nodeId}: {ex.Message}");
                return (false, nodeId, string.Empty, new List<Edge>());
            }
        }

        /// <summary>
        /// Handles user input for saving a node and returns the newly created nodes and edges.
        /// </summary>
        public (List<Node>, List<Edge>) SaveNodeWithUserInput()
        {
            var newlyCreatedNodes = new List<Node>();
            var newlyCreatedEdges = new List<Edge>();

            string saveOption = GetSaveOptionFromUser();
            if (saveOption == "1")
            {
                var (nodes, edges) = SaveNodeWithManualEntry();
                newlyCreatedNodes.AddRange(nodes);
                newlyCreatedEdges.AddRange(edges);
            }
            else if (saveOption == "2")
            {
                var (nodes, edges) = SaveNodesFromChangesList();
                newlyCreatedNodes.AddRange(nodes);
                newlyCreatedEdges.AddRange(edges);
            }
            else
            {
                DebugWriter.DebugWriteLine("#A1B2#", "Invalid save option. Please try again.");
            }

            return (newlyCreatedNodes, newlyCreatedEdges);
        }

        /// <summary>
        /// Handles user input for loading a node.
        /// </summary>
        public void LoadNodeWithUserInput()
        {
            DebugWriter.DebugWriteLine("#C3D4#", "Enter node ID: ");
            if (long.TryParse(Console.ReadLine(), out long nodeId))
            {
                var result = LoadNode(nodeId);
                if (result.Item1)
                {
                    DisplayLoadedNode(result);
                }
            }
            else
            {
                DebugWriter.DebugWriteLine("#E5F6#", "Invalid node ID.");
            }
        }

        private string GetSaveOptionFromUser()
        {
            DebugWriter.DebugWriteLine("#G7H8#", "Select save option:");
            DebugWriter.DebugWriteLine("#I9J0#", "1. Manual entry");
            DebugWriter.DebugWriteLine("#K1L2#", "2. Save from changes list");
            DebugWriter.DebugWriteLine("#M3N4#", "Enter option: ");
            return Console.ReadLine()?.ToLower() ?? string.Empty;
        }

        private (List<Node>, List<Edge>) SaveNodeWithManualEntry()
        {
            var newlyCreatedNodes = new List<Node>();
            var newlyCreatedEdges = new List<Edge>();

            DebugWriter.DebugWriteLine("#O5P6#", "Enter node ID: ");
            if (long.TryParse(Console.ReadLine(), out long nodeId))
            {
                DebugWriter.DebugWriteLine("#Q7R8#", "Enter node content: ");
                string nodeContent = Console.ReadLine() ?? string.Empty;

                var edges = GetEdgesFromUserInput();

                if (SaveNode(nodeId, nodeContent, edges))
                {
                    DebugWriter.DebugWriteLine("#S9T0#", $"Node {nodeId} saved successfully.");
                    var node = new Node(nodeId, nodeContent);
                    newlyCreatedNodes.Add(node);
                    newlyCreatedEdges.AddRange(edges);
                }
            }
            else
            {
                DebugWriter.DebugWriteLine("#U1V2#", "Invalid node ID.");
            }

            return (newlyCreatedNodes, newlyCreatedEdges);
        }

        private List<Edge> GetEdgesFromUserInput()
        {
            var newlyCreatedEdges = new List<Edge>();
            while (true)
            {
                DebugWriter.DebugWriteLine("#W3X4#", $"{newlyCreatedEdges.Count} edges added. Add an{(
                    newlyCreatedEdges.Count > 0 ? "other" : "")} edge? (y/n): ");
                if (Console.ReadLine()?.ToLower() != "y")
                    break;

                long fromNodeId = GetLongFromUser("#Y5Z6#", "Enter from node ID: ");
                long toNodeId = GetLongFromUser("#A7B8#", "Enter to node ID: ");
                double weight = GetDoubleFromUser("#C9D0#", "Enter edge weight: ");
                DebugWriter.DebugWriteLine("#E1F2#", "Enter edge content: ");
                string edgeContent = Console.ReadLine() ?? string.Empty;

                newlyCreatedEdges.Add(new Edge(fromNodeId, toNodeId, weight, edgeContent));
                DebugWriter.DebugWriteLine("#d3X4#", "Edge created successfully");
            }
            return newlyCreatedEdges;
        }

        private long GetLongFromUser(string debugCode, string prompt)
        {
            while (true)
            {
                DebugWriter.DebugWriteLine(debugCode, prompt);
                if (long.TryParse(Console.ReadLine(), out long result))
                {
                    return result;
                }
                DebugWriter.DebugWriteLine("#G3H4#", "Invalid input. Please enter a valid number.");
            }
        }

        private double GetDoubleFromUser(string debugCode, string prompt)
        {
            while (true)
            {
                DebugWriter.DebugWriteLine(debugCode, prompt);
                if (double.TryParse(Console.ReadLine(), out double result))
                {
                    return result;
                }
                DebugWriter.DebugWriteLine("#I5J6#", "Invalid input. Please enter a valid number.");
            }
        }

        private (List<Node>, List<Edge>) SaveNodesFromChangesList()
        {
            var newlyCreatedNodes = new List<Node>();
            var newlyCreatedEdges = new List<Edge>();

            foreach (var node in Program.nodeChanges)
            {
                var edgesToSave = Program.edgeChanges.FindAll(e => e.FromNode == node.Id || e.ToNode == node.Id);
                if (SaveNode(node.Id, node.Content, edgesToSave))
                {
                    DebugWriter.DebugWriteLine("#K7L8#", $"Node {node.Id} saved successfully from changes list.");
                    newlyCreatedNodes.Add(node);
                    newlyCreatedEdges.AddRange(edgesToSave);
                }
                else
                {
                    DebugWriter.DebugWriteLine("#M9N0#", $"Failed to save node {node.Id} from changes list.");
                }
            }

            return (newlyCreatedNodes, newlyCreatedEdges);
        }

        private void DisplayLoadedNode((bool, long, string, List<Edge>) result)
        {
            DebugWriter.DebugWriteLine("#O1P2#", $"Node ID: {result.Item2}");
            DebugWriter.DebugWriteLine("#Q3R4#", $"Node Content: {result.Item3}");
            DebugWriter.DebugWriteLine("#S5T6#", $"Edges: {JsonConvert.SerializeObject(result.Item4, Formatting.Indented)}");
        }

        private string GetNodeFileName(long nodeId)
        {
            return nodeId.ToString("D16") + ".json";
        }
    }

    class NodeData
    {
        public Node Node { get; set; }
        public List<Edge> Edges { get; set; }

        public NodeData(Node node, List<Edge> edges)
        {
            Node = node;
            Edges = edges;
        }
    }
}
