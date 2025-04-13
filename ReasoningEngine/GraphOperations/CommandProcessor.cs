using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using ReasoningEngine.GraphFileHandling;
using ReasoningEngine; // Corrected namespace for Core classes
using DebugUtils;
using System.Linq; // Added for Skip

namespace ReasoningEngine.GraphAccess
{
    /// <summary>
    /// Processes text-based commands to interact with the reasoning graph.
    /// This class acts as a facade over the GraphObjectMapper, providing a command-based interface.
    /// Rationale for Async-First:
    /// The underlying GraphObjectMapper and IGraphStorageProvider use asynchronous I/O operations.
    /// To avoid blocking threads and potential deadlocks (especially when consumed by async frameworks like ASP.NET Core),
    /// this processor implements its core logic asynchronously using async/await.
    /// A synchronous ProcessCommand method is provided for convenience, but it blocks waiting for the async result
    /// and should be used cautiously in contexts sensitive to blocking (UI, high-throughput servers).
    /// Consumers should prefer ProcessCommandAsync whenever possible.
    /// </summary>
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
        /// Example EditNode: "1|New Content|FunctionType=Linear|FunctionParams=Weights:0.5,0.6;Bias:0.1" 
        /// Note: FunctionParams value uses a semicolon-delimited string of Key:Value pairs.
        /// </summary>
        /// <remarks>
        /// This method provides a synchronous wrapper around the asynchronous core logic.
        /// It blocks the calling thread until the underlying asynchronous operation completes.
        /// WARNING: Blocking on asynchronous code can lead to deadlocks in synchronization contexts
        /// (like UI frameworks or ASP.NET Classic) and can harm scalability in server applications
        /// by holding onto threads. Prefer using <see cref="ProcessCommandAsync"/> whenever possible.
        /// </remarks>
        public virtual string ProcessCommand(string command, string payload)
        {
            // Call the internal async helper and block for the result using GetAwaiter().GetResult()
            // This is generally preferred over .Result for blocking as it unwraps AggregateException directly.
            try
            {
                // Use GetAwaiter().GetResult() for synchronous blocking
                return ProcessCommandAsyncInternal(command, payload).GetAwaiter().GetResult();
            }
            // No need to catch AggregateException specifically, GetAwaiter().GetResult() unwraps it.
            // catch (AggregateException ae) when (ae.InnerException != null)
            // {
            //     // Unwrap AggregateException often thrown by .Result
            //     // Log the error or return a specific error message
            //     DebugUtils.DebugWriter.DebugWriteLine("#CMD_PROC_ERR#", $"Error processing command '{command}': {ae.InnerException.Message}");
            //     return $"Error processing command '{command}': {ae.InnerException.Message}";
            // }
             catch (Exception ex) // Catch potential exceptions from the async operation (GetAwaiter().GetResult() unwraps AggregateException)
            {
                // Log the actual exception message
                DebugUtils.DebugWriter.DebugWriteLine("#CMD_PROC_ERR#", $"Error processing command '{command}': {ex.Message}");
                return $"Error processing command '{command}': {ex.Message}";
            }
            // Removed duplicate catch block
        }

        // Public async method now directly calls the internal async helper
        public virtual async Task<string> ProcessCommandAsync(string command, string payload)
        {
            return await ProcessCommandAsyncInternal(command, payload); 
        }

        // Internal async helper containing the core logic
        private async Task<string> ProcessCommandAsyncInternal(string command, string payload)
        {
             switch (command.ToLower())
            {
                case "node_query":
                    return await QueryNodeAsync(payload);
                case "outgoing_edge_query":
                    return await QueryEdgesAsync(payload, true);
                case "incoming_edge_query":
                    return await QueryEdgesAsync(payload, false);
                case "add_node":
                    return await AddNodeAsync(payload);
                case "delete_node":
                    return await DeleteNodeAsync(payload);
                case "edit_node":
                    return await EditNodeAsync(payload);
                case "add_edge":
                    return await AddEdgeAsync(payload);
                case "delete_edge":
                    return await DeleteEdgeAsync(payload);
                case "edit_edge":
                    return await EditEdgeAsync(payload);
                default:
                    return "Unknown command";
            }
        }

        private async Task<string> QueryNodeAsync(string payload) // Changed signature to async Task<string>
        {
            if (long.TryParse(payload, out long nodeId))
            {
                // Use await with ConfigureAwait(false) for library code
                NodeV3? node = await graphObjectMapper.GetNodeAsync(nodeId).ConfigureAwait(false);
                if (node != null)
                {
                    // Basic formatting, might need more detail depending on node Role
                    return $"Node {nodeId}: Version={node.Version}, Role={node.Role}, Content='{node.Content}'";
                }
                return $"Node {nodeId} not found.";
            }
            return "Invalid node ID.";
        }

