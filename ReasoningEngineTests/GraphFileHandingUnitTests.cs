using NUnit.Framework;
using ReasoningEngine;
using ReasoningEngine.GraphFileHandling;
using System;
using System.Collections.Generic;
using System.IO;

namespace ReasoningEngineTests
{
    [TestFixture]
    public class GraphFileHandlingUnitTests
    {
        private GraphFileManager graphFileManager;
        private string tempDir;

        [SetUp]
        public void Setup()
        {
            tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDir);
            graphFileManager = new GraphFileManager(tempDir);
        }

        [TearDown]
        public void TearDown()
        {
            Directory.Delete(tempDir, true);
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
            Assert.That(graphFileManager.SaveNode(node), Is.True);
            var loadedNode = graphFileManager.LoadNode(node.Id);
            Assert.That(loadedNode, Is.Not.Null);
            Assert.That(loadedNode.Id, Is.EqualTo(node.Id));
            Assert.That((loadedNode as dynamic).Content, Is.EqualTo(node.Content));
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
                Assert.That(graphFileManager.SaveNode(node), Is.True);
            }

            foreach (var node in nodes)
            {
                var loadedNode = graphFileManager.LoadNode(node.Id);
                Assert.That(loadedNode, Is.Not.Null);
                Assert.That(loadedNode.Id, Is.EqualTo(node.Id));
                Assert.That((loadedNode as dynamic).Content, Is.EqualTo(node.Content));
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
            Assert.That(graphFileManager.SaveNode(node), Is.True);
            var loadedNode = graphFileManager.LoadNode(node.Id);
            Assert.That(loadedNode, Is.Not.Null);
            Assert.That(loadedNode.Id, Is.EqualTo(node.Id));
            Assert.That((loadedNode as dynamic).Content, Is.EqualTo(node.Content));
        }

        [Test]
        public void TestLoadNodeThatDoesNotExist()
        {
            var loadedNode = graphFileManager.LoadNode(9999999999999999);
            Assert.That(loadedNode, Is.Null);
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
            Assert.That(graphFileManager.SaveEdge(edge), Is.True);
            var loadedEdges = graphFileManager.LoadEdges(edge.FromNode);
            Assert.That(loadedEdges, Is.Not.Empty);
            var loadedEdge = loadedEdges[0];
            Assert.That(loadedEdge.FromNode, Is.EqualTo(edge.FromNode));
            Assert.That(loadedEdge.ToNode, Is.EqualTo(edge.ToNode));
            Assert.That((loadedEdge as dynamic).Weight, Is.EqualTo(edge.Weight));
            Assert.That((loadedEdge as dynamic).EdgeContent, Is.EqualTo(edge.EdgeContent));
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
                Assert.That(graphFileManager.SaveEdge(edge), Is.True);
            }

            var loadedEdges = graphFileManager.LoadEdges(1234567890123456);
            Assert.That(loadedEdges.Count, Is.EqualTo(edges.Count));

            for (int i = 0; i < edges.Count; i++)
            {
                Assert.That(loadedEdges[i].FromNode, Is.EqualTo(edges[i].FromNode));
                Assert.That(loadedEdges[i].ToNode, Is.EqualTo(edges[i].ToNode));
                Assert.That((loadedEdges[i] as dynamic).Weight, Is.EqualTo(edges[i].Weight));
                Assert.That((loadedEdges[i] as dynamic).EdgeContent, Is.EqualTo(edges[i].EdgeContent));
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
            Assert.That(graphFileManager.SaveEdge(edge), Is.True);
            var loadedEdges = graphFileManager.LoadEdges(edge.FromNode);
            Assert.That(loadedEdges, Is.Not.Empty);
            var loadedEdge = loadedEdges[0];
            Assert.That(loadedEdge.FromNode, Is.EqualTo(edge.FromNode));
            Assert.That(loadedEdge.ToNode, Is.EqualTo(edge.ToNode));
            Assert.That((loadedEdge as dynamic).Weight, Is.EqualTo(edge.Weight));
            Assert.That((loadedEdge as dynamic).EdgeContent, Is.EqualTo(edge.EdgeContent));
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
            Assert.That(loadedEdges, Is.Empty);
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

            Assert.That(graphFileManager.SaveEdge(edge1), Is.True);
            Assert.That(graphFileManager.SaveEdge(edge2), Is.True);
            Assert.That(graphFileManager.SaveEdge(edge3), Is.True);

            var loadedEdges = graphFileManager.LoadEdges(1234567890123456);
            var filteredEdges = loadedEdges.Where(e => e.ToNode == 6543210987654321).ToList();

            Assert.That(filteredEdges, Has.Count.EqualTo(1));

            var loadedEdge = filteredEdges[0];
            Assert.That(loadedEdge.FromNode, Is.EqualTo(edge3.FromNode));
            Assert.That(loadedEdge.ToNode, Is.EqualTo(edge3.ToNode));
            Assert.That((loadedEdge as dynamic).Weight, Is.EqualTo(edge3.Weight));
            Assert.That((loadedEdge as dynamic).EdgeContent, Is.EqualTo(edge3.EdgeContent));
        }
    }
}