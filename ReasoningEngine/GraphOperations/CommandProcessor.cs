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

        public virtual string ProcessCommand(string command, string payload)
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

        public virtual async Task<string> ProcessCommandAsync(string command, string payload)
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
            if (parts.Length < 2 || !long.TryParse(parts[0], out long nodeId))
            {
                return "Invalid payload for adding a node.";
            }
            
            string content = parts[1];
            NodeType nodeType = NodeType.Standard; // Default
            
            // Parse node type if provided
            if (parts.Length >= 3 && Enum.TryParse<NodeType>(parts[2], true, out NodeType parsedType))
            {
                nodeType = parsedType;
            }
            
            // Create the appropriate node type
            NodeBase newNode;
            switch (nodeType)
            {
                case NodeType.SIMO:
                    // For SIMO nodes, we need a domain interpretation
                    DomainInterpretation interpretation = DomainInterpretation.Truth; // Default
                    if (parts.Length >= 4 && Enum.TryParse<DomainInterpretation>(parts[3], true, out DomainInterpretation parsedInterp))
                    {
                        interpretation = parsedInterp;
                    }
                    newNode = new SIMONode(nodeId, content, interpretation);
                    break;
                    
                case NodeType.MISO:
                    newNode = new MISONode(nodeId, content);
                    break;
                    
                default: // Standard
                    newNode = new Node(nodeId, content);
                    break;
            }
            
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
            if (parts.Length < 2 || !long.TryParse(parts[0], out long nodeId))
            {
                return "Invalid payload for editing a node.";
            }
            
            // First, load the existing node to preserve its type
            NodeBase? existingNode = graphFileManager.LoadNode(nodeId);
            if (existingNode == null)
            {
                return $"Node {nodeId} not found.";
            }
            
            string newContent = parts[1];
            NodeType nodeType = existingNode.Type; // Preserve existing type by default
            
            // Allow changing node type if specified
            if (parts.Length >= 3 && Enum.TryParse<NodeType>(parts[2], true, out NodeType parsedType))
            {
                nodeType = parsedType;
            }
            
            // Create the appropriate node type
            NodeBase updatedNode;
            switch (nodeType)
            {
                case NodeType.SIMO:
                    // For SIMO nodes, we need a domain interpretation
                    DomainInterpretation interpretation = DomainInterpretation.Truth; // Default
                    
                    // If it's already a SIMO node, preserve its interpretation
                    if (existingNode is SIMONode existingSIMO)
                    {
                        interpretation = existingSIMO.Interpretation;
                    }
                    
                    // Allow changing interpretation if specified
                    if (parts.Length >= 4 && Enum.TryParse<DomainInterpretation>(parts[3], true, out DomainInterpretation parsedInterp))
                    {
                        interpretation = parsedInterp;
                    }
                    
                    updatedNode = new SIMONode(nodeId, newContent, interpretation);
                    break;
                    
                case NodeType.MISO:
                    updatedNode = new MISONode(nodeId, newContent);
                    
                    // If it's already a MISO node, preserve its output edge
                    if (existingNode is MISONode existingMISO && existingMISO.SingleOutputEdgeId.HasValue)
                    {
                        ((MISONode)updatedNode).SetSingleOutputEdge(existingMISO.SingleOutputEdgeId.Value);
                    }
                    break;
                    
                default: // Standard
                    updatedNode = new Node(nodeId, newContent);
                    break;
            }
            
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
