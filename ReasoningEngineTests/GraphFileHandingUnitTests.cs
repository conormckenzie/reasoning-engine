using NUnit.Framework;
using NUnit.Framework.Legacy;
using ReasoningEngine;
using ReasoningEngine.GraphFileHandling;
using System.Collections.Generic;

namespace ReasoningEngineTests
{
    [TestFixture]
    public class UnitTest1
    {
        private GraphFileManager graphFileManager;

        [SetUp]
        public void Setup()
        {
            graphFileManager = new GraphFileManager("data");
        }
        
        /// <summary>
        /// Tests saving and loading a single node.
        /// Verifies that the node remains unchanged after saving and loading.
        /// (Load(Save(node)) == node)
        /// </summary>
        [Test]
        public void TestSaveAndLoadSingleNode()
        {
            var node = new Node(1234567890123456, "Test Node");
            ClassicAssert.IsTrue(graphFileManager.SaveNode(node.Id, node.Content));
            var loadedNode = graphFileManager.LoadNode(node.Id);
            ClassicAssert.IsNotNull(loadedNode);
            ClassicAssert.AreEqual(node.Id, loadedNode.Id);
            ClassicAssert.AreEqual(node.Content, loadedNode.Content);
        }

        /// <summary>
        /// Tests saving and loading multiple nodes.
        /// Verifies that for each node, the loaded node remains unchanged after saving and loading.
        /// ∀ node ∈ nodes: Load(Save(node)) == node
        /// </summary>
        [Test]
        public void TestSaveAndLoadMultipleNodes()
        {
            var nodes = new List<Node>
            {
                new Node(1234567890123456, "Test Node 1"),
                new Node(2345678901234567, "Test Node 2"),
                new Node(3456789012345678, "Test Node 3")
            };

            foreach (var node in nodes)
            {
                ClassicAssert.IsTrue(graphFileManager.SaveNode(node.Id, node.Content));
            }

            foreach (var node in nodes)
            {
                var loadedNode = graphFileManager.LoadNode(node.Id);
                ClassicAssert.IsNotNull(loadedNode);
                ClassicAssert.AreEqual(node.Id, loadedNode.Id);
                ClassicAssert.AreEqual(node.Content, loadedNode.Content);
            }
        }

        /// <summary>
        /// Tests saving and loading a node with special characters in its content.
        /// Verifies that nodes with content containing special characters are correctly saved and loaded.
        /// Special characters tested: !@#$%^&*()
        /// Load(Save(node_with_special_characters)) == node_with_special_characters
        /// </summary>
        [Test]
        public void TestSaveAndLoadNodeWithSpecialCharacters()
        {
            var node = new Node(1234567890123456, "Test Node!@#$%^&*()");
            ClassicAssert.IsTrue(graphFileManager.SaveNode(node.Id, node.Content));
            var loadedNode = graphFileManager.LoadNode(node.Id);
            ClassicAssert.IsNotNull(loadedNode);
            ClassicAssert.AreEqual(node.Id, loadedNode.Id);
            ClassicAssert.AreEqual(node.Content, loadedNode.Content);
        }

        [Test]
        public void TestLoadNodeThatDoesNotExist()
        {
            var loadedNode = graphFileManager.LoadNode(9999999999999999);
            ClassicAssert.IsNull(loadedNode);
        }

        /// <summary>
        /// Tests saving and loading a single edge.
        /// Verifies that the edge remains unchanged after saving and loading.
        /// (Load(Save(edge)) == edge)
        /// </summary>
        [Test]
        public void TestSaveAndLoadSingleEdge()
        {
            var edge = new Edge(1234567890123456, 6543210987654321, 1.5, "Test Edge");
            ClassicAssert.IsTrue(graphFileManager.SaveEdge(edge));
            var loadedEdges = graphFileManager.LoadEdges(edge.FromNode);
            ClassicAssert.IsNotEmpty(loadedEdges);
            var loadedEdge = loadedEdges[0];
            ClassicAssert.AreEqual(edge.FromNode, loadedEdge.FromNode);
            ClassicAssert.AreEqual(edge.ToNode, loadedEdge.ToNode);
            ClassicAssert.AreEqual(edge.Weight, loadedEdge.Weight);
            ClassicAssert.AreEqual(edge.EdgeContent, loadedEdge.EdgeContent);
        }

