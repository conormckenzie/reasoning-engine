using System; // For Guid
using System.Text.Json.Serialization; // Changed for JsonConstructor attribute

namespace ReasoningEngine
{
    // Removed EdgeV1 as we are starting fresh from V3 framework (conceptually V2 edges)

    /// <summary>
    /// Represents a directed edge in the reasoning graph (Version 2).
    /// Edges connect nodes and represent dependencies or the flow of information.
    /// Inherits common properties from EdgeBase.
    /// </summary>
    public class EdgeV2 : EdgeBase // Keep V2 as the current concrete implementation
    {
        /// <summary>
        /// Gets the version of this edge structure.
        /// </summary>
        public override int Version => 2; // Keep version number 2 for now
        
        /// <summary>
        /// Gets or sets the weight of the edge, representing the strength or confidence of the dependency.
        /// </summary>
        public double Weight { get; set; }
        
        /// <summary>
        /// Gets or sets a string providing a human-readable description or label for the edge.
        /// </summary>
        public string EdgeContent { get; set; }
        // Placeholder for future additions
        // public DateTime CreationDate { get; set; }

        /// <summary>
        /// Initializes a new instance of the EdgeV2 class with a newly generated unique identifier.
        /// </summary>
        /// <param name="fromNode">The ID of the source node.</param>
        /// <param name="toNode">The ID of the destination node.</param>
        /// <param name="weight">The weight of the edge.</param>
        /// <param name="edgeContent">The content/description of the edge.</param>
        public EdgeV2(long fromNode, long toNode, double weight, string edgeContent) 
            : base(fromNode, toNode) // Calls EdgeBase constructor that generates Guid
        {
            Weight = weight;
            EdgeContent = edgeContent;
            // CreationDate = DateTime.Now;
        }

        /// <summary>
        /// Initializes a new instance of the EdgeV2 class with a specified unique identifier (used during loading).
        /// </summary>
        /// <param name="edgeId">The unique identifier for the edge.</param>
        /// <param name="fromNode">The ID of the source node.</param>
        /// <param name="toNode">The ID of the destination node.</param>
        /// <param name="weight">The weight of the edge.</param>
        /// <param name="edgeContent">The content/description of the edge.</param>
         [System.Text.Json.Serialization.JsonConstructor] // Changed attribute
         public EdgeV2(Guid edgeId, long fromNode, long toNode, double weight, string edgeContent) 
            : base(edgeId, fromNode, toNode) // Calls EdgeBase constructor that accepts Guid
        {
            Weight = weight;
            EdgeContent = edgeContent;
            // Potentially load CreationDate if/when added
        }

        /// <summary>
        /// Upgrades this edge instance to the latest version.
        /// Since this is currently the latest version, it returns itself.
        /// </summary>
        /// <returns>The current EdgeV2 instance.</returns>
        public override EdgeBase UpgradeToLatest()
        {
            return this; // Already the latest version
        }
    }

    /// <summary>
    /// Alias for the latest version of the Edge class (currently EdgeV2) for easier use.
    /// </summary>
    public class Edge : EdgeV2 // Alias still points to V2
    {
        /// <summary>
        /// Initializes a new instance of the Edge alias class, forwarding to the latest concrete implementation.
        /// </summary>
        /// <param name="fromNode">The ID of the source node.</param>
        /// <param name="toNode">The ID of the destination node.</param>
        /// <param name="weight">The weight of the edge.</param>
        /// <param name="edgeContent">The content/description of the edge.</param>
        public Edge(long fromNode, long toNode, double weight, string edgeContent)
            : base(fromNode, toNode, weight, edgeContent) { }

        // Removed commented-out constructor for loading with Guid.
        // Deserialization targets EdgeV2 directly via its [JsonConstructor].
    }
}
