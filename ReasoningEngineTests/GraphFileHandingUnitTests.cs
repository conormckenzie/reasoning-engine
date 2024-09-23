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

        private IndexFile LoadIndexFile(string indexFilePath)
        {
            string json = File.ReadAllText(indexFilePath);
            return JsonConvert.DeserializeObject<IndexFile>(json) ?? new IndexFile();
        }
    }
}