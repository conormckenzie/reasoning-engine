// File: /home/user/code/reasoning-engine/ReasoningEngine/Core/Node.cs
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization; // For JsonConstructor attribute
// using Newtonsoft.Json.Converters; // Removed for StringEnumConverter

namespace ReasoningEngine
{
    // Removed NodeV1 and NodeV2 as we are starting fresh from V3 framework

    // --- Version 3 ---
    // This is now the primary concrete node class
    public class NodeV3 : NodeBase 
    {
        // Override the abstract Version property from NodeBase
        public override int Version => 3; 

        // Made setter public for easier deserialization (as in commit b7611d3)
        // Removed [JsonConverter(typeof(StringEnumConverter))]
        public NodeRole Role { get; set; } 

        // Properties for different roles - ensure Distribution is not nullable for Variable role
        public ProbabilityDistribution? Distribution { get; set; } // Nullable for non-Variable roles
        public FunctionType? Function { get; set; } // Nullable for non-Function roles
        public Dictionary<string, object>? FunctionParams { get; set; } // Nullable for non-Function roles

        // Constructor for Variable role 
        public NodeV3(long id, string content, DomainType domainType) // Require DomainType for Variable
            : base(id) // Calls updated NodeBase constructor
        {
            this.Content = content; 
            Role = NodeRole.Variable;
            Distribution = new ProbabilityDistribution(domainType); // Ensure Distribution is created
            Function = null; // Ensure function properties are null
            FunctionParams = null;
        }

        // Constructor for Function role
         public NodeV3(long id, string content, FunctionType functionType, Dictionary<string, object>? parameters = null)
            : base(id) // Calls updated NodeBase constructor
        {
            this.Content = content;
            Role = NodeRole.Function;
            Function = functionType;
            Distribution = null; // Ensure distribution is null
            FunctionParams = parameters ?? new Dictionary<string, object>(); // Ensure params dict exists
        }

        // Private constructor for JSON deserialization (as in commit b7611d3)
        [System.Text.Json.Serialization.JsonConstructor]
        private NodeV3(long id, string content, NodeRole role, ProbabilityDistribution? distribution, FunctionType? function, Dictionary<string, object>? functionParams)
            : base(id) // Base constructor handles Id
        {
            this.Content = content;
            this.Role = role;
            this.Distribution = distribution;
            this.Function = function;
            this.FunctionParams = functionParams;

            // Basic validation after deserialization
            if (Role == NodeRole.Variable && Distribution == null)
            {
                // Potentially throw or log an error, or try to create a default distribution?
                // For now, let it be null, but this indicates potentially corrupt data.
                 Console.Error.WriteLine($"Warning: Deserialized Variable node {id} with null Distribution.");
            }
             if (Role == NodeRole.Function && Function == null)
             {
                 Console.Error.WriteLine($"Warning: Deserialized Function node {id} with null Function type.");
             }

            // Re-process FunctionParams after deserialization to convert JsonElements
            if (this.Role == NodeRole.Function && this.Function.HasValue && this.FunctionParams != null)
            {
                try
                {
                    // Call the factory's parser which handles type conversions (e.g., string/JsonElement to double/List<double>)
                    this.FunctionParams = NodeFactory.ParseFunctionParameters(this.Function.Value, this.FunctionParams);
                    // Log success? Optional.
                    // DebugUtils.DebugWriter.DebugWriteLine("#NODE_PARAM_REPARSE#", $"Re-parsed FunctionParams for node {id} after deserialization.", true, DebugUtils.VerbosityLevel.Detailed);
                }
                catch (Exception ex)
                {
                     // Log error and potentially leave FunctionParams partially processed or null?
                     // Leaving them as they were deserialized might be safer than nulling them.
                     Console.Error.WriteLine($"Error re-parsing FunctionParams for node {id} after deserialization: {ex.Message}");
                     // Optionally: this.FunctionParams = null; // Or keep the potentially partially parsed dictionary
                }
            }
        }

        // Removed UpgradeToLatest as V1/V2 are gone and this IS the latest
    }

    // Alias the latest version as Node for easier use
    public class Node : NodeV3 
    {
         // Forward constructors to NodeV3
         public Node(long id, string content, DomainType domainType) 
            : base(id, content, domainType) {} // Calls NodeV3 Variable constructor

         public Node(long id, string content, FunctionType functionType, Dictionary<string, object>? parameters = null)
            : base(id, content, functionType, parameters) {} // Calls NodeV3 Function constructor
    }
}