        /// <summary>
        /// Tests saving and loading multiple edges.
        /// Verifies that for each edge, the loaded edge remains unchanged after saving and loading.
        /// ∀ edge ∈ edges: Load(Save(edge)) == edge
        /// </summary>
        [Test]
        public void TestSaveAndLoadMultipleEdges()
        {
            var edges = new List<Edge>
            {
                new Edge(1234567890123456, 6543210987654321, 1.5, "Test Edge 1"),
                new Edge(1234567890123456, 7654321098765432, 2.0, "Test Edge 2"),
                new Edge(1234567890123456, 8765432109876543, 2.5, "Test Edge 3")
            };

            foreach (var edge in edges)
            {
                ClassicAssert.IsTrue(graphFileManager.SaveEdge(edge));
            }

            var loadedEdges = graphFileManager.LoadEdges(1234567890123456);
            ClassicAssert.AreEqual(edges.Count, loadedEdges.Count);

            for (int i = 0; i < edges.Count; i++)
            {
                ClassicAssert.AreEqual(edges[i].FromNode, loadedEdges[i].FromNode);
                ClassicAssert.AreEqual(edges[i].ToNode, loadedEdges[i].ToNode);
                ClassicAssert.AreEqual(edges[i].Weight, loadedEdges[i].Weight);
                ClassicAssert.AreEqual(edges[i].EdgeContent, loadedEdges[i].EdgeContent);
            }
        }

        /// <summary>
        /// Tests saving and loading an edge with special characters in its content.
        /// Verifies that edges with content containing special characters are correctly saved and loaded.
        /// Special characters tested: !@#$%^&*()
        /// Load(Save(edge_with_special_characters)) == edge_with_special_characters
        /// </summary>
        [Test]
        public void TestSaveAndLoadEdgeWithSpecialCharacters()
        {
            var edge = new Edge(1234567890123456, 6543210987654321, 1.5, "Test Edge!@#$%^&*()");
            ClassicAssert.IsTrue(graphFileManager.SaveEdge(edge));
            var loadedEdges = graphFileManager.LoadEdges(edge.FromNode);
            ClassicAssert.IsNotEmpty(loadedEdges);
            var loadedEdge = loadedEdges[0];
            ClassicAssert.AreEqual(edge.FromNode, loadedEdge.FromNode);
            ClassicAssert.AreEqual(edge.ToNode, loadedEdge.ToNode);
            ClassicAssert.AreEqual(edge.Weight, loadedEdge.Weight);
            ClassicAssert.AreEqual(edge.EdgeContent, loadedEdge.EdgeContent);
        }

        /// <summary>
        /// Tests loading an edge that does not exist.
        /// Verifies that loading a nonexistent edge returns an empty list.
        /// (Load(nonexistentEdge) == empty)
        /// </summary>
        [Test]
        public void TestLoadEdgeThatDoesNotExist()
        {
            var loadedEdges = graphFileManager.LoadEdges(9999999999999999);
            ClassicAssert.IsEmpty(loadedEdges);
        }

        /// <summary>
        /// Tests saving and loading multiple edges between the same source and destination nodes.
        /// Verifies that when multiple edges are saved between the same nodes, the latest saved edge overwrites the previous ones.
        /// (Load(Save(edge_1) -> Save(edge_2) -> Save(edge_3)) == edge_3)
        /// </summary>
        [Test]
        public void TestSaveAndLoadEdgesBetweenSameNodes()
        {
            var edge1 = new Edge(1234567890123456, 6543210987654321, 1.5, "Test Edge 1");
            var edge2 = new Edge(1234567890123456, 6543210987654321, 2.0, "Test Edge 2");
            var edge3 = new Edge(1234567890123456, 6543210987654321, 2.5, "Test Edge 3");

            // Save the first edge
            ClassicAssert.IsTrue(graphFileManager.SaveEdge(edge1));
            // Save the second edge, which should overwrite the first one due to the same source and destination
            ClassicAssert.IsTrue(graphFileManager.SaveEdge(edge2));
            // Save the third edge, which should overwrite the second one
            ClassicAssert.IsTrue(graphFileManager.SaveEdge(edge3));

            var loadedEdges = graphFileManager.LoadEdges(1234567890123456);

            // Filter to find edges from source to the specific destination node
            var filteredEdges = loadedEdges.Where(e => e.ToNode == 6543210987654321).ToList();

            // Debug output to check the filtered edges
            System.Console.WriteLine($"Filtered Edges Count: {filteredEdges.Count}");
            foreach (var edge in filteredEdges)
            {
                System.Console.WriteLine($"Edge from {edge.FromNode} to {edge.ToNode} with weight {edge.Weight} and content {edge.EdgeContent}");
            }

            // Expect only one edge between the specified source and destination nodes
            ClassicAssert.AreEqual(1, filteredEdges.Count);

            // The last saved edge should be the one that's loaded
            var loadedEdge = filteredEdges[0];
            ClassicAssert.AreEqual(edge3.FromNode, loadedEdge.FromNode);
            ClassicAssert.AreEqual(edge3.ToNode, loadedEdge.ToNode);
            ClassicAssert.AreEqual(edge3.Weight, loadedEdge.Weight);
            ClassicAssert.AreEqual(edge3.EdgeContent, loadedEdge.EdgeContent);
        }
    }
}
