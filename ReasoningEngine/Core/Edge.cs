using System; // For Guid
using System.Text.Json.Serialization; // Changed for JsonConstructor attribute

namespace ReasoningEngine
{
    // Removed EdgeV1 as we are starting fresh from V3 framework (conceptually V2 edges)

    public class EdgeV2 : EdgeBase // Keep V2 as the current concrete implementation
    {
        public override int Version => 2; // Keep version number 2 for now
        public double Weight { get; set; }
        public string EdgeContent { get; set; }
        // Placeholder for future additions
        // public DateTime CreationDate { get; set; }

        // Constructor generating new Guid
        public EdgeV2(long fromNode, long toNode, double weight, string edgeContent) 
            : base(fromNode, toNode) // Calls EdgeBase constructor that generates Guid
        {
            Weight = weight;
            EdgeContent = edgeContent;
            // CreationDate = DateTime.Now;
        }

        // Constructor accepting existing Guid (for loading) - Mark for JSON deserialization (as in commit b7611d3)
         [System.Text.Json.Serialization.JsonConstructor] // Changed attribute
         public EdgeV2(Guid edgeId, long fromNode, long toNode, double weight, string edgeContent) 
            : base(edgeId, fromNode, toNode) // Calls EdgeBase constructor that accepts Guid
        {
            Weight = weight;
            EdgeContent = edgeContent;
            // Potentially load CreationDate if/when added
        }


        public override EdgeBase UpgradeToLatest()
        {
            return this; // Already the latest version
        }
    }

    // Alias the latest version as Edge for easier use
    public class Edge : EdgeV2 // Alias still points to V2
    {
        // Forward constructors to EdgeV2
        public Edge(long fromNode, long toNode, double weight, string edgeContent) 
            : base(fromNode, toNode, weight, edgeContent) { }
        
        // Add forwarding constructor for loading with Guid if needed by GraphFileManager later
        // public Edge(Guid edgeId, long fromNode, long toNode, double weight, string edgeContent) 
        //    : base(edgeId, fromNode, toNode, weight, edgeContent) { }
    }
}
