using System;
using System.Collections.Generic;
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

        public bool SaveNode(long nodeId, string nodeContent, List<Edge> edges)
        {
            // Implement the logic to save the node and its edges to a file.
            // Return true if the operation is successful, false otherwise.
            return true;
        }

        public (bool, long, string, List<Edge>) LoadNode(long nodeId)
        {
            // Implement the logic to load the node and its edges from a file.
            // Return a tuple with the success status, nodeId, nodeContent, and edges.
            return (true, nodeId, "Node Content", new List<Edge>());
        }

        public void SaveNodeWithUserInput()
        {
            DebugWriter.DebugWriteLine("#D7D8#", "Select save option:");
            DebugWriter.DebugWriteLine("#D7D9#", "1. Manual entry");
            DebugWriter.DebugWriteLine("#D7DA#", "2. Save from changes list");
            DebugWriter.DebugWriteLine("#D7DB#", "Enter option: ");
            var saveOption = Console.ReadLine();

            if (saveOption == "1")
            {
                DebugWriter.DebugWriteLine("#D7DC#", "Enter node ID: ");
                if (long.TryParse(Console.ReadLine(), out long nodeId))
                {
                    DebugWriter.DebugWriteLine("#D7DD#", "Enter node content: ");
                    string nodeContent = Console.ReadLine();

                    var edges = new List<Edge>();
                    while (true)
                    {
                        DebugWriter.DebugWriteLine("#D7DE#", "Add an edge? (yes/no): ");
                        if (Console.ReadLine().ToLower() != "yes")
                            break;

                        DebugWriter.DebugWriteLine("#D7DF#", "Enter from node ID: ");
                        long fromNodeId = long.Parse(Console.ReadLine());
                        DebugWriter.DebugWriteLine("#D7E0#", "Enter to node ID: ");
                        long toNodeId = long.Parse(Console.ReadLine());
                        DebugWriter.DebugWriteLine("#D7E1#", "Enter edge weight: ");
                        double weight = double.Parse(Console.ReadLine());
                        DebugWriter.DebugWriteLine("#D7E2#", "Enter edge content: ");
                        string edgeContent = Console.ReadLine();

                        edges.Add(new Edge { FromNode = fromNodeId, ToNode = toNodeId, Weight = weight, EdgeContent = edgeContent });
                    }

                    if (SaveNode(nodeId, nodeContent, edges))
                    {
                        DebugWriter.DebugWriteLine("#D7E3#", $"Node {nodeId} saved successfully.");
                        // Add to changes list
                        Program.nodeChanges.Add(new Node { Id = nodeId, Content = nodeContent });
                        Program.edgeChanges.AddRange(edges);
                    }
                }
                else
                {
                    DebugWriter.DebugWriteLine("#D7E4#", "Invalid node ID.");
                }
            }
            else if (saveOption == "2")
            {
                foreach (var node in Program.nodeChanges)
                {
                    if (SaveNode(node.Id, node.Content, Program.edgeChanges.FindAll(e => e.FromNode == node.Id || e.ToNode == node.Id)))
                    {
                        DebugWriter.DebugWriteLine("#D7E5#", $"Node {node.Id} saved successfully from changes list.");
                    }
                    else
                    {
                        DebugWriter.DebugWriteLine("#D7E6#", $"Failed to save node {node.Id} from changes list.");
                    }
                }
            }
            else
            {
                DebugWriter.DebugWriteLine("#D7E7#", "Invalid save option. Please try again.");
            }
        }

        public void LoadNodeWithUserInput()
        {
            DebugWriter.DebugWriteLine("#D7E8#", "Enter node ID: ");
            if (long.TryParse(Console.ReadLine(), out long nodeId))
            {
                var result = LoadNode(nodeId);
                if (result.Item1)
                {
                    DebugWriter.DebugWriteLine("#D7E9#", $"Node ID: {result.Item2}");
                    DebugWriter.DebugWriteLine("#D7EA#", $"Node Content: {result.Item3}");
                    DebugWriter.DebugWriteLine("#D7EB#", $"Edges: {JsonConvert.SerializeObject(result.Item4, Formatting.Indented)}");
                }
            }
            else
            {
                DebugWriter.DebugWriteLine("#D7EC#", "Invalid node ID.");
            }
        }
    }
}
