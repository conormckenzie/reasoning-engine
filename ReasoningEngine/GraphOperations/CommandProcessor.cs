using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using ReasoningEngine.GraphFileHandling;
using ReasoningEngine; // Corrected namespace for Core classes
using DebugUtils;

namespace ReasoningEngine.GraphAccess
{
    public class CommandProcessor
    {
        // Inject GraphObjectMapper which handles interaction with the storage provider
        private readonly GraphObjectMapper graphObjectMapper; 

        public CommandProcessor(GraphObjectMapper graphObjectMapper) // Updated constructor parameter
        {
            this.graphObjectMapper = graphObjectMapper ?? throw new ArgumentNullException(nameof(graphObjectMapper));
        }

        /// <summary>
        /// Processes a command with its payload.
        /// Payloads are generally pipe-delimited strings.
        /// For AddNode/EditNode, the format after id|content is key=value pairs, also pipe-delimited.
        /// Example AddNode: "1|Node A|Role=Variable|VariableDomainType=Truth"
        /// Example EditNode: "1|New Content|FunctionType=Linear|FunctionParams=Weights=[0.5];Bias=0.1" 
        /// </summary>
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
                // Use Task.Result for simplicity in this synchronous method. Consider async/await pattern later.
                Node? node = graphObjectMapper.GetNodeAsync(nodeId).Result; 
                if (node != null)
                {
                    // Basic formatting, might need more detail depending on node Role
                    return $"Node {nodeId}: Version={node.Version}, Role={node.Role}, Content='{node.Content}'";
                }
                return $"Node {nodeId} not found.";
            }
            return "Invalid node ID.";
        }

        private string QueryEdges(string payload, bool outgoing)
        {
            if (long.TryParse(payload, out long nodeId))
            {
                 // Use Task.Result for simplicity in this synchronous method. Consider async/await pattern later.
                 List<Edge> edges = outgoing 
                                    ? graphObjectMapper.GetOutgoingEdgesAsync(nodeId).Result 
                                    : graphObjectMapper.GetIncomingEdgesAsync(nodeId).Result;

                if (edges.Count > 0)
                {
                    string direction = outgoing ? "Outgoing" : "Incoming";
                    // Use System.Text.StringBuilder for better performance with string concatenation
                    var resultBuilder = new System.Text.StringBuilder($"{direction} edges for node {nodeId}:\n"); 
                    foreach (var edge in edges)
                    {
                        string connectedNode = outgoing ? edge.ToNode.ToString() : edge.FromNode.ToString();
                        // Access EdgeContent property on the Edge object (alias for EdgeV2)
                        resultBuilder.AppendLine($"  -> Connected Node: {connectedNode}, EdgeId: {edge.EdgeId}, Version: {edge.Version}, Weight: {edge.Weight}, Content: '{edge.EdgeContent}'"); 
                    }
                    return resultBuilder.ToString();
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
            // Remaining parts define the role and subtype info (index 2 onwards)
            string[] rolePayloadParts = parts.Skip(2).ToArray(); 

            try 
            {
                // Parse payload parts into dictionary (values are strings initially)
                var parameters = ParsePayloadDictionary(rolePayloadParts);

                // If FunctionParams was provided as a string, parse it into a dictionary
                if (parameters.TryGetValue("FunctionParams", out object? funcParamsObj) && funcParamsObj is string funcParamsStr)
                {
                    try 
                    {
                        parameters["FunctionParams"] = ParseFunctionParamsString(funcParamsStr);
                    }
                    catch (ArgumentException ex)
                    {
                         throw new ArgumentException($"Invalid format for FunctionParams string '{funcParamsStr}': {ex.Message}", ex);
                    }
                }

                // Delegate creation to NodeFactory using the dictionary (which now might contain a parsed FunctionParams dict)
                Node newNode = NodeFactory.CreateNodeFromPayload(nodeId, content, parameters); // Use Node alias
                
                // TODO: Implement using GraphObjectMapper layer (to serialize newNode and call SaveNodeDataAsync)
                // Use Task.Result for simplicity in this synchronous method. Consider async/await pattern later.
                bool success = graphObjectMapper.SaveNodeAsync(newNode).Result;
                if (success)
                {
                     return $"Node {nodeId} added successfully.";
                }
                else 
                {
                    return $"Failed to save node {nodeId}.";
                }
            }
            catch (ArgumentException ex) // Catch errors from NodeFactory payload parsing
            {
                return $"Failed to add node {nodeId}: {ex.Message}";
            }
            catch (Exception ex) // Catch other potential errors during creation/saving
            {
                 DebugWriter.DebugWriteLine("#ADD_NODE_ERR#", $"Unexpected error adding node {nodeId}: {ex.Message}");
                 return $"Failed to add node {nodeId} due to an unexpected error.";
            }
        }

        private string DeleteNode(string payload)
        {
            if (long.TryParse(payload, out long nodeId))
            {
                 // TODO: Implement using GraphObjectMapper layer (to call DeleteNodeDataAsync and handle edges)
                 // Use Task.Result for simplicity in this synchronous method. Consider async/await pattern later.
                 bool success = graphObjectMapper.DeleteNodeAsync(nodeId).Result;
                 if (success)
                 {
                    // Note: Mapper currently warns that edges are not deleted.
                    return $"Node {nodeId} data deleted. (Associated edges might still exist).";
                 }
                 else
                 {
                    return $"Failed to delete node {nodeId}. It might not exist or an error occurred.";
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
            
            // Use Task.Result for simplicity in this synchronous method. Consider async/await pattern later.
            Node? existingNode = graphObjectMapper.GetNodeAsync(nodeId).Result;
            if (existingNode == null)
            {
                return $"Node {nodeId} not found.";
            }

            // Node alias is NodeV3, so this cast should be safe if GetNodeAsync works correctly
             if (!(existingNode is Node currentNode)) 
             {
                  // This path indicates an issue with GetNodeAsync or the stored data type
                  DebugWriter.DebugWriteLine("#EDIT_NODE_TYPE_ERR#", $"Loaded node {nodeId} is not of expected type Node/NodeV3.");
                  return $"Node {nodeId} is not of the expected type (NodeV3). Edit failed.";
             }


            string newContent = parts[1];
            // Remaining parts define the potential new role and subtype info (index 2 onwards)
            string[] rolePayloadParts = parts.Skip(2).ToArray(); 

            try
            {
                // Parse payload parts into dictionary (values are strings initially)
                var updateParameters = ParsePayloadDictionary(rolePayloadParts);

                 // If FunctionParams was provided as a string, parse it into a dictionary
                if (updateParameters.TryGetValue("FunctionParams", out object? funcParamsObj) && funcParamsObj is string funcParamsStr)
                {
                    try 
                    {
                        updateParameters["FunctionParams"] = ParseFunctionParamsString(funcParamsStr);
                    }
                    catch (ArgumentException ex)
                    {
                         throw new ArgumentException($"Invalid format for FunctionParams string during update '{funcParamsStr}': {ex.Message}", ex);
                    }
                }

                // Delegate update logic to NodeFactory, passing the dictionary
                Node updatedNode = NodeFactory.UpdateNodeFromPayload(currentNode, newContent, updateParameters); 

                // Save the updated node
                bool success = graphObjectMapper.SaveNodeAsync(updatedNode).Result; 
                if (success)
                {
                    return $"Node {nodeId} updated successfully.";
                }
                else 
                {
                    return $"Failed to save updated node {nodeId}.";
                }
            }
            catch (ArgumentException ex) // Catch errors from NodeFactory payload parsing/update logic
            {
                return $"Failed to update node {nodeId}: {ex.Message}";
            }
            catch (Exception ex) // Catch other potential errors during update/saving
            {
                 DebugWriter.DebugWriteLine("#EDIT_NODE_ERR#", $"Unexpected error editing node {nodeId}: {ex.Message}");
                 return $"Failed to update node {nodeId} due to an unexpected error.";
            }
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
            Edge newEdge = new Edge(fromNodeId, toNodeId, weight, content); // Edge alias is EdgeV2
            
            // Use Task.Result for simplicity in this synchronous method. Consider async/await pattern later.
            bool success = graphObjectMapper.SaveEdgeAsync(newEdge).Result;
            if (success)
            {
                return $"Edge from {fromNodeId} to {toNodeId} added successfully.";
            }
            else
            {
                // The mapper or provider should log specific errors
                return $"Failed to add edge from {fromNodeId} to {toNodeId}. Check logs for details.";
            }
        }

        private string DeleteEdge(string payload)
        {
            string[] parts = payload.Split('|');
            if (parts.Length != 2 || !long.TryParse(parts[0], out long sourceNodeId) || 
                !long.TryParse(parts[1], out long destNodeId))
            {
                return "Invalid payload for deleting an edge.";
            }

            // Use Task.Result for simplicity in this synchronous method. Consider async/await pattern later.
            bool success = graphObjectMapper.DeleteEdgeAsync(sourceNodeId, destNodeId).Result;
            if (success)
            {
                return $"Edge from node {sourceNodeId} to node {destNodeId} deleted successfully.";
            }
            else
            {
                // The mapper or provider should log specific errors
                return $"Failed to delete edge from node {sourceNodeId} to node {destNodeId}. It might not exist or an error occurred.";
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
            Edge updatedEdge = new Edge(sourceNodeId, destNodeId, newWeight, newContent); // Edge alias is EdgeV2
            
            // TODO: Add logic to actually *load* the existing edge first if needed for validation/merging?
            // For now, just overwrite by saving the new edge data.
            // Use Task.Result for simplicity in this synchronous method. Consider async/await pattern later.
            bool success = graphObjectMapper.SaveEdgeAsync(updatedEdge).Result;
            if (success)
            {
                 return $"Edge from {sourceNodeId} to {destNodeId} updated successfully.";
            }
            else
            {
                 // The mapper or provider should log specific errors
                 return $"Failed to update edge from {sourceNodeId} to {destNodeId}. Check logs for details.";
            }
        }

        // Helper method to parse payload parts (key=value pairs) into a dictionary
        private Dictionary<string, object> ParsePayloadDictionary(IEnumerable<string> payloadParts)
        {
            var parameters = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase); // Case-insensitive keys
            foreach (var part in payloadParts)
            {
                var keyValue = part.Split('=', 2); // Split only on the first '='
                if (keyValue.Length == 2)
                {
                    // Store value as string for now, NodeFactory will handle type conversion
                    parameters[keyValue[0].Trim()] = keyValue[1].Trim(); 
                }
                else if (!string.IsNullOrWhiteSpace(part))
                {
                    // Handle potential flags or parts without '=' if needed, or log warning
                     DebugWriter.DebugWriteLine("#PAYLOAD_WARN#", $"Ignoring payload part without '=': '{part}'");
                }
            }
            return parameters;
        }

        // Helper to parse FunctionParams string (e.g., "Key1:Value1;Key2:Value2")
        // Note: This assumes simple key-value pairs and doesn't handle nested structures or complex types within the string.
        // NodeFactory's ParseFunctionParameters will handle type conversion (e.g., string "0.5" to double 0.5).
        private Dictionary<string, object> ParseFunctionParamsString(string paramsString)
        {
            var dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(paramsString)) return dict;

            var pairs = paramsString.Split(';');
            foreach (var pair in pairs)
            {
                var keyValue = pair.Split(':', 2);
                if (keyValue.Length == 2)
                {
                    string key = keyValue[0].Trim();
                    string value = keyValue[1].Trim();
                    if (!string.IsNullOrEmpty(key))
                    {
                        // Store as string initially; NodeFactory will handle conversion
                        dict[key] = value; 
                    }
                }
                 else if (!string.IsNullOrWhiteSpace(pair))
                {
                     DebugWriter.DebugWriteLine("#PARAM_PARSE_WARN#", $"Ignoring malformed FunctionParams part: '{pair}'");
                }
            }
            return dict;
        }
    }
}
