using NUnit.Framework;
using ReasoningEngine;
using ReasoningEngine.GraphFileHandling;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

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

        [Test]
        public void TestSaveAndLoadSingleEdge()
        {
            var edge = new Edge(1234567890123456, 6543210987654321, 1.5, "Test Edge");

            // Create nodes first
            graphFileManager.SaveNode(new Node(edge.FromNode, "From Node"));
            graphFileManager.SaveNode(new Node(edge.ToNode, "To Node"));

            Assert.That(graphFileManager.SaveEdge(edge), Is.True);

            var outgoingEdges = graphFileManager.LoadEdges(edge.FromNode, true);
            var incomingEdges = graphFileManager.LoadEdges(edge.ToNode, false);
            Assert.That(outgoingEdges.Count, Is.EqualTo(1));
            Assert.That(incomingEdges.Count, Is.EqualTo(1));

            var loadedOutgoingEdge = outgoingEdges[0];
            var loadedIncomingEdge = incomingEdges[0];

            Assert.That(loadedOutgoingEdge.FromNode, Is.EqualTo(edge.FromNode));
            Assert.That(loadedOutgoingEdge.ToNode, Is.EqualTo(edge.ToNode));
            Assert.That((loadedOutgoingEdge as dynamic).Weight, Is.EqualTo(edge.Weight));
            Assert.That((loadedOutgoingEdge as dynamic).EdgeContent, Is.EqualTo(edge.EdgeContent));

            Assert.That(loadedIncomingEdge.FromNode, Is.EqualTo(edge.FromNode));
            Assert.That(loadedIncomingEdge.ToNode, Is.EqualTo(edge.ToNode));
            Assert.That((loadedIncomingEdge as dynamic).Weight, Is.EqualTo(edge.Weight));
            Assert.That((loadedIncomingEdge as dynamic).EdgeContent, Is.EqualTo(edge.EdgeContent));

            // Verify index files exist and contain correct entries
            string outgoingEdgeFilePath = graphFileManager.GetEdgeFilePath(edge.FromNode, edge.ToNode, true);
            string? outgoingEdgeDir = Path.GetDirectoryName(outgoingEdgeFilePath);
            string outgoingIndexFilePath = Path.Combine(outgoingEdgeDir ?? "", "index.json");
            Assert.That(File.Exists(outgoingIndexFilePath), Is.True);

            string incomingEdgeFilePath = graphFileManager.GetEdgeFilePath(edge.ToNode, edge.FromNode, false);
            string? incomingEdgeDir = Path.GetDirectoryName(incomingEdgeFilePath);
            string incomingIndexFilePath = Path.Combine(incomingEdgeDir ?? "", "index.json");
            Assert.That(File.Exists(incomingIndexFilePath), Is.True);
        }

        [Test]
        public void TestDeleteEdgeUpdatesIndex()
        {
            var edge = new Edge(1234567890123456, 6543210987654321, 1.5, "Test Edge");
    
            // Create nodes first
            graphFileManager.SaveNode(new Node(edge.FromNode, "From Node"));
            graphFileManager.SaveNode(new Node(edge.ToNode, "To Node"));
    
            Assert.That(graphFileManager.SaveEdge(edge), Is.True);

            string outgoingEdgeFilePath = graphFileManager.GetEdgeFilePath(edge.FromNode, edge.ToNode, true);
            string? outgoingEdgeDir = Path.GetDirectoryName(outgoingEdgeFilePath);
            string outgoingIndexFilePath = Path.Combine(outgoingEdgeDir ?? "", "index.json");

            string incomingEdgeFilePath = graphFileManager.GetEdgeFilePath(edge.ToNode, edge.FromNode, false);
            string? incomingEdgeDir = Path.GetDirectoryName(incomingEdgeFilePath);
            string incomingIndexFilePath = Path.Combine(incomingEdgeDir ?? "", "index.json");

            // Ensure edge and index files exist
            Assert.That(File.Exists(outgoingEdgeFilePath), Is.True);
            Assert.That(File.Exists(outgoingIndexFilePath), Is.True);
            Assert.That(File.Exists(incomingEdgeFilePath), Is.True);
            Assert.That(File.Exists(incomingIndexFilePath), Is.True);

            // Delete the edge
            Assert.That(graphFileManager.DeleteEdge(edge.FromNode, edge.ToNode), Is.True);

            // Edge files should be deleted
            Assert.That(File.Exists(outgoingEdgeFilePath), Is.False);
            Assert.That(File.Exists(incomingEdgeFilePath), Is.False);

            // Index files should be updated
            IndexFile outgoingIndex = LoadIndexFile(outgoingIndexFilePath);
            IndexFile incomingIndex = LoadIndexFile(incomingIndexFilePath);

            Assert.That(outgoingIndex.EdgeFiles.Contains(Path.GetFileName(outgoingEdgeFilePath)), Is.False);
            Assert.That(incomingIndex.EdgeFiles.Contains(Path.GetFileName(incomingEdgeFilePath)), Is.False);
        }

        [Test]
        public void TestLoadEdgesUsingIndexFiles()
        {
            var edge1 = new Edge(1234567890123456, 6543210987654321, 1.0, "Edge 1");
            var edge2 = new Edge(1234567890123456, 7654321098765432, 2.0, "Edge 2");
            var edge3 = new Edge(1234567890123456, 8765432109876543, 3.0, "Edge 3");

            // Create nodes first
            graphFileManager.SaveNode(new Node(1234567890123456, "Source Node"));
            graphFileManager.SaveNode(new Node(6543210987654321, "Dest Node 1"));
            graphFileManager.SaveNode(new Node(7654321098765432, "Dest Node 2"));
            graphFileManager.SaveNode(new Node(8765432109876543, "Dest Node 3"));

            graphFileManager.SaveEdge(edge1);
            graphFileManager.SaveEdge(edge2);
            graphFileManager.SaveEdge(edge3);

            var outgoingEdges = graphFileManager.LoadEdges(1234567890123456, true);
            Assert.That(outgoingEdges.Count, Is.EqualTo(3));

            var incomingEdges1 = graphFileManager.LoadEdges(6543210987654321, false);
            var incomingEdges2 = graphFileManager.LoadEdges(7654321098765432, false);
            var incomingEdges3 = graphFileManager.LoadEdges(8765432109876543, false);

            Assert.That(incomingEdges1.Count, Is.EqualTo(1));
            Assert.That(incomingEdges2.Count, Is.EqualTo(1));
            Assert.That(incomingEdges3.Count, Is.EqualTo(1));

            // Verify that index files exist and are correct
            foreach (var edge in outgoingEdges)
            {
                string edgeFilePath = graphFileManager.GetEdgeFilePath(edge.FromNode, edge.ToNode, true);
                string? edgeDir = Path.GetDirectoryName(edgeFilePath);
                string indexFilePath = Path.Combine(edgeDir ?? "", "index.json");
                IndexFile indexFile = LoadIndexFile(indexFilePath);
                Assert.That(indexFile.EdgeFiles.Contains(Path.GetFileName(edgeFilePath)), Is.True);
            }
        }

        [Test]
        public void TestSaveAndLoadNodeWithLargeId()
        {
            long largeId = 9223372036854775807; // Max long value
            var node = new Node(largeId, "Large ID Node");
            Assert.That(graphFileManager.SaveNode(node), Is.True);

            var loadedNode = graphFileManager.LoadNode(largeId);
            Assert.That(loadedNode, Is.Not.Null);
            Assert.That(loadedNode.Id, Is.EqualTo(largeId));
            Assert.That((loadedNode as dynamic).Content, Is.EqualTo("Large ID Node"));
        }

        [Test]
        public void TestSaveAndLoadMultipleNodes()
        {
            var nodes = new List<Node>
            {
                new Node(1, "Node One"),
                new Node(2, "Node Two"),
                new Node(3, "Node Three")
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
                Assert.That((loadedNode as dynamic).Content, Is.EqualTo((node as dynamic).Content));
            }
        }

        [Test]
        public void TestDeleteNodeWithEdges()
        {
            var node1 = new Node(1, "Node One");
            var node2 = new Node(2, "Node Two");
            var edge = new Edge(1, 2, 1.0, "Test Edge");

            graphFileManager.SaveNode(node1);
            graphFileManager.SaveNode(node2);
            graphFileManager.SaveEdge(edge);

            Assert.That(graphFileManager.DeleteNode(1), Is.True);

            Assert.That(graphFileManager.LoadNode(1), Is.Null);
            Assert.That(graphFileManager.LoadEdges(1, true), Is.Empty);
            Assert.That(graphFileManager.LoadEdges(2, false), Is.Empty);
        }

        [Test]
        public void TestSaveAndLoadEdgeWithLargeNodeIds()
        {
            long largeId1 = 9223372036854775806;
            long largeId2 = 9223372036854775807;

            var node1 = new Node(largeId1, "Large Node One");
            var node2 = new Node(largeId2, "Large Node Two");
            var edge = new Edge(largeId1, largeId2, 1.0, "Large ID Edge");

            graphFileManager.SaveNode(node1);
            graphFileManager.SaveNode(node2);
            Assert.That(graphFileManager.SaveEdge(edge), Is.True);

            var loadedEdges = graphFileManager.LoadEdges(largeId1, true);
            Assert.That(loadedEdges, Has.Count.EqualTo(1));
            Assert.That(loadedEdges[0].FromNode, Is.EqualTo(largeId1));
            Assert.That(loadedEdges[0].ToNode, Is.EqualTo(largeId2));
        }

        [Test]
        public void TestEdgeConsistencyAfterNodeDeletion()
        {
            var node1 = new Node(1, "Node One");
            var node2 = new Node(2, "Node Two");
            var edge = new Edge(1, 2, 1.0, "Test Edge");

            graphFileManager.SaveNode(node1);
            graphFileManager.SaveNode(node2);
            graphFileManager.SaveEdge(edge);

            graphFileManager.DeleteNode(1);

            var edgesFromNode2 = graphFileManager.LoadEdges(2, false);
            Assert.That(edgesFromNode2, Is.Empty, "Incoming edges to Node 2 should be empty after deleting Node 1");
        }

        [Test]
        public void TestGetAllNodeIds()
        {
            var nodes = new List<Node>
            {
                new Node(1, "Node One"),
                new Node(2, "Node Two"),
                new Node(3, "Node Three")
            };

            foreach (var node in nodes)
            {
                graphFileManager.SaveNode(node);
            }

            var allNodeIds = graphFileManager.GetAllNodeIds();
            Assert.That(allNodeIds, Is.EquivalentTo(new List<long> { 1, 2, 3 }));

            graphFileManager.DeleteNode(2);

            allNodeIds = graphFileManager.GetAllNodeIds();
            Assert.That(allNodeIds, Is.EquivalentTo(new List<long> { 1, 3 }));
        }

        [Test]
        public void TestSaveEdgeWithNonExistentNodes()
        {
            var edge = new Edge(1, 2, 1.0, "Test Edge");
            Assert.That(graphFileManager.SaveEdge(edge), Is.False, "Saving an edge with non-existent nodes should fail");
        }

        [Test]
        public void TestUpdateEdge()
        {
            var node1 = new Node(1, "Node One");
            var node2 = new Node(2, "Node Two");
            var edge = new Edge(1, 2, 1.0, "Original Edge");

            graphFileManager.SaveNode(node1);
            graphFileManager.SaveNode(node2);
            graphFileManager.SaveEdge(edge);

            var updatedEdge = new Edge(1, 2, 2.0, "Updated Edge");
            Assert.That(graphFileManager.SaveEdge(updatedEdge), Is.True);

            var loadedEdges = graphFileManager.LoadEdges(1, true);
            Assert.That(loadedEdges, Has.Count.EqualTo(1));
            Assert.That((loadedEdges[0] as dynamic).Weight, Is.EqualTo(2.0));
            Assert.That((loadedEdges[0] as dynamic).EdgeContent, Is.EqualTo("Updated Edge"));
        }

        private IndexFile LoadIndexFile(string indexFilePath)
        {
            string json = File.ReadAllText(indexFilePath);
            return JsonConvert.DeserializeObject<IndexFile>(json) ?? new IndexFile();
        }
    }
}