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

        public void ShowCommandProcessorMenu()
        {
            while (true)
            {
                DebugWriter.DebugWriteLine("#CMD001#", "\nCommand Processor Options:");
                DebugWriter.DebugWriteLine("#CMD002#", "1. Query Node");
                DebugWriter.DebugWriteLine("#CMD003#", "2. Query Outgoing Edges");
                DebugWriter.DebugWriteLine("#CMD004#", "3. Query Incoming Edges");
                DebugWriter.DebugWriteLine("#CMD005#", "4. Add Node");
                DebugWriter.DebugWriteLine("#CMD006#", "5. Delete Node");
                DebugWriter.DebugWriteLine("#CMD007#", "6. Edit Node");
                DebugWriter.DebugWriteLine("#CMD008#", "7. Add Edge");
                DebugWriter.DebugWriteLine("#CMD009#", "8. Delete Edge");
                DebugWriter.DebugWriteLine("#CMD010#", "9. Edit Edge");
                DebugWriter.DebugWriteLine("#CMD011#", "10. Back to Main Menu");
                DebugWriter.DebugWrite("#CMD012#", "Enter option: ");

                var option = Console.ReadLine();
                string command = "";
                string payload = "";

                switch (option)
                {
                    case "1":
                        command = "node_query";
                        DebugWriter.DebugWrite("#QRY001#", "Enter node ID: ");
                        payload = Console.ReadLine() ?? "";
                        break;
                    case "2":
                        command = "outgoing_edge_query";
                        DebugWriter.DebugWrite("#QRY002#", "Enter source node ID: ");
                        payload = Console.ReadLine() ?? "";
                        break;
                    case "3":
                        command = "incoming_edge_query";
                        DebugWriter.DebugWrite("#QRY003#", "Enter destination node ID: ");
                        payload = Console.ReadLine() ?? "";
                        break;
                    case "4":
                        command = "add_node";
                        DebugWriter.DebugWrite("#ADD001#", "Enter node ID: ");
                        string nodeId = Console.ReadLine() ?? "";
                        DebugWriter.DebugWrite("#ADD002#", "Enter node content: ");
                        string content = Console.ReadLine() ?? "";
                        payload = $"{nodeId}|{content}";
                        break;
                    case "5":
                        command = "delete_node";
                        DebugWriter.DebugWrite("#DEL001#", "Enter node ID to delete: ");
                        payload = Console.ReadLine() ?? "";
                        break;
                    case "6":
                        command = "edit_node";
                        DebugWriter.DebugWrite("#EDT001#", "Enter node ID to edit: ");
                        string editNodeId = Console.ReadLine() ?? "";
                        DebugWriter.DebugWrite("#EDT002#", "Enter new node content: ");
                        string newContent = Console.ReadLine() ?? "";
                        payload = $"{editNodeId}|{newContent}";
                        break;
                    case "7":
                        command = "add_edge";
                        DebugWriter.DebugWrite("#ADD003#", "Enter source node ID: ");
                        string sourceId = Console.ReadLine() ?? "";
                        DebugWriter.DebugWrite("#ADD004#", "Enter destination node ID: ");
                        string destId = Console.ReadLine() ?? "";
                        DebugWriter.DebugWrite("#ADD005#", "Enter edge weight: ");
                        string weight = Console.ReadLine() ?? "";
                        DebugWriter.DebugWrite("#ADD006#", "Enter edge content: ");
                        string edgeContent = Console.ReadLine() ?? "";
                        payload = $"{sourceId}|{destId}|{weight}|{edgeContent}";
                        break;
                    case "8":
                        command = "delete_edge";
                        DebugWriter.DebugWrite("#DEL003#", "Enter source node ID: ");
                        string delSourceId = Console.ReadLine() ?? "";
                        DebugWriter.DebugWrite("#DEL004#", "Enter destination node ID: ");
                        string delDestId = Console.ReadLine() ?? "";
                        payload = $"{delSourceId}|{delDestId}";
                        break;
                    case "9":
                        command = "edit_edge";
                        DebugWriter.DebugWrite("#EDT003#", "Enter source node ID: ");
                        string editSourceId = Console.ReadLine() ?? "";
                        DebugWriter.DebugWrite("#EDT004#", "Enter destination node ID: ");
                        string editDestId = Console.ReadLine() ?? "";
                        DebugWriter.DebugWrite("#EDT005#", "Enter new edge weight: ");
                        string newWeight = Console.ReadLine() ?? "";
                        DebugWriter.DebugWrite("#EDT006#", "Enter new edge content: ");
                        string newEdgeContent = Console.ReadLine() ?? "";
                        payload = $"{editSourceId}|{editDestId}|{newWeight}|{newEdgeContent}";
                        break;
                    case "10":
                        return;
                    default:
                        DebugWriter.DebugWriteLine("#INV001#", "Invalid option. Please try again.");
                        continue;
                }

                string result = ProcessCommand(command, payload);
                DebugWriter.DebugWriteLine("#CMDRES#", result);
            }
        }

        public string ProcessCommand(string command, string payload)
        {
            switch (command.ToLower())
            {
                case "node_query":
                    return QueryNode(payload);
                case "outgoing_edge_query":
                    return QueryEdges(payload, true);
                case "incoming_edge_query":
                    return QueryEdges(payload, false);
                case "add_node":
                    return AddNode(payload);
                case "delete_node":
                    return DeleteNode(payload);
                case "edit_node":
                    return EditNode(payload);
                case "add_edge":
                    return AddEdge(payload);
                case "delete_edge":
                    return DeleteEdge(payload);
                case "edit_edge":
                    return EditEdge(payload);
                default:
                    return "Unknown command";
            }
        }

        public async Task<string> ProcessCommandAsync(string command, string payload)
        {
            return await Task.Run(() => ProcessCommand(command, payload));
        }

        private string QueryNode(string payload)
        {
            if (long.TryParse(payload, out long nodeId))
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

        private string QueryEdges(string payload, bool outgoing)
        {
            if (long.TryParse(payload, out long nodeId))
            {
                List<EdgeBase> edges = graphFileManager.LoadEdges(nodeId, outgoing);
                if (edges.Count > 0)
                {
                    string direction = outgoing ? "Outgoing" : "Incoming";
                    string result = $"{direction} edges for node {nodeId}:\n";
                    foreach (var edge in edges)
                    {
                        string connectedNode = outgoing ? edge.ToNode.ToString() : edge.FromNode.ToString();
                        result += $"Connected Node: {connectedNode}, Version: {edge.Version}, Weight: {(edge as dynamic).Weight}, Content: {(edge as dynamic).EdgeContent}\n";
                    }
                    return result;
                }
                return $"No {(outgoing ? "outgoing" : "incoming")} edges found for node {nodeId}.";
            }
            return "Invalid node ID.";
        }

        private string AddNode(string payload)
        {
            string[] parts = payload.Split('|');
            if (parts.Length != 2 || !long.TryParse(parts[0], out long nodeId))
            {
                return "Invalid payload for adding a node.";
            }
            string content = parts[1];
            Node newNode = new Node(nodeId, content);
            if (graphFileManager.SaveNode(newNode))
            {
                return $"Node {nodeId} added successfully.";
            }
            return $"Failed to add node {nodeId}.";
        }

        private string DeleteNode(string payload)
        {
            if (long.TryParse(payload, out long nodeId))
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

        private string EditNode(string payload)
        {
            string[] parts = payload.Split('|');
            if (parts.Length != 2 || !long.TryParse(parts[0], out long nodeId))
            {
                return "Invalid payload for editing a node.";
            }
            string newContent = parts[1];
            Node updatedNode = new Node(nodeId, newContent);
            if (graphFileManager.SaveNode(updatedNode))
            {
                return $"Node {nodeId} updated successfully.";
            }
            return $"Failed to update node {nodeId}.";
        }

        private string AddEdge(string payload)
        {
            string[] parts = payload.Split('|');
            if (parts.Length != 4 || !long.TryParse(parts[0], out long fromNodeId) || 
                !long.TryParse(parts[1], out long toNodeId) || !double.TryParse(parts[2], out double weight))
            {
                return "Invalid payload for adding an edge.";
            }
            string content = parts[3];
            Edge newEdge = new Edge(fromNodeId, toNodeId, weight, content);
            if (graphFileManager.SaveEdge(newEdge))
            {
                return $"Edge from {fromNodeId} to {toNodeId} added successfully.";
            }
            return $"Failed to add edge from {fromNodeId} to {toNodeId}. One or both nodes may not exist.";
        }

        private string DeleteEdge(string payload)
        {
            string[] parts = payload.Split('|');
            if (parts.Length != 2 || !long.TryParse(parts[0], out long sourceNodeId) || 
                !long.TryParse(parts[1], out long destNodeId))
            {
                return "Invalid payload for deleting an edge.";
            }
            if (graphFileManager.DeleteEdge(sourceNodeId, destNodeId))
            {
                return $"Edge from node {sourceNodeId} to node {destNodeId} has been deleted successfully.";
            }
            else
            {
                return $"Failed to delete edge from node {sourceNodeId} to node {destNodeId}. It may not exist or an error occurred.";
            }
        }

        private string EditEdge(string payload)
        {
            string[] parts = payload.Split('|');
            if (parts.Length != 4 || !long.TryParse(parts[0], out long sourceNodeId) || 
                !long.TryParse(parts[1], out long destNodeId) || !double.TryParse(parts[2], out double newWeight))
            {
                return "Invalid payload for editing an edge.";
            }
            string newContent = parts[3];
            Edge updatedEdge = new Edge(sourceNodeId, destNodeId, newWeight, newContent);
            if (graphFileManager.SaveEdge(updatedEdge))
            {
                return $"Edge from {sourceNodeId} to {destNodeId} updated successfully.";
            }
            return $"Failed to update edge from {sourceNodeId} to {destNodeId}.";
        }
    }
}