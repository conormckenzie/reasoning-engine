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
            var outgoingEdges = graphFileManager.LoadEdges(edge.FromNode, true);
            var incomingEdges = graphFileManager.LoadEdges(edge.ToNode, false);
            Assert.That(outgoingEdges, Has.Count.EqualTo(1));
            Assert.That(incomingEdges, Has.Count.EqualTo(1));
            var loadedOutgoingEdge = outgoingEdges[0];
            var loadedIncomingEdge = incomingEdges[0];
            Assert.That(loadedOutgoingEdge.FromNode, Is.EqualTo(edge.FromNode));
            Assert.That(loadedOutgoingEdge.ToNode, Is.EqualTo(edge.ToNode));
            Assert.That((loadedOutgoingEdge as dynamic).Weight, Is.EqualTo(edge.Weight));
            Assert.That((loadedOutgoingEdge as dynamic).EdgeContent, Is.EqualTo(edge.EdgeContent));
            Assert.That(loadedIncomingEdge.FromNode, Is.EqualTo(edge.FromNode));
            Assert.That(loadedIncomingEdge.ToNode, Is.EqualTo(edge.ToNode));
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

            var loadedOutgoingEdges = graphFileManager.LoadEdges(1234567890123456, true);
            Assert.That(loadedOutgoingEdges, Has.Count.EqualTo(edges.Count));

            for (int i = 0; i < edges.Count; i++)
            {
                Assert.That(loadedOutgoingEdges[i].FromNode, Is.EqualTo(edges[i].FromNode));
                Assert.That(loadedOutgoingEdges[i].ToNode, Is.EqualTo(edges[i].ToNode));
                Assert.That((loadedOutgoingEdges[i] as dynamic).Weight, Is.EqualTo(edges[i].Weight));
                Assert.That((loadedOutgoingEdges[i] as dynamic).EdgeContent, Is.EqualTo(edges[i].EdgeContent));
            }

            // Check incoming edges for each destination node
            foreach (var edge in edges)
            {
                var loadedIncomingEdges = graphFileManager.LoadEdges(edge.ToNode, false);
                Assert.That(loadedIncomingEdges, Has.Count.GreaterThan(0));
                var matchingEdge = loadedIncomingEdges.Find(e => e.FromNode == edge.FromNode && e.ToNode == edge.ToNode);
                Assert.That(matchingEdge, Is.Not.Null);
                Assert.That((matchingEdge as dynamic).Weight, Is.EqualTo(edge.Weight));
                Assert.That((matchingEdge as dynamic).EdgeContent, Is.EqualTo(edge.EdgeContent));
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
            var loadedEdges = graphFileManager.LoadEdges(edge.FromNode, true);
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
            var loadedEdges = graphFileManager.LoadEdges(9999999999999999, true);
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

            var loadedOutgoingEdges = graphFileManager.LoadEdges(1234567890123456, true);
            var loadedIncomingEdges = graphFileManager.LoadEdges(6543210987654321, false);

            Assert.That(loadedOutgoingEdges, Has.Count.EqualTo(1), "There should be only one outgoing edge");
            Assert.That(loadedIncomingEdges, Has.Count.EqualTo(1), "There should be only one incoming edge");

            var loadedOutgoingEdge = loadedOutgoingEdges[0];
            var loadedIncomingEdge = loadedIncomingEdges[0];

            // The last saved edge (edge3) should be the one that persists
            Assert.That(loadedOutgoingEdge.FromNode, Is.EqualTo(edge3.FromNode));
            Assert.That(loadedOutgoingEdge.ToNode, Is.EqualTo(edge3.ToNode));
            Assert.That((loadedOutgoingEdge as dynamic).Weight, Is.EqualTo(edge3.Weight));
            Assert.That((loadedOutgoingEdge as dynamic).EdgeContent, Is.EqualTo(edge3.EdgeContent));

            Assert.That(loadedIncomingEdge.FromNode, Is.EqualTo(edge3.FromNode));
            Assert.That(loadedIncomingEdge.ToNode, Is.EqualTo(edge3.ToNode));
            Assert.That((loadedIncomingEdge as dynamic).Weight, Is.EqualTo(edge3.Weight));
            Assert.That((loadedIncomingEdge as dynamic).EdgeContent, Is.EqualTo(edge3.EdgeContent));
        }

        [Test]
        public void TestDeleteNode()
        {
            var node = new Node(1234567890123456, "Test Node");
            Assert.That(graphFileManager.SaveNode(node), Is.True);
            Assert.That(graphFileManager.DeleteNode(node.Id), Is.True);
            var loadedNode = graphFileManager.LoadNode(node.Id);
            Assert.That(loadedNode, Is.Null);
        }

        [Test]
        public void TestDeleteEdge()
        {
            var edge = new Edge(1234567890123456, 6543210987654321, 1.5, "Test Edge");
            Assert.That(graphFileManager.SaveEdge(edge), Is.True);
            Assert.That(graphFileManager.DeleteEdge(edge.FromNode, edge.ToNode), Is.True);
            var loadedOutgoingEdges = graphFileManager.LoadEdges(edge.FromNode, true);
            var loadedIncomingEdges = graphFileManager.LoadEdges(edge.ToNode, false);
            Assert.That(loadedOutgoingEdges, Is.Empty);
            Assert.That(loadedIncomingEdges, Is.Empty);
        }

        [Test]
        public void TestDeleteNodeWithEdges()
        {
            Console.WriteLine("Starting TestDeleteNodeWithEdges");

            var node1 = new Node(1234567890123456, "Test Node 1");
            var node2 = new Node(6543210987654321, "Test Node 2");
            var edge = new Edge(node1.Id, node2.Id, 1.5, "Test Edge");
            
            Console.WriteLine($"Created nodes: {node1.Id}, {node2.Id}");
            Console.WriteLine($"Created edge: {edge.FromNode} -> {edge.ToNode}");

            Assert.That(graphFileManager.SaveNode(node1), Is.True, "Failed to save node1");
            Assert.That(graphFileManager.SaveNode(node2), Is.True, "Failed to save node2");
            Assert.That(graphFileManager.SaveEdge(edge), Is.True, "Failed to save edge");

            Console.WriteLine("Saved nodes and edge");

            // Verify edge was saved correctly
            var savedOutgoingEdges = graphFileManager.LoadEdges(node1.Id, true);
            var savedIncomingEdges = graphFileManager.LoadEdges(node2.Id, false);
            Console.WriteLine($"Outgoing edges for node1: {savedOutgoingEdges.Count}");
            Console.WriteLine($"Incoming edges for node2: {savedIncomingEdges.Count}");

            Assert.That(graphFileManager.DeleteNode(node1.Id), Is.True, "Failed to delete node1");
            Console.WriteLine($"Deleted node: {node1.Id}");

            var loadedNode1 = graphFileManager.LoadNode(node1.Id);
            var loadedNode2 = graphFileManager.LoadNode(node2.Id);
            var loadedOutgoingEdges = graphFileManager.LoadEdges(node1.Id, true);
            var loadedIncomingEdges = graphFileManager.LoadEdges(node2.Id, false);

            Console.WriteLine($"After deletion - Node1 exists: {loadedNode1 != null}");
            Console.WriteLine($"After deletion - Node2 exists: {loadedNode2 != null}");
            Console.WriteLine($"After deletion - Outgoing edges for node1: {loadedOutgoingEdges.Count}");
            Console.WriteLine($"After deletion - Incoming edges for node2: {loadedIncomingEdges.Count}");

            Assert.That(loadedNode1, Is.Null, "Node1 should be null after deletion");
            Assert.That(loadedNode2, Is.Not.Null, "Node2 should still exist");
            Assert.That(loadedOutgoingEdges, Is.Empty, "Outgoing edges should be empty");
            Assert.That(loadedIncomingEdges, Is.Empty, "Incoming edges should be empty");

            if (loadedIncomingEdges.Any())
            {
                Console.WriteLine("Remaining incoming edges:");
                foreach (var remainingEdge in loadedIncomingEdges)
                {
                    Console.WriteLine($"  Edge: {remainingEdge.FromNode} -> {remainingEdge.ToNode}");
                    Console.WriteLine($"  Weight: {(remainingEdge as dynamic).Weight}");
                    Console.WriteLine($"  Content: {(remainingEdge as dynamic).EdgeContent}");
                    
                    // Check if the file for this edge still exists
                    string incomingEdgeFilePath = graphFileManager.GetEdgeFilePath(remainingEdge.FromNode, remainingEdge.ToNode, false);
                    Console.WriteLine($"  Edge file exists: {File.Exists(incomingEdgeFilePath)}");
                    Console.WriteLine($"  Edge file path: {incomingEdgeFilePath}");
                }
            }

            Console.WriteLine("TestDeleteNodeWithEdges completed");
        }
    }
}