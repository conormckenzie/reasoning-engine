using System;
using System.Threading.Tasks;
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
                DebugWriter.DebugWriteLine("#SNE1#", "Invalid node ID. Please enter a valid integer.");
                return;
            }

            Console.Write("Enter node content: ");
            string? nodeContent = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(nodeContent))
            {
                DebugWriter.DebugWriteLine("#SNE2#", "Node content cannot be empty.");
                return;
            }

            bool success = manager.SaveNode(nodeId, nodeContent);
            if (success)
            {
                DebugWriter.DebugWriteLine("#SNS1#", $"Node {nodeId} saved successfully.");
            }
            else
            {
                DebugWriter.DebugWriteLine("#SNE3#", $"Failed to save node {nodeId}.");
            }
        }

        static void LoadNodeWithUserInput(GraphFileManager manager)
        {
            Console.Write("Enter node ID to load: ");
            if (!long.TryParse(Console.ReadLine(), out long nodeId))
            {
                DebugWriter.DebugWriteLine("#LNE1#", "Invalid node ID. Please enter a valid integer.");
                return;
            }

            Node? node = manager.LoadNode(nodeId);
            if (node != null)
            {
                DebugWriter.DebugWriteLine("#LNS1#", $"Node {nodeId} loaded successfully.");
                DebugWriter.DebugWriteLine("#LNS2#", $"Content: {node.Content}");
            }
            else
            {
                DebugWriter.DebugWriteLine("#LNE2#", $"Failed to load node {nodeId}. The node may not exist.");
            }
        }

        static void SaveEdgeWithUserInput(GraphFileManager manager)
        {
            Console.Write("Enter source node ID: ");
            if (!long.TryParse(Console.ReadLine(), out long fromNodeId))
            {
                DebugWriter.DebugWriteLine("#SEE1#", "Invalid source node ID. Please enter a valid integer.");
                return;
            }

            Console.Write("Enter destination node ID: ");
            if (!long.TryParse(Console.ReadLine(), out long toNodeId))
            {
                DebugWriter.DebugWriteLine("#SEE2#", "Invalid destination node ID. Please enter a valid integer.");
                return;
            }

            Console.Write("Enter edge weight: ");
            if (!double.TryParse(Console.ReadLine(), out double weight))
            {
                DebugWriter.DebugWriteLine("#SEE3#", "Invalid weight. Please enter a valid number.");
                return;
            }

            Console.Write("Enter edge content: ");
            string? edgeContent = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(edgeContent))
            {
                DebugWriter.DebugWriteLine("#SEE4#", "Edge content cannot be empty.");
                return;
            }

            var edge = new Edge(fromNodeId, toNodeId, weight, edgeContent);
            bool success = manager.SaveEdge(edge);
            if (success)
            {
                DebugWriter.DebugWriteLine("#SES1#", $"Edge from {fromNodeId} to {toNodeId} saved successfully.");
            }
            else
            {
                DebugWriter.DebugWriteLine("#SEE5#", $"Failed to save edge from {fromNodeId} to {toNodeId}.");
            }
        }

        static void LoadEdgesWithUserInput(GraphFileManager manager)
        {
            Console.Write("Enter node ID to load edges for: ");
            if (!long.TryParse(Console.ReadLine(), out long nodeId))
            {
                DebugWriter.DebugWriteLine("#LEE1#", "Invalid node ID. Please enter a valid integer.");
                return;
            }

            List<Edge> edges = manager.LoadEdges(nodeId);
            if (edges.Count > 0)
            {
                DebugWriter.DebugWriteLine("#LES1#", $"Edges for node {nodeId}:");
                foreach (var edge in edges)
                {
                    DebugWriter.DebugWriteLine("#LES2#", $"To: {edge.ToNode}, Weight: {edge.Weight}, Content: {edge.EdgeContent}");
                }
            }
            else
            {
                DebugWriter.DebugWriteLine("#LEE2#", $"No edges found for node {nodeId}.");
            }
        }

        static void ViewNodesAndEdges(GraphFileManager manager)
        {
            // This method assumes we have a way to get all node IDs. 
            // If not, we'll need to modify GraphFileManager to support this.
            List<long> nodeIds = manager.GetAllNodeIds();

            if (nodeIds.Count == 0)
            {
                DebugWriter.DebugWriteLine("#VNE1#", "No nodes found in the graph.");
                return;
            }

            foreach (long nodeId in nodeIds)
            {
                Node? node = manager.LoadNode(nodeId);
                if (node != null)
                {
                    DebugWriter.DebugWriteLine("#VNS1#", $"Node {nodeId}: {node.Content}");
                    List<Edge> edges = manager.LoadEdges(nodeId);
                    if (edges.Count > 0)
                    {
                        DebugWriter.DebugWriteLine("#VNS2#", $"  Edges:");
                        foreach (var edge in edges)
                        {
                            DebugWriter.DebugWriteLine("#VNS3#", $"    To: {edge.ToNode}, Weight: {edge.Weight}, Content: {edge.EdgeContent}");
                        }
                    }
                    else
                    {
                        DebugWriter.DebugWriteLine("#VNS4#", "  No edges for this node.");
                    }
                }
                else
                {
                    DebugWriter.DebugWriteLine("#VNE2#", $"Failed to load node {nodeId}.");
                }
            }
        }
    }
}