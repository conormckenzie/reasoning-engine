

namespace ReasoningEngine
{
    public class Node
    {
        public long Id { get; set; }
        public string Content { get; set; }

        public Node(long id, string content)
        {
            Id = id;
            Content = content;
        }
    }
}