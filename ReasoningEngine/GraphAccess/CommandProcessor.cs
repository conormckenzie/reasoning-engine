using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using ReasoningEngine.GraphFileHandling;
using DebugUtils;

namespace ReasoningEngine.GraphAccess
{
    public class CommandProcessor
    {
        private readonly GraphFileManager graphFileManager;

        public CommandProcessor(GraphFileManager graphFileManager)
        {
            this.graphFileManager = graphFileManager;
        }

        /// <summary>
        /// Displays the command processor menu and handles user input for command processing.
        /// </summary>
        public void ShowCommandProcessorMenu()
        {
            while (true)
            {
                DebugWriter.DebugWriteLine("#CMD001#", "\nCommand Processor Options:");
                DebugWriter.DebugWriteLine("#CMD002#", "1. Process Node Query");
                DebugWriter.DebugWriteLine("#CMD003#", "2. Process Edge Query");
                DebugWriter.DebugWriteLine("#CMD004#", "3. Add Node");
                DebugWriter.DebugWriteLine("#CMD005#", "4. Delete Node");
                DebugWriter.DebugWriteLine("#CMD006#", "5. Edit Node");
                DebugWriter.DebugWriteLine("#CMD007#", "6. Add Edge");
                DebugWriter.DebugWriteLine("#CMD008#", "7. Delete Edge");
                DebugWriter.DebugWriteLine("#CMD009#", "8. Edit Edge");
                DebugWriter.DebugWriteLine("#CMD010#", "9. Find Edges by Destination Node");
                DebugWriter.DebugWriteLine("#CMD011#", "10. Back to Main Menu");
                DebugWriter.DebugWrite("#CMD012#", "Enter option: ");

                var option = Console.ReadLine();

                string commandResult;
                switch (option)
                {
                    case "1":
                        commandResult = ProcessCommand("node_query", "");
                        DebugWriter.DebugWriteLine("#RES001#", commandResult);
                        break;
                    case "2":
                        commandResult = ProcessCommand("edge_query", "");
                        DebugWriter.DebugWriteLine("#RES002#", commandResult);
                        break;
                    case "3":
                        commandResult = ProcessCommand("add_node", "");
                        DebugWriter.DebugWriteLine("#RES003#", commandResult);
                        break;
                    case "4":
                        commandResult = ProcessCommand("delete_node", "");
                        DebugWriter.DebugWriteLine("#RES004#", commandResult);
                        break;
                    case "5":
                        commandResult = ProcessCommand("edit_node", "");
                        DebugWriter.DebugWriteLine("#RES005#", commandResult);
                        break;
                    case "6":
                        commandResult = ProcessCommand("add_edge", "");
                        DebugWriter.DebugWriteLine("#RES006#", commandResult);
                        break;
                    case "7":
                        commandResult = ProcessCommand("delete_edge", "");
                        DebugWriter.DebugWriteLine("#RES007#", commandResult);
                        break;
                    case "8":
                        commandResult = ProcessCommand("edit_edge", "");
                        DebugWriter.DebugWriteLine("#RES008#", commandResult);
                        break;
                    case "9":
                        commandResult = FindEdgesByDestination();
                        DebugWriter.DebugWriteLine("#RES009#", commandResult);
                        break;
                    case "10":
                        return;
                    default:
                        DebugWriter.DebugWriteLine("#INV002#", "Invalid option. Please try again.");
                        break;
                }
            }
        }

        // Synchronous command processing
        public string ProcessCommand(string command, string payload)
        {
            switch (command.ToLower())
            {
                case "node_query":
                    return QueryNode();
                case "edge_query":
                    return QueryEdge();
                case "add_node":
                    return AddNode();
                case "delete_node":
                    return DeleteNode();
                case "edit_node":
                    return EditNode();
                case "add_edge":
                    return AddEdge();
                case "delete_edge":
                    return DeleteEdge();
                case "edit_edge":
                    return EditEdge();
                default:
                    return "Unknown command";
            }
        }

        // Asynchronous command processing
        public async Task<string> ProcessCommandAsync(string command, string payload)
        {
            return await Task.FromResult(ProcessCommand(command, payload));
        }

        private string QueryNode()
        {
            DebugWriter.DebugWrite("#QRY001#", "Enter node ID to query: ");
            if (long.TryParse(Console.ReadLine(), out long nodeId))
            {
                NodeBase? node = graphFileManager.LoadNode(nodeId);
                if (node != null)
                {
                    return $"Node {nodeId}: Version {node.Version}, Content: {(node as dynamic).Content}";
                }
                return $"Node {nodeId} not found.";
            }
            return "Invalid node ID.";
        }

        private string QueryEdge()
        {
            DebugWriter.DebugWrite("#QRY002#", "Enter source node ID: ");
            if (long.TryParse(Console.ReadLine(), out long sourceNodeId))
            {
                List<EdgeBase> edges = graphFileManager.LoadEdges(sourceNodeId);
                if (edges.Count > 0)
                {
                    string result = $"Edges from node {sourceNodeId}:\n";
                    foreach (var edge in edges)
                    {
                        result += $"To node {edge.ToNode}: Version {edge.Version}, Weight: {(edge as dynamic).Weight}, Content: {(edge as dynamic).EdgeContent}\n";
                    }
                    return result;
                }
                return $"No edges found for node {sourceNodeId}.";
            }
            return "Invalid node ID.";
        }

        private string AddNode()
        {
            DebugWriter.DebugWrite("#ADD001#", "Enter node ID: ");
            if (long.TryParse(Console.ReadLine(), out long nodeId))
            {
                DebugWriter.DebugWrite("#ADD002#", "Enter node content: ");
                string content = Console.ReadLine() ?? "";
                Node newNode = new Node(nodeId, content);
                if (graphFileManager.SaveNode(newNode))
                {
                    return $"Node {nodeId} added successfully.";
                }
                return $"Failed to add node {nodeId}.";
            }
            return "Invalid node ID.";
        }

