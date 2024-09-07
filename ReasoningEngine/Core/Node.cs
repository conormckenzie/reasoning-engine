namespace ReasoningEngine
{
    public class NodeV1 : NodeBase
    {
        public override int Version => 1;
        public string Content { get; set; }

        public NodeV1(long id, string content) : base(id)
        {
            Content = content;
        }

        public override NodeBase UpgradeToLatest()
        {
            return new NodeV2(this.Id, this.Content);
        }
    }

    public class NodeV2 : NodeBase
    {
        public override int Version => 2;
        public string Content { get; set; }

        public NodeV2(long id, string content) : base(id)
        {
            Content = content;
        }

        public override NodeBase UpgradeToLatest()
        {
            return this;
        }
    }

    // Alias the latest version as Node for easier use
    public class Node : NodeV2
    {
        public Node(long id, string content) : base(id, content) { }
    }
}