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

        [Test]
        public void TestSaveAndLoadNode()
        {
            var node = new Node(1234567890123456, "Test Node");
            bool saveSuccess = graphFileManager.SaveNode(node.Id, node.Content);
            ClassicAssert.IsTrue(saveSuccess, "Node save should succeed");

            var loadedNode = graphFileManager.LoadNode(node.Id);
            ClassicAssert.IsNotNull(loadedNode, "Loaded node should not be null");
            ClassicAssert.AreEqual(node.Id, loadedNode.Id, "Node IDs should match");
            ClassicAssert.AreEqual(node.Content, loadedNode.Content, "Node contents should match");
        }

        [Test]
        public void TestSaveAndLoadEdge()
        {
            var edge = new Edge(1234567890123456, 6543210987654321, 1.5, "Test Edge");
            bool saveSuccess = graphFileManager.SaveEdge(edge);
            ClassicAssert.IsTrue(saveSuccess, "Edge save should succeed");

            var loadedEdges = graphFileManager.LoadEdges(edge.FromNode);
            ClassicAssert.IsNotEmpty(loadedEdges, "Loaded edges should not be empty");
            var loadedEdge = loadedEdges[0];  // Assuming the first edge is the one we saved
            ClassicAssert.AreEqual(edge.FromNode, loadedEdge.FromNode, "FromNode IDs should match");
            ClassicAssert.AreEqual(edge.ToNode, loadedEdge.ToNode, "ToNode IDs should match");
            ClassicAssert.AreEqual(edge.Weight, loadedEdge.Weight, "Edge weights should match");
            ClassicAssert.AreEqual(edge.EdgeContent, loadedEdge.EdgeContent, "Edge contents should match");
        }
    }
}