        private async Task<string> QueryEdgesAsync(string payload, bool outgoing) // Changed signature
        {
            if (long.TryParse(payload, out long nodeId))
            {
                 // Use await with ConfigureAwait(false) for library code
                 List<EdgeV2> edges = outgoing
                                    ? await graphObjectMapper.GetOutgoingEdgesAsync(nodeId).ConfigureAwait(false)
                                    : await graphObjectMapper.GetIncomingEdgesAsync(nodeId).ConfigureAwait(false);

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

        private async Task<string> AddNodeAsync(string payload) // Changed signature
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

                // Use await with ConfigureAwait(false) for library code
                bool success = await graphObjectMapper.SaveNodeAsync(newNode).ConfigureAwait(false);
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

        private async Task<string> DeleteNodeAsync(string payload) // Changed signature
        {
            if (long.TryParse(payload, out long nodeId))
            {
                 // Use await with ConfigureAwait(false) for library code
                 bool success = await graphObjectMapper.DeleteNodeAsync(nodeId).ConfigureAwait(false);
                 if (success)
                 {
                    // Note: Mapper attempts to delete associated edges. Check logs for details if any failed.
                    return $"Node {nodeId} data deleted. Associated edges were also attempted to be deleted.";
                 }
                 else
                 {
                    return $"Failed to delete node {nodeId}. It might not exist or an error occurred.";
                 }
            }
            return "Invalid node ID.";
        }

        private async Task<string> EditNodeAsync(string payload) // Changed signature
        {
            string[] parts = payload.Split('|');
            if (parts.Length < 2 || !long.TryParse(parts[0], out long nodeId))
            {
                return "Invalid payload for editing a node.";
            }

            // Use await with ConfigureAwait(false) for library code
            NodeV3? existingNode = await graphObjectMapper.GetNodeAsync(nodeId).ConfigureAwait(false);
            if (existingNode == null)
            {
                return $"Node {nodeId} not found.";
            }

            // Removed the unnecessary cast, existingNode is already NodeV3
            // NodeFactory expects NodeV3 (or Node alias)

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
                // NodeFactory.UpdateNodeFromPayload now accepts NodeV3 directly.
                // Pass existingNode (which is NodeV3) without casting.
                Node updatedNode = NodeFactory.UpdateNodeFromPayload(existingNode, newContent, updateParameters);

                // Save the updated node
                // Use await with ConfigureAwait(false) for library code
                bool success = await graphObjectMapper.SaveNodeAsync(updatedNode).ConfigureAwait(false);
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

        private async Task<string> AddEdgeAsync(string payload) // Changed signature
        {
            string[] parts = payload.Split('|');
            if (parts.Length != 4 || !long.TryParse(parts[0], out long fromNodeId) || 
                !long.TryParse(parts[1], out long toNodeId) || !double.TryParse(parts[2], out double weight))
            {
                return "Invalid payload for adding an edge.";
            }
            string content = parts[3];
            Edge newEdge = new Edge(fromNodeId, toNodeId, weight, content); // Edge alias is EdgeV2

            // Use await with ConfigureAwait(false) for library code
            bool success = await graphObjectMapper.SaveEdgeAsync(newEdge).ConfigureAwait(false);
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

        private async Task<string> DeleteEdgeAsync(string payload) // Changed signature
        {
            string[] parts = payload.Split('|');
            if (parts.Length != 2 || !long.TryParse(parts[0], out long sourceNodeId) || 
                !long.TryParse(parts[1], out long destNodeId))
            {
                return "Invalid payload for deleting an edge.";
            }

            // Use await with ConfigureAwait(false) for library code
            bool success = await graphObjectMapper.DeleteEdgeAsync(sourceNodeId, destNodeId).ConfigureAwait(false);
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

        private async Task<string> EditEdgeAsync(string payload) // Changed signature
        {
            string[] parts = payload.Split('|');
            if (parts.Length != 4 || !long.TryParse(parts[0], out long sourceNodeId) || 
                !long.TryParse(parts[1], out long destNodeId) || !double.TryParse(parts[2], out double newWeight))
            {
                return "Invalid payload for editing an edge.";
            }
            string newContent = parts[3];

            // Load the existing edge to preserve its EdgeId
            // Load the existing edge to preserve its EdgeId
            // Use await with ConfigureAwait(false) for library code
            EdgeV2? existingEdge = await graphObjectMapper.GetEdgeAsync(sourceNodeId, destNodeId).ConfigureAwait(false);

            if (existingEdge == null)
            {
                return $"Failed to update edge: Edge from {sourceNodeId} to {destNodeId} not found.";
            }

            // Update the properties of the existing edge object
            existingEdge.Weight = newWeight;
            existingEdge.EdgeContent = newContent;
            // EdgeId remains the same

            // Save the modified existing edge object
            // Use await with ConfigureAwait(false) for library code
            bool success = await graphObjectMapper.SaveEdgeAsync(existingEdge).ConfigureAwait(false);
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
