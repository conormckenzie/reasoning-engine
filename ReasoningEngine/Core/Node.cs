// File: /home/user/code/reasoning-engine/ReasoningEngine/Core/Node.cs
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization; // For JsonConstructor attribute
// using Newtonsoft.Json.Converters; // Removed for StringEnumConverter

namespace ReasoningEngine
{
    // Removed NodeV1 and NodeV2 as we are starting fresh from V3 framework

    // --- Version 3 ---
    /// <summary>
    /// Represents a node in the reasoning graph (Version 3).
    /// This is the primary concrete node class, inheriting common properties from NodeBase.
    /// Nodes can represent Variables or Functions, determined by the NodeRole property.
    /// </summary>
    public class NodeV3 : NodeBase
    {
        /// <summary>
        /// Gets the version of this node structure.
        /// </summary>
        public override int Version => 3;

        /// <summary>
        /// Gets or sets the role of the node (Variable or Function).
        /// </summary>
        // Made setter public for easier deserialization (as in commit b7611d3)
        // Removed [JsonConverter(typeof(StringEnumConverter))]
        public NodeRole Role { get; set; }

        /// <summary>
        /// Gets or sets the probability distribution for Variable nodes. Null for Function nodes.
        /// </summary>
        // Properties for different roles - ensure Distribution is not nullable for Variable role
        public ProbabilityDistribution? Distribution { get; set; } // Nullable for non-Variable roles
        
        /// <summary>
        /// Gets or sets the type of function performed by Function nodes. Null for Variable nodes.
        /// </summary>
        public FunctionType? Function { get; set; } // Nullable for non-Function roles
        
        /// <summary>
        /// Gets or sets the parameters for Function nodes. Null for Variable nodes.
        /// </summary>
        public Dictionary<string, object>? FunctionParams { get; set; } // Nullable for non-Function roles

        /// <summary>
        /// Initializes a new instance of the NodeV3 class for a Variable node.
        /// </summary>
        /// <param name="id">The unique identifier for the node.</param>
        /// <param name="content">The human-readable content/description of the node.</param>
        /// <param name="domainType">The domain type for the variable's probability distribution.</param>
        public NodeV3(long id, string content, DomainType domainType) // Require DomainType for Variable
            : base(id) // Calls updated NodeBase constructor
        {
            this.Content = content;
            Role = NodeRole.Variable;
            Distribution = new ProbabilityDistribution(domainType); // Ensure Distribution is created
            Function = null; // Ensure function properties are null
            FunctionParams = null;
        }

        /// <summary>
        /// Initializes a new instance of the NodeV3 class for a Function node.
        /// </summary>
        /// <param name="id">The unique identifier for the node.</param>
        /// <param name="content">The human-readable content/description of the node.</param>
        /// <param name="functionType">The type of function performed by the node.</param>
        /// <param name="parameters">Optional dictionary of parameters for the function.</param>
         public NodeV3(long id, string content, FunctionType functionType, Dictionary<string, object>? parameters = null)
            : base(id) // Calls updated NodeBase constructor
        {
            this.Content = content;
            Role = NodeRole.Function;
            Function = functionType;
            Distribution = null; // Ensure distribution is null
            FunctionParams = parameters ?? new Dictionary<string, object>(); // Ensure params dict exists
        }

        /// <summary>
        /// Private constructor used by System.Text.Json for deserialization.
        /// Performs basic validation and re-parses FunctionParams after loading.
        /// </summary>
        /// <param name="id">The unique identifier for the node.</param>
        /// <param name="content">The human-readable content/description of the node.</param>
        /// <param name="role">The role of the node.</param>
        /// <param name="distribution">The probability distribution (for Variable nodes).</param>
        /// <param name="function">The function type (for Function nodes).</param>
        /// <param name="functionParams">The function parameters (for Function nodes).</param>
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

    /// <summary>
    /// Alias for the latest version of the Node class (currently NodeV3) for easier use.
    /// </summary>
    public class Node : NodeV3
    {
         /// <summary>
         /// Initializes a new instance of the Node alias class for a Variable node, forwarding to the latest concrete implementation.
         /// </summary>
         /// <param name="id">The unique identifier for the node.</param>
         /// <param name="content">The human-readable content/description of the node.</param>
         /// <param name="domainType">The domain type for the variable's probability distribution.</param>
         public Node(long id, string content, DomainType domainType)
            : base(id, content, domainType) {} // Calls NodeV3 Variable constructor

         /// <summary>
         /// Initializes a new instance of the Node alias class for a Function node, forwarding to the latest concrete implementation.
         /// </summary>
         /// <param name="id">The unique identifier for the node.</param>
         /// <param name="content">The human-readable content/description of the node.</param>
         /// <param name="functionType">The type of function performed by the node.</param>
         /// <param name="parameters">Optional dictionary of parameters for the function.</param>
         public Node(long id, string content, FunctionType functionType, Dictionary<string, object>? parameters = null)
            : base(id, content, functionType, parameters) {} // Calls NodeV3 Function constructor
    }
}
