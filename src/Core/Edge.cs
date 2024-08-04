// Class to represent an edge in the graph
public class Edge
{
    public long FromNode { get; set; }
    public long ToNode { get; set; }
    public double Weight { get; set; }
    public string EdgeContent { get; set; } = string.Empty;

    public Edge(long fromNode, long toNode, double weight, string edgeContent)
    {
        FromNode = fromNode;
        ToNode = toNode;
        Weight = weight;
        EdgeContent = edgeContent;
    }
}

