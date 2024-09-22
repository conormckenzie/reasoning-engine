// File: GraphFileHandlingUnitTests.cs

using NUnit.Framework;
using ReasoningEngine;
using ReasoningEngine.GraphFileHandling;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
            Assert.That((loadedIncomingEdge as dynamic).Weight, Is.EqualTo(edge.Weight));
            Assert.That((loadedIncomingEdge as dynamic).EdgeContent, Is.EqualTo(edge.EdgeContent));

            // Verify index files exist and contain correct entries
            string outgoingEdgeFilePath = graphFileManager.GetEdgeFilePath(edge.FromNode, edge.ToNode, true);
            string outgoingIndexFilePath = Path.Combine(Path.GetDirectoryName(outgoingEdgeFilePath), "index.json");
            Assert.IsTrue(File.Exists(outgoingIndexFilePath));

            string incomingEdgeFilePath = graphFileManager.GetEdgeFilePath(edge.ToNode, edge.FromNode, false);
            string incomingIndexFilePath = Path.Combine(Path.GetDirectoryName(incomingEdgeFilePath), "index.json");
            Assert.IsTrue(File.Exists(incomingIndexFilePath));
        }

        [Test]
        public void TestDeleteEdgeUpdatesIndex()
        {
            var edge = new Edge(1234567890123456, 6543210987654321, 1.5, "Test Edge");
            graphFileManager.SaveEdge(edge);

            string outgoingEdgeFilePath = graphFileManager.GetEdgeFilePath(edge.FromNode, edge.ToNode, true);
            string outgoingIndexFilePath = Path.Combine(Path.GetDirectoryName(outgoingEdgeFilePath), "index.json");

            string incomingEdgeFilePath = graphFileManager.GetEdgeFilePath(edge.ToNode, edge.FromNode, false);
            string incomingIndexFilePath = Path.Combine(Path.GetDirectoryName(incomingEdgeFilePath), "index.json");

            // Ensure edge and index files exist
            Assert.IsTrue(File.Exists(outgoingEdgeFilePath));
            Assert.IsTrue(File.Exists(outgoingIndexFilePath));
            Assert.IsTrue(File.Exists(incomingEdgeFilePath));
            Assert.IsTrue(File.Exists(incomingIndexFilePath));

            // Delete the edge
            Assert.IsTrue(graphFileManager.DeleteEdge(edge.FromNode, edge.ToNode));

            // Edge files should be deleted
            Assert.IsFalse(File.Exists(outgoingEdgeFilePath));
            Assert.IsFalse(File.Exists(incomingEdgeFilePath));

            // Index files should be updated
            IndexFile outgoingIndex = LoadIndexFile(outgoingIndexFilePath);
            IndexFile incomingIndex = LoadIndexFile(incomingIndexFilePath);

            Assert.IsFalse(outgoingIndex.EdgeFiles.Contains(Path.GetFileName(outgoingEdgeFilePath)));
            Assert.IsFalse(incomingIndex.EdgeFiles.Contains(Path.GetFileName(incomingEdgeFilePath)));
        }

        [Test]
        public void TestLoadEdgesUsingIndexFiles()
        {
            var edge1 = new Edge(1234567890123456, 6543210987654321, 1.0, "Edge 1");
            var edge2 = new Edge(1234567890123456, 7654321098765432, 2.0, "Edge 2");
            var edge3 = new Edge(1234567890123456, 8765432109876543, 3.0, "Edge 3");

            graphFileManager.SaveEdge(edge1);
            graphFileManager.SaveEdge(edge2);
            graphFileManager.SaveEdge(edge3);

            var outgoingEdges = graphFileManager.LoadEdges(1234567890123456, true);
            Assert.AreEqual(3, outgoingEdges.Count);

            var incomingEdges1 = graphFileManager.LoadEdges(6543210987654321, false);
            var incomingEdges2 = graphFileManager.LoadEdges(7654321098765432, false);
            var incomingEdges3 = graphFileManager.LoadEdges(8765432109876543, false);

            Assert.AreEqual(1, incomingEdges1.Count);
            Assert.AreEqual(1, incomingEdges2.Count);
            Assert.AreEqual(1, incomingEdges3.Count);

            // Verify that index files exist and are correct
            foreach (var edge in outgoingEdges)
            {
                string edgeFilePath = graphFileManager.GetEdgeFilePath(edge.FromNode, edge.ToNode, true);
                string indexFilePath = Path.Combine(Path.GetDirectoryName(edgeFilePath), "index.json");
                IndexFile indexFile = LoadIndexFile(indexFilePath);
                Assert.IsTrue(indexFile.EdgeFiles.Contains(Path.GetFileName(edgeFilePath)));
            }
        }

        private IndexFile LoadIndexFile(string indexFilePath)
        {
            string json = File.ReadAllText(indexFilePath);
            return JsonConvert.DeserializeObject<IndexFile>(json);
        }
    }
}
