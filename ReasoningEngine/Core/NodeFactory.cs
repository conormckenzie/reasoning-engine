using System;
using System.Collections.Generic;
using System.Linq;
using DebugUtils;

namespace ReasoningEngine
{
    /// <summary>
    /// Factory class responsible for creating and updating Node objects from payloads.
    /// </summary>
    public static class NodeFactory
    {
        /// <summary>
        /// Creates a NodeV3 instance based on payload parts.
        /// </summary>
        /// <param name="id">Node ID.</param>
        /// <param name="content">Node content.</param>
        /// <param name="payloadParts">Payload parts starting from the Role definition.</param>
        /// <returns>A new Node instance (which is NodeV3).</returns>
        /// <exception cref="ArgumentException">Thrown if the payload is invalid.</exception>
        public static Node CreateNodeFromPayload(long id, string content, string[] payloadParts) // Return type changed to Node
        {
            // Payload format assumption: Role[|SubTypeSpecificInfo...]
            // Variable: Variable[|DomainType] 
            // Function: Function|FunctionType[|param1=val1;param2=val2...]

            if (payloadParts == null || payloadParts.Length == 0)
            {
                // Default to Variable if no role specified? Or throw? Let's throw for now.
                throw new ArgumentException("Node role must be specified in the payload.");
            }

            if (!Enum.TryParse<NodeRole>(payloadParts[0], true, out NodeRole role))
            {
                 throw new ArgumentException($"Invalid NodeRole specified: '{payloadParts[0]}'.");
            }

            // Create the appropriate V3 node (using Node alias)
            Node newNode; 
            switch (role)
            {
                case NodeRole.Variable:
                    DomainType domainType = DomainType.Truth; // Default for Variable
                    if (payloadParts.Length >= 2 && !string.IsNullOrEmpty(payloadParts[1]))
                    {
                         if (!Enum.TryParse<DomainType>(payloadParts[1], true, out DomainType parsedDomain))
                         {
                             throw new ArgumentException($"Invalid DomainType specified for Variable node: '{payloadParts[1]}'.");
                         }
                         domainType = parsedDomain;
                    }
                    // Use the correct Node constructor for Variable role
                    newNode = new Node(id, content, domainType); 
                    return newNode;

                case NodeRole.Function:
                    if (payloadParts.Length < 2 || !Enum.TryParse<FunctionType>(payloadParts[1], true, out FunctionType functionType))
                    {
                         throw new ArgumentException("Function node requires a valid FunctionType as the second payload part.");
                    }
                    
                    var functionParams = new Dictionary<string, object>();
                    if (payloadParts.Length >= 3 && !string.IsNullOrEmpty(payloadParts[2]))
                    {
                        // TODO: Implement robust parameter parsing logic here
                        // Example simple parsing: "key1=value1;key2=value2"
                        try
                        {
                            functionParams = payloadParts[2].Split(';')
                                .Select(part => part.Split('='))
                                .Where(split => split.Length == 2)
                                .ToDictionary(split => split[0].Trim(), split => (object)split[1].Trim()); // Store values as string initially
                             DebugWriter.DebugWriteLine("#FUNC_PARAM_PARSE#", $"Parsed {functionParams.Count} function parameters for node {id}.", true, VerbosityLevel.Detailed);
                        }
                        catch (Exception ex)
                        {
                             throw new ArgumentException($"Error parsing function parameters '{payloadParts[2]}': {ex.Message}");
                         }
                    }
                     // Use the correct Node constructor for Function role
                    newNode = new Node(id, content, functionType, functionParams);
                    return newNode;

                // Add cases for other roles as needed

                default:
                    throw new ArgumentException($"Unsupported NodeRole '{role}' for node creation.");
            }
        }