        private string AddEdge()
        {
            DebugWriter.DebugWrite("#ADD003#", "Enter source node ID: ");
            if (long.TryParse(Console.ReadLine(), out long sourceNodeId))
            {
                DebugWriter.DebugWrite("#ADD004#", "Enter destination node ID: ");
                if (long.TryParse(Console.ReadLine(), out long destNodeId))
                {
                    DebugWriter.DebugWrite("#ADD005#", "Enter edge weight: ");
                    if (double.TryParse(Console.ReadLine(), out double weight))
                    {
                        DebugWriter.DebugWrite("#ADD006#", "Enter edge content: ");
                        string content = Console.ReadLine() ?? "";
                        Edge newEdge = new Edge(sourceNodeId, destNodeId, weight, content);
                        if (graphFileManager.SaveEdge(newEdge))
                        {
                            return $"Edge from {sourceNodeId} to {destNodeId} added successfully.";
                        }
                        return $"Failed to add edge from {sourceNodeId} to {destNodeId}.";
                    }
                    return "Invalid weight.";
                }
                return "Invalid destination node ID.";
            }
            return "Invalid source node ID.";
        }

        private string DeleteNode()
        {
            DebugWriter.DebugWrite("#DEL001#", "Enter node ID to delete: ");
            if (long.TryParse(Console.ReadLine(), out long nodeId))
            {
                if (graphFileManager.DeleteNode(nodeId))
                {
                    return $"Node {nodeId} and all its associated edges have been deleted successfully.";
                }
                else
                {
                    return $"Failed to delete node {nodeId}. It may not exist or an error occurred.";
                }
            }
            return "Invalid node ID.";
        }

        private string EditNode()
        {
            DebugWriter.DebugWrite("#EDT001#", "Enter node ID to edit: ");
            if (long.TryParse(Console.ReadLine(), out long nodeId))
            {
                NodeBase? node = graphFileManager.LoadNode(nodeId);
                if (node != null)
                {
                    DebugWriter.DebugWrite("#EDT002#", "Enter new node content: ");
                    string newContent = Console.ReadLine() ?? "";
                    Node updatedNode = new Node(nodeId, newContent);
                    if (graphFileManager.SaveNode(updatedNode))
                    {
                        return $"Node {nodeId} updated successfully.";
                    }
                    return $"Failed to update node {nodeId}.";
                }
                return $"Node {nodeId} not found.";
            }
            return "Invalid node ID.";
        }

        private string DeleteEdge()
        {
            DebugWriter.DebugWrite("#DEL003#", "Enter source node ID: ");
            if (long.TryParse(Console.ReadLine(), out long sourceNodeId))
            {
                DebugWriter.DebugWrite("#DEL004#", "Enter destination node ID: ");
                if (long.TryParse(Console.ReadLine(), out long destNodeId))
                {
                    if (graphFileManager.DeleteEdge(sourceNodeId, destNodeId))
                    {
                        return $"Edge from node {sourceNodeId} to node {destNodeId} has been deleted successfully.";
                    }
                    else
                    {
                        return $"Failed to delete edge from node {sourceNodeId} to node {destNodeId}. It may not exist or an error occurred.";
                    }
                }
                return "Invalid destination node ID.";
            }
            return "Invalid source node ID.";
        }

        private string EditEdge()
        {
            DebugWriter.DebugWrite("#EDT003#", "Enter source node ID: ");
            if (long.TryParse(Console.ReadLine(), out long sourceNodeId))
            {
                DebugWriter.DebugWrite("#EDT004#", "Enter destination node ID: ");
                if (long.TryParse(Console.ReadLine(), out long destNodeId))
                {
                    List<EdgeBase> edges = graphFileManager.LoadEdges(sourceNodeId);
                    EdgeBase? edgeToEdit = edges.FirstOrDefault(e => e.ToNode == destNodeId);
                    if (edgeToEdit != null)
                    {
                        DebugWriter.DebugWrite("#EDT005#", "Enter new edge weight: ");
                        if (double.TryParse(Console.ReadLine(), out double newWeight))
                        {
                            DebugWriter.DebugWrite("#EDT006#", "Enter new edge content: ");
                            string newContent = Console.ReadLine() ?? "";
                            Edge updatedEdge = new Edge(sourceNodeId, destNodeId, newWeight, newContent);
                            if (graphFileManager.SaveEdge(updatedEdge))
                            {
                                return $"Edge from {sourceNodeId} to {destNodeId} updated successfully.";
                            }
                            return $"Failed to update edge from {sourceNodeId} to {destNodeId}.";
                        }
                        return "Invalid weight.";
                    }
                    return $"Edge from {sourceNodeId} to {destNodeId} not found.";
                }
                return "Invalid destination node ID.";
            }
            return "Invalid source node ID.";
        }

        private string FindEdgesByDestination()
        {
            DebugWriter.DebugWrite("#FND001#", "Enter destination node ID: ");
            if (long.TryParse(Console.ReadLine(), out long destNodeId))
            {
                List<EdgeBase> edges = graphFileManager.FindEdgesByDestinationNode(destNodeId);
                if (edges.Count > 0)
                {
                    string result = $"Edges to node {destNodeId}:\n";
                    foreach (var edge in edges)
                    {
                        result += $"From node {edge.FromNode}: Version {edge.Version}, Weight: {(edge as dynamic).Weight}, Content: {(edge as dynamic).EdgeContent}\n";
                    }
                    return result;
                }
                return $"No edges found with destination node {destNodeId}.";
            }
            return "Invalid node ID.";
        }
    }
}