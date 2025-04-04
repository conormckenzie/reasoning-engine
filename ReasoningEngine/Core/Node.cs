// File: /home/user/code/reasoning-engine/ReasoningEngine/Core/Node.cs
using System; // For Dictionary, Guid etc. if needed later
using System.Collections.Generic; // For Dictionary

namespace ReasoningEngine
{
    // Removed NodeV1 and NodeV2 as we are starting fresh from V3 framework

    // --- Version 3 ---
    // This is now the primary concrete node class
    public class NodeV3 : NodeBase 
    {
        // Override the abstract Version property from NodeBase
        public override int Version => 3; 

        public NodeRole Role { get; protected set; }

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