        /// <summary>
        /// Creates an updated NodeV3 instance based on an existing node and payload parts.
        /// </summary>
        /// <param name="existingNodeV3">The current NodeV3 object.</param>
        /// <param name="newContent">The new content for the node.</param>
        /// <param name="payloadParts">Payload parts starting from the potential new Role definition.</param>
        /// <returns>A new, updated Node instance (which is NodeV3).</returns>
        /// <exception cref="ArgumentException">Thrown if the payload is invalid.</exception>
        public static Node UpdateNodeFromPayload(Node existingNode, string newContent, string[] payloadParts) // Parameter and Return type changed to Node
        {
             // Payload format assumption: [NewRole][|SubTypeSpecificInfo...]
             // If NewRole is omitted, role is unchanged.
             // If Role is Variable: [Variable][|NewDomainType] (Only changes domain if distribution is reset)
             // If Role is Function: [Function][|NewFunctionType][|params...]

            NodeRole targetRole = existingNode.Role; // Use existingNode
            int payloadStartIndex = 0;

            // Check if the first part specifies a new role
            if (payloadParts != null && payloadParts.Length > 0 && Enum.TryParse<NodeRole>(payloadParts[0], true, out NodeRole parsedRole))
            {
                targetRole = parsedRole;
                payloadStartIndex = 1; // Start parsing subtype info from the next part
            }
            
            // Get remaining parts for subtype parsing
            string[] subTypePayload = payloadParts?.Skip(payloadStartIndex).ToArray() ?? Array.Empty<string>();

            switch (targetRole)
            {
                case NodeRole.Variable:
                    // Preserve existing distribution if role hasn't changed, otherwise start fresh
                    ProbabilityDistribution? dist = (targetRole == existingNode.Role) ? existingNode.Distribution : null; // Use existingNode
                    DomainType domainType = dist?.DomainType ?? DomainType.Truth; // Default if creating new

                    // Allow changing domain type only if distribution is being reset (role changed or was null)
                     if (subTypePayload.Length >= 1 && !string.IsNullOrEmpty(subTypePayload[0]))
                     {
                         if (!Enum.TryParse<DomainType>(subTypePayload[0], true, out DomainType parsedDomain))
                         {
                             throw new ArgumentException($"Invalid DomainType specified for Variable node update: '{subTypePayload[0]}'.");
                         }
                         if (dist == null || targetRole != existingNode.Role) { // Use existingNode
                            domainType = parsedDomain;
                         } else if (domainType != parsedDomain) {
                             // Optionally warn: Cannot change DomainType of existing distribution via edit.
                              DebugWriter.DebugWriteLine("#EDIT_WARN_DOMAIN#", $"Attempted to change DomainType for existing distribution on node {existingNode.Id}. Domain not changed."); // Use existingNode
                         }
                     }
                     
                    // Create new Variable node using appropriate constructor
                    // Note: We create a new distribution if role changed or didn't exist, 
                    // but the constructor *always* creates one. We need to assign the old one back if preserved.
                    var updatedVarNode = new Node(existingNode.Id, newContent, domainType); 
                    if (dist != null && targetRole == existingNode.Role) {
                        updatedVarNode.Distribution = dist; // Re-assign preserved distribution
                    }
                    return updatedVarNode;
                    
                case NodeRole.Function:
                     FunctionType funcType = existingNode.Function ?? FunctionType.Linear; // Use existingNode, Default if changing role
                     Dictionary<string, object>? funcParams = (targetRole == existingNode.Role) ? existingNode.FunctionParams : null; // Use existingNode
                     bool functionTypeChanged = false;

                     // Allow changing function type if specified
                     if (subTypePayload.Length >= 1 && !string.IsNullOrEmpty(subTypePayload[0]))
                     {
                          if (!Enum.TryParse<FunctionType>(subTypePayload[0], true, out FunctionType parsedFuncType))
                          {
                               throw new ArgumentException($"Invalid FunctionType specified for Function node update: '{subTypePayload[0]}'.");
                          }
                          // Check if the type actually changed from the existing node's function type
                          if (existingNode.Function != parsedFuncType) { 
                              funcType = parsedFuncType;
                              functionTypeChanged = true;
                              funcParams = null; // Reset params if type changes
                          }
                     }

                     // Parse new parameters if provided (and function type didn't just change)
                     if (!functionTypeChanged && subTypePayload.Length >= 2 && !string.IsNullOrEmpty(subTypePayload[1]))
                     {
                         // TODO: Implement robust parameter parsing logic here
                         try
                         {
                             funcParams = subTypePayload[1].Split(';')
                                 .Select(part => part.Split('='))
                                 .Where(split => split.Length == 2)
                                 .ToDictionary(split => split[0].Trim(), split => (object)split[1].Trim()); // Store as string initially
                              DebugWriter.DebugWriteLine("#FUNC_PARAM_PARSE_EDIT#", $"Parsed {funcParams.Count} new function parameters for node {existingNode.Id}.", true, VerbosityLevel.Detailed); // Use existingNode
                         }
                         catch (Exception ex)
                         {
                              throw new ArgumentException($"Error parsing function parameters '{subTypePayload[1]}': {ex.Message}");
                         }
                     }
                     funcParams ??= new Dictionary<string, object>(); // Ensure not null

                    // Use the correct Node constructor for Function role
                    return new Node(existingNode.Id, newContent, funcType, funcParams); 

                default:
                     throw new ArgumentException($"Unsupported NodeRole '{targetRole}' for node update.");
            }
            // TODO: Copy ExtendedProperties from existingNode if needed (or handle in NodeBase/V3 constructor)
        }
    }
}
