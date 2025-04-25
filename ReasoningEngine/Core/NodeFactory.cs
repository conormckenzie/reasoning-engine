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
        /// <param name="parameters">Dictionary containing node role and role-specific parameters.
        /// Expected keys:
        ///   - "Role" (string, required): "Variable" or "Function".
        ///   - "VariableDomainType" (string, optional, for Variable role): Name of the DomainType enum (e.g., "Truth", "Continuous"). Defaults to Truth.
        ///   - "FunctionType" (string, required for Function role): Name of the FunctionType enum (e.g., "Linear", "DefinedOp").
        ///   - "FunctionParams" (Dictionary<string, object> or string, optional, for Function role): Parameters specific to the FunctionType. 
        ///     If provided as a string (e.g., from CommandProcessor), it's expected in the format "Key1:Value1;Key2:Value2;...".
        ///     The factory parses this string into the required Dictionary<string, object>.
        ///     - For `DefinedOp`: Expected key `"Operation"` (string, e.g., "Multiply", "Add", "Sigmoid").
        ///     - For `Linear`: Expected keys `"Weights"` (`List<double>` or comma-separated string) and `"Bias"` (`double` or string convertible to double).
        ///     - TODO: Define parameters for `NeuralNet`.
        /// </param>
        /// <returns>A new Node instance (which is NodeV3).</returns>
        /// <exception cref="ArgumentException">Thrown if the parameters are invalid.</exception>
        public static Node CreateNodeFromPayload(long id, string content, Dictionary<string, object> parameters) 
        {
            // Expected keys in parameters dictionary:
            // "Role" (string, required)
            // "VariableDomainType" (string, optional, for Variable role)
            // "FunctionType" (string, required for Function role)
            // "FunctionParams" (Dictionary<string, object>, optional, for Function role)

            if (!parameters.TryGetValue("Role", out object? roleObj) || !(roleObj is string roleStr))
            {
                throw new ArgumentException("Node 'Role' must be specified as a string in parameters.");
            }

            if (!Enum.TryParse<NodeRole>(roleStr, true, out NodeRole role))
            {
                 throw new ArgumentException($"Invalid NodeRole specified: '{roleStr}'.");
            }

            Node newNode; 
            switch (role)
            {
                case NodeRole.Variable:
                    DomainType domainType = DomainType.Truth; // Default
                    if (parameters.TryGetValue("VariableDomainType", out object? domainObj) && domainObj is string domainStr)
                    {
                         if (!Enum.TryParse<DomainType>(domainStr, true, out DomainType parsedDomain))
                         {
                             throw new ArgumentException($"Invalid DomainType specified for Variable node: '{domainStr}'.");
                         }
                         domainType = parsedDomain;
                    } // If key not present or wrong type, use default
                    
                    newNode = new Node(id, content, domainType); 
                    return newNode;

                case NodeRole.Function:
                    if (!parameters.TryGetValue("FunctionType", out object? funcTypeObj) || !(funcTypeObj is string funcTypeStr))
                    {
                         throw new ArgumentException("Function node requires 'FunctionType' string parameter.");
                    }
                    if (!Enum.TryParse<FunctionType>(funcTypeStr, true, out FunctionType functionType))
                    {
                         throw new ArgumentException($"Invalid FunctionType specified: '{funcTypeStr}'.");
                    }

                    Dictionary<string, object> rawFunctionParams = new Dictionary<string, object>();
                    if (parameters.TryGetValue("FunctionParams", out object? funcParamsObj))
                    {
                        if (funcParamsObj is Dictionary<string, object> dictParams)
                        {
                            rawFunctionParams = dictParams;
                            DebugWriter.DebugWriteLine("#FUNC_PARAM_DICT#", $"Received FunctionParams as Dictionary for node {id}.", true, VerbosityLevel.Detailed);
                        }
                        else if (funcParamsObj is string strParams)
                        {
                            try
                            {
                                rawFunctionParams = ParseFunctionParamsString(strParams);
                                DebugWriter.DebugWriteLine("#FUNC_PARAM_STR#", $"Received and parsed FunctionParams string for node {id}: '{strParams}'.", true, VerbosityLevel.Detailed);
                            }
                            catch (ArgumentException ex)
                            {
                                throw new ArgumentException($"Invalid FunctionParams string format: {ex.Message}", ex);
                            }
                        }
                        else if (funcParamsObj != null) // Handle unexpected types
                        {
                             throw new ArgumentException("'FunctionParams' must be a Dictionary<string, object> or a string in 'Key1:Value1;...' format.");
                        }
                        // If funcParamsObj is null, rawFunctionParams remains empty, which is fine.
                    }
                    // If "FunctionParams" key doesn't exist, rawFunctionParams remains empty.

                    // Validate and parse the raw parameters (which might contain strings needing conversion)
                    Dictionary<string, object>? finalFunctionParams;
                    try
                    {
                        finalFunctionParams = ParseFunctionParameters(functionType, rawFunctionParams);
                        DebugWriter.DebugWriteLine("#FUNC_PARAM_PARSE#", $"Parsed {finalFunctionParams.Count} final function parameters for node {id}.", true, VerbosityLevel.Detailed);
                    }
                    catch (ArgumentException ex)
                    {
                        throw new ArgumentException($"Invalid FunctionParams content for FunctionType '{functionType}': {ex.Message}", ex);
                    }
                    
                    newNode = new Node(id, content, functionType, finalFunctionParams); 
                    return newNode;

                // Add cases for other roles as needed

                default:
                    throw new ArgumentException($"Unsupported NodeRole '{role}' for node creation.");
            }
        }

        /// <summary>
        /// Creates an updated Node instance based on an existing node and update parameters.
        /// </summary>
        /// <param name="existingNode">The current Node object.</param>
        /// <param name="newContent">The new content for the node (required).</param>
        /// <param name="updateParameters">Dictionary containing optional parameters to update. 
        /// Keys match CreateNodeFromPayload. Only provided keys are considered for update.
        ///   - "Role": If provided and different, resets role-specific properties (Distribution or Function/Params).
        ///   - "VariableDomainType": Only used if Role is changed to Variable. Ignored otherwise.
        ///   - "FunctionType": If provided and different, resets FunctionParams.
        ///   - "FunctionParams": Replaces existing parameters if provided (and FunctionType didn't change). Can be set to null to clear. Can be provided as a Dictionary<string, object> or a string in the format "Key1:Value1;Key2:Value2;...".
        /// </param>
        /// <returns>A new, updated Node instance.</returns>
        /// <exception cref="ArgumentException">Thrown if the parameters are invalid.</exception>
        // Changed parameter type from Node to NodeV3
        public static Node UpdateNodeFromPayload(NodeV3 existingNode, string newContent, Dictionary<string, object> updateParameters) 
        {
            // Parameters that can be updated: "Role", "VariableDomainType", "FunctionType", "FunctionParams"
            // If "Role" is provided and different, other relevant properties might be reset.

            NodeRole targetRole = existingNode.Role;
            bool roleChanged = false;

            // Check if Role is being updated
            if (updateParameters.TryGetValue("Role", out object? roleObj) && roleObj is string roleStr)
            {
                 if (!Enum.TryParse<NodeRole>(roleStr, true, out NodeRole parsedRole))
                 {
                      throw new ArgumentException($"Invalid NodeRole specified for update: '{roleStr}'.");
                 }
                 if (parsedRole != targetRole) {
                     targetRole = parsedRole;
                     roleChanged = true;
                 }
            }

            switch (targetRole)
            {
                case NodeRole.Variable:
                    // Start with existing distribution if role didn't change, else null
                    ProbabilityDistribution? dist = !roleChanged ? existingNode.Distribution : null;
                    DomainType domainType = dist?.DomainType ?? DomainType.Truth; // Use existing or default

                    // Check if DomainType is specified *and* we are changing role (resetting dist)
                    if (roleChanged && updateParameters.TryGetValue("VariableDomainType", out object? domainObj) && domainObj is string domainStr)
                    {
                         if (!Enum.TryParse<DomainType>(domainStr, true, out DomainType parsedDomain))
                         {
                             throw new ArgumentException($"Invalid DomainType specified for Variable node update: '{domainStr}'.");
                         }
                         domainType = parsedDomain; // Set domain for the new distribution
                    }
                    else if (updateParameters.ContainsKey("VariableDomainType") && !roleChanged)
                    {
                         // Warn if trying to change domain without changing role
                         DebugWriter.DebugWriteLine("#EDIT_WARN_DOMAIN#", $"Attempted to change DomainType for existing distribution on node {existingNode.Id}. Domain not changed.");
                    }
                     
                    // Create the new node (constructor creates distribution if needed)
                    var updatedVarNode = new Node(existingNode.Id, newContent, domainType); 
                    // If role didn't change and distribution existed, copy it back
                    if (!roleChanged && dist != null) {
                        updatedVarNode.Distribution = dist;
                    }
                    // Copy ExtendedProperties using the public setter method
                    foreach (var kvp in existingNode.ExtendedProperties)
                    {
                        updatedVarNode.SetExtendedProperty(kvp.Key, kvp.Value);
                    }
                    return updatedVarNode;

                case NodeRole.Function:
                     // Start with existing function info if role didn't change
                     FunctionType funcType = !roleChanged ? (existingNode.Function ?? FunctionType.Linear) : FunctionType.Linear; // Default if changing role
                     Dictionary<string, object>? funcParams = !roleChanged ? existingNode.FunctionParams : null; 
                     bool functionTypeChanged = false;

                     // Check if FunctionType is being updated
                     if (updateParameters.TryGetValue("FunctionType", out object? funcTypeObj) && funcTypeObj is string funcTypeStr)
                     {
                          if (!Enum.TryParse<FunctionType>(funcTypeStr, true, out FunctionType parsedFuncType))
                          {
                               throw new ArgumentException($"Invalid FunctionType specified for Function node update: '{funcTypeStr}'.");
                          }
                          if (parsedFuncType != funcType) {
                              funcType = parsedFuncType;
                              functionTypeChanged = true;
                              funcParams = null; // Reset params if type changes
                          }
                     }

                     // Check if FunctionParams are being updated (only if type didn't just change)
                     if (!functionTypeChanged && updateParameters.TryGetValue("FunctionParams", out object? funcParamsObj))
                     {
                         Dictionary<string, object>? rawFunctionParams = null; // Holds params before final parsing

                         if (funcParamsObj is Dictionary<string, object> dictParams) {
                             rawFunctionParams = dictParams;
                             DebugWriter.DebugWriteLine("#FUNC_PARAM_DICT_EDIT#", $"Received FunctionParams update as Dictionary for node {existingNode.Id}.", true, VerbosityLevel.Detailed);
                         } 
                         else if (funcParamsObj is string strParams) {
                             try
                             {
                                 rawFunctionParams = ParseFunctionParamsString(strParams);
                                 DebugWriter.DebugWriteLine("#FUNC_PARAM_STR_EDIT#", $"Received and parsed FunctionParams update string for node {existingNode.Id}: '{strParams}'.", true, VerbosityLevel.Detailed);
                             }
                             catch (ArgumentException ex)
                             {
                                 throw new ArgumentException($"Invalid FunctionParams string format for update: {ex.Message}", ex);
                             }
                         } 
                         else if (funcParamsObj == null) {
                             // Explicitly clearing parameters
                             funcParams = null; 
                             rawFunctionParams = null; // Ensure raw is also null for consistency below
                             DebugWriter.DebugWriteLine("#FUNC_PARAM_CLEAR_EDIT#", $"Cleared function parameters for node {existingNode.Id}.", true, VerbosityLevel.Detailed);
                         } 
                         else {
                             // Invalid type provided
                             throw new ArgumentException("'FunctionParams' for update must be a Dictionary<string, object>, a string ('Key1:Value1;...'), or null.");
                         }

                         // If parameters were provided (not cleared), parse them
                         if (rawFunctionParams != null) 
                         {
                             try
                             {
                                 // Parse and validate the raw parameters (which might contain strings needing conversion)
                                 funcParams = ParseFunctionParameters(funcType, rawFunctionParams); // Use current funcType
                                 DebugWriter.DebugWriteLine("#FUNC_PARAM_PARSE_EDIT#", $"Updated and parsed function parameters for node {existingNode.Id}.", true, VerbosityLevel.Detailed);
                             }
                             catch (ArgumentException ex)
                             {
                                 throw new ArgumentException($"Invalid FunctionParams content for update (FunctionType '{funcType}'): {ex.Message}", ex);
                             }
                         }
                         // If rawFunctionParams is null here, it means funcParamsObj was null, so funcParams remains null (cleared).
                     }
                     // Ensure funcParams is not null if role is Function, even if cleared or unchanged (Node constructor expects non-null)
                     if (targetRole == NodeRole.Function && funcParams == null)
                     {
                         funcParams = new Dictionary<string, object>();
                     }

                    var updatedFuncNode = new Node(existingNode.Id, newContent, funcType, funcParams);
                    // Copy ExtendedProperties using the public setter method
                    foreach (var kvp in existingNode.ExtendedProperties)
                    {
                        updatedFuncNode.SetExtendedProperty(kvp.Key, kvp.Value);
                    }
                    return updatedFuncNode;

                default:
                     throw new ArgumentException($"Unsupported NodeRole '{targetRole}' for node update.");
            }
            // Removed TODO comment as ExtendedProperties are now copied
        }

        /// <summary>
        /// Parses a string representation of function parameters into a dictionary.
        /// Expected format: "Key1:Value1;Key2:Value2;..."
        /// </summary>
        /// <param name="paramString">The parameter string.</param>
        /// <returns>A dictionary with string keys and string values.</returns>
        /// <exception cref="ArgumentException">Thrown if the format is invalid.</exception>
        private static Dictionary<string, object> ParseFunctionParamsString(string paramString)
        {
            var parsedParams = new Dictionary<string, object>();
            if (string.IsNullOrWhiteSpace(paramString))
            {
                return parsedParams; // Return empty dictionary for empty/whitespace string
            }

            var pairs = paramString.Split(';');
            foreach (var pair in pairs)
            {
                if (string.IsNullOrWhiteSpace(pair)) continue; // Skip empty segments

                var parts = pair.Split(':');
                if (parts.Length != 2)
                {
                    throw new ArgumentException($"Invalid parameter segment format: '{pair}'. Expected 'Key:Value'.");
                }

                var key = parts[0].Trim();
                var value = parts[1].Trim(); // Keep value as string for later type-specific parsing

                if (string.IsNullOrEmpty(key))
                {
                     throw new ArgumentException($"Invalid parameter segment: '{pair}'. Key cannot be empty.");
                }

                if (parsedParams.ContainsKey(key))
                {
                    // Overwrite or throw? Let's overwrite for flexibility, like standard dictionaries.
                    DebugWriter.DebugWriteLine("#PARAM_OVERWRITE#", $"Duplicate key '{key}' found in FunctionParams string. Using last value '{value}'.", true, VerbosityLevel.Detailed);
                }
                parsedParams[key] = value;
            }
            return parsedParams;
        }


        // --- Private Helper Methods for Parameter Parsing ---

        private static Dictionary<string, object> ParseFunctionParameters(FunctionType type, Dictionary<string, object> rawParams)
        {
            switch (type)
            {
                case FunctionType.Linear:
                    return ParseLinearParams(rawParams);
                case FunctionType.DefinedOp:
                    return ParseDefinedOpParams(rawParams);
                case FunctionType.NeuralNet:
                    // TODO: Implement NeuralNet parameter parsing
                    DebugWriter.DebugWriteLine("#NN_PARAM_TODO#", "NeuralNet parameter parsing not yet implemented.", true, VerbosityLevel.Minimal);
                    return rawParams; // Return raw params for now
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), $"Unsupported FunctionType for parameter parsing: {type}");
            }
        }

        private static Dictionary<string, object> ParseLinearParams(Dictionary<string, object> rawParams)
        {
            var parsedParams = new Dictionary<string, object>();

            // Parse Weights
            if (!rawParams.TryGetValue("Weights", out object? weightsObj))
            {
                throw new ArgumentException("Missing required parameter 'Weights' for Linear function.");
            }
            if (weightsObj is List<double> weightsList)
            {
                parsedParams["Weights"] = weightsList;
            }
            else if (weightsObj is string weightsStr)
            {
                try
                {
                    parsedParams["Weights"] = weightsStr.Split(',')
                                                        .Select(s => double.Parse(s.Trim()))
                                                        .ToList();
                }
                catch (Exception ex) when (ex is FormatException || ex is OverflowException)
                {
                    throw new ArgumentException($"Invalid format for 'Weights' string parameter. Expected comma-separated doubles. Error: {ex.Message}", ex);
                }
            }
            else
            {
                throw new ArgumentException("'Weights' parameter must be a List<double> or a comma-separated string of doubles.");
            }

            // Parse Bias
            if (!rawParams.TryGetValue("Bias", out object? biasObj))
            {
                throw new ArgumentException("Missing required parameter 'Bias' for Linear function.");
            }
             if (biasObj is double biasDouble)
            {
                parsedParams["Bias"] = biasDouble;
            }
            else if (biasObj is string biasStr)
            {
                 try
                 {
                     parsedParams["Bias"] = double.Parse(biasStr.Trim());
                 }
                 catch (Exception ex) when (ex is FormatException || ex is OverflowException)
                 {
                     throw new ArgumentException($"Invalid format for 'Bias' string parameter. Expected a double. Error: {ex.Message}", ex);
                 }
            }
             else if (biasObj is int biasInt) // Allow int implicitly convertible to double
             {
                 parsedParams["Bias"] = (double)biasInt;
             }
             else if (biasObj is long biasLong) // Allow long implicitly convertible to double
             {
                  parsedParams["Bias"] = (double)biasLong;
             }
            else
            {
                throw new ArgumentException("'Bias' parameter must be a double or a string convertible to double.");
            }

            // Copy any other parameters directly (though none are expected for Linear currently)
            foreach(var kvp in rawParams)
            {
                if (!parsedParams.ContainsKey(kvp.Key))
                {
                    parsedParams[kvp.Key] = kvp.Value;
                }
            }

            return parsedParams;
        }

        private static Dictionary<string, object> ParseDefinedOpParams(Dictionary<string, object> rawParams)
        {
             var parsedParams = new Dictionary<string, object>();

             if (!rawParams.TryGetValue("Operation", out object? opObj) || !(opObj is string opStr))
             {
                 throw new ArgumentException("Missing or invalid 'Operation' string parameter for DefinedOp function.");
             }
             // TODO: Could add validation here to check if opStr is a known/supported operation name
             parsedParams["Operation"] = opStr;

             // Copy any other parameters directly
             foreach(var kvp in rawParams)
             {
                 if (!parsedParams.ContainsKey(kvp.Key))
                 {
                     parsedParams[kvp.Key] = kvp.Value;
                 }
             }

             return parsedParams;
        }
    }
}
