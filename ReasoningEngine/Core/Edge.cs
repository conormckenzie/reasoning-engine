namespace ReasoningEngine
{
    public class EdgeV1 : EdgeBase
    {
        public override int Version => 1;
        public double Weight { get; set; }
        public string EdgeContent { get; set; }

        public EdgeV1(long fromNode, long toNode, double weight, string edgeContent) 
            : base(fromNode, toNode)
        {
            Weight = weight;
            EdgeContent = edgeContent;
        }

        public override EdgeBase UpgradeToLatest()
        {
            return new EdgeV2(this.FromNode, this.ToNode, this.Weight, this.EdgeContent);
        }
    }

    public class EdgeV2 : EdgeBase
    {
        public override int Version => 2;
        public double Weight { get; set; }
        public string EdgeContent { get; set; }
        // Placeholder for future additions
        // public DateTime CreationDate { get; set; }

        public EdgeV2(long fromNode, long toNode, double weight, string edgeContent) 
            : base(fromNode, toNode)
        {
            Weight = weight;
            EdgeContent = edgeContent;
            // CreationDate = DateTime.Now;
        }

        public override EdgeBase UpgradeToLatest()
        {
            return this; // Already the latest version
        }
    }

    // Alias the latest version as Edge for easier use
    public class Edge : EdgeV2
    {
        public Edge(long fromNode, long toNode, double weight, string edgeContent) 
            : base(fromNode, toNode, weight, edgeContent) { }
    }
}