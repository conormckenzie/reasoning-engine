using System;
using System.Collections.Generic;
using ReasoningEngine.GraphFileHandling;
using DebugUtils;

namespace ReasoningEngine.GraphAccess
{
    public class GraphAccessUserMenu
    {
        static void SaveNodeWithUserInput(GraphFileManager manager)
        {
            Console.Write("Enter node ID: ");
            if (!long.TryParse(Console.ReadLine(), out long nodeId))
            {
                DebugWriter.DebugWriteLine("#00SNE1#", "Invalid node ID. Please enter a valid integer.");
                return;
            }

            Console.Write("Enter node content: ");
            string? nodeContent = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(nodeContent))
            {
                DebugWriter.DebugWriteLine("#00SNE2#", "Node content cannot be empty.");
                return;
            }

            var node = new Node(nodeId, nodeContent);
            bool success = manager.SaveNode(node);
            if (success)
            {
                DebugWriter.DebugWriteLine("#00SNS1#", $"Node {nodeId} saved successfully.");
            }
            else
            {
                DebugWriter.DebugWriteLine("#00SNE3#", $"Failed to save node {nodeId}.");
            }
        }

        static void LoadNodeWithUserInput(GraphFileManager manager)
        {
            Console.Write("Enter node ID to load: ");
            if (!long.TryParse(Console.ReadLine(), out long nodeId))
            {
                DebugWriter.DebugWriteLine("#00LNE1#", "Invalid node ID. Please enter a valid integer.");
                return;
            }

            NodeBase? node = manager.LoadNode(nodeId);
            if (node != null)
            {
                DebugWriter.DebugWriteLine("#00LNS1#", $"Node {nodeId} loaded successfully.");
                DebugWriter.DebugWriteLine("#00LNS2#", $"Content: {(node as dynamic).Content}");
            }
            else
            {
                DebugWriter.DebugWriteLine("#00LNE2#", $"Failed to load node {nodeId}. The node may not exist.");
            }
        }

        static void SaveEdgeWithUserInput(GraphFileManager manager)
        {
            Console.Write("Enter source node ID: ");
            if (!long.TryParse(Console.ReadLine(), out long fromNodeId))
            {
                DebugWriter.DebugWriteLine("#00SEE1#", "Invalid source node ID. Please enter a valid integer.");
                return;
            }

            Console.Write("Enter destination node ID: ");
            if (!long.TryParse(Console.ReadLine(), out long toNodeId))
            {
                DebugWriter.DebugWriteLine("#00SEE2#", "Invalid destination node ID. Please enter a valid integer.");
                return;
            }

            Console.Write("Enter edge weight: ");
            if (!double.TryParse(Console.ReadLine(), out double weight))
            {
                DebugWriter.DebugWriteLine("#00SEE3#", "Invalid weight. Please enter a valid number.");
                return;
            }

            Console.Write("Enter edge content: ");
            string? edgeContent = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(edgeContent))
            {
                DebugWriter.DebugWriteLine("#00SEE4#", "Edge content cannot be empty.");
                return;
            }

            var edge = new Edge(fromNodeId, toNodeId, weight, edgeContent);
            bool success = manager.SaveEdge(edge);
            if (success)
            {
                DebugWriter.DebugWriteLine("#00SES1#", $"Edge from {fromNodeId} to {toNodeId} saved successfully.");
            }
            else
            {
                DebugWriter.DebugWriteLine("#00SEE5#", $"Failed to save edge from {fromNodeId} to {toNodeId}.");
            }
        }

        static void LoadEdgesWithUserInput(GraphFileManager manager)
        {
            Console.Write("Enter node ID to load edges for: ");
            if (!long.TryParse(Console.ReadLine(), out long nodeId))
            {
                DebugWriter.DebugWriteLine("#00LEE1#", "Invalid node ID. Please enter a valid integer.");
                return;
            }

            List<EdgeBase> edges = manager.LoadEdges(nodeId);
            if (edges.Count > 0)
            {
                DebugWriter.DebugWriteLine("#00LES1#", $"Edges for node {nodeId}:");
                foreach (var edge in edges)
                {
                    DebugWriter.DebugWriteLine("#00LES2#", $"To: {edge.ToNode}, Weight: {(edge as dynamic).Weight}, Content: {(edge as dynamic).EdgeContent}");
                }
            }
            else
            {
                DebugWriter.DebugWriteLine("#00LEE2#", $"No edges found for node {nodeId}.");
            }
        }

        static void ViewNodesAndEdges(GraphFileManager manager)
        {
            List<long> nodeIds = manager.GetAllNodeIds();

            if (nodeIds.Count == 0)
            {
                DebugWriter.DebugWriteLine("#00VNE1#", "No nodes found in the graph.");
                return;
            }

            foreach (long nodeId in nodeIds)
            {
                NodeBase? node = manager.LoadNode(nodeId);
                if (node != null)
                {
                    DebugWriter.DebugWriteLine("#00VNS1#", $"Node {nodeId}: {(node as dynamic).Content}");
                    List<EdgeBase> edges = manager.LoadEdges(nodeId);
                    if (edges.Count > 0)
                    {
                        DebugWriter.DebugWriteLine("#00VNS2#", $"  Edges:");
                        foreach (var edge in edges)
                        {
                            DebugWriter.DebugWriteLine("#00VNS3#", $"    To: {edge.ToNode}, Weight: {(edge as dynamic).Weight}, Content: {(edge as dynamic).EdgeContent}");
                        }
                    }
                    else
                    {
                        DebugWriter.DebugWriteLine("#00VNS4#", "  No edges for this node.");
                    }
                }
                else
                {
                    DebugWriter.DebugWriteLine("#00VNE2#", $"Failed to load node {nodeId}.");
                }
            }
        }
    }
}