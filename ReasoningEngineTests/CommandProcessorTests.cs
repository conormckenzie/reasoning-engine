using NUnit.Framework;
using ReasoningEngine.GraphAccess;
using ReasoningEngine.GraphFileHandling;
using System;
using System.IO;

namespace ReasoningEngineTests
{
    [TestFixture]
    public class CommandProcessorTests
    {
        private CommandProcessor commandProcessor;
        private GraphFileManager graphFileManager;
        private string tempDir;

        [SetUp]
        public void Setup()
        {
            tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDir);
            graphFileManager = new GraphFileManager(tempDir);
            commandProcessor = new CommandProcessor(graphFileManager);
        }

        [TearDown]
        public void TearDown()
        {
            Directory.Delete(tempDir, true);
        }

        [Test]
        public void TestAddNode()
        {
            string result = commandProcessor.ProcessCommand("add_node", "1234567890123456|Test Node");
            Assert.That(result, Is.EqualTo("Node 1234567890123456 added successfully."));

            var loadedNode = graphFileManager.LoadNode(1234567890123456);
            Assert.That(loadedNode, Is.Not.Null);
            Assert.That((loadedNode as dynamic).Content, Is.EqualTo("Test Node"));
        }

        [Test]
        public void TestQueryNode()
        {
            commandProcessor.ProcessCommand("add_node", "1234567890123456|Test Node");
            string result = commandProcessor.ProcessCommand("node_query", "1234567890123456");
            Assert.That(result, Does.Contain("Node 1234567890123456"));
            Assert.That(result, Does.Contain("Test Node"));
        }

        [Test]
        public void TestAddEdge()
        {
            commandProcessor.ProcessCommand("add_node", "1234567890123456|Source Node");
            commandProcessor.ProcessCommand("add_node", "6543210987654321|Destination Node");
            string result = commandProcessor.ProcessCommand("add_edge", "1234567890123456|6543210987654321|1.5|Test Edge");
            Assert.That(result, Is.EqualTo("Edge from 1234567890123456 to 6543210987654321 added successfully."));

            var loadedEdges = graphFileManager.LoadEdges(1234567890123456, true);
            Assert.That(loadedEdges, Has.Count.EqualTo(1));
            Assert.That(loadedEdges[0].ToNode, Is.EqualTo(6543210987654321));
            Assert.That((loadedEdges[0] as dynamic).Weight, Is.EqualTo(1.5));
            Assert.That((loadedEdges[0] as dynamic).EdgeContent, Is.EqualTo("Test Edge"));
        }

        [Test]
        public void TestQueryEdges()
        {
            commandProcessor.ProcessCommand("add_node", "1234567890123456|Source Node");
            commandProcessor.ProcessCommand("add_node", "6543210987654321|Destination Node");
            commandProcessor.ProcessCommand("add_edge", "1234567890123456|6543210987654321|1.5|Test Edge");
            
            string outgoingResult = commandProcessor.ProcessCommand("outgoing_edge_query", "1234567890123456");
            Assert.That(outgoingResult, Does.Contain("6543210987654321"));
            Assert.That(outgoingResult, Does.Contain("1.5"));
            Assert.That(outgoingResult, Does.Contain("Test Edge"));

            string incomingResult = commandProcessor.ProcessCommand("incoming_edge_query", "6543210987654321");
            Assert.That(incomingResult, Does.Contain("1234567890123456"));
            Assert.That(incomingResult, Does.Contain("1.5"));
            Assert.That(incomingResult, Does.Contain("Test Edge"));
        }

        [Test]
        public void TestDeleteNode()
        {
            commandProcessor.ProcessCommand("add_node", "1234567890123456|Test Node");
            string result = commandProcessor.ProcessCommand("delete_node", "1234567890123456");
            Assert.That(result, Does.Contain("Node 1234567890123456 and all its associated edges have been deleted successfully"));

            var loadedNode = graphFileManager.LoadNode(1234567890123456);
            Assert.That(loadedNode, Is.Null);
        }

        [Test]
        public void TestDeleteEdge()
        {
            commandProcessor.ProcessCommand("add_node", "1234567890123456|Source Node");
            commandProcessor.ProcessCommand("add_node", "6543210987654321|Destination Node");
            commandProcessor.ProcessCommand("add_edge", "1234567890123456|6543210987654321|1.5|Test Edge");
            
            string result = commandProcessor.ProcessCommand("delete_edge", "1234567890123456|6543210987654321");
            Assert.That(result, Does.Contain("Edge from node 1234567890123456 to node 6543210987654321 has been deleted successfully"));

            var loadedEdges = graphFileManager.LoadEdges(1234567890123456, true);
            Assert.That(loadedEdges, Is.Empty);
        }

        [Test]
        public void TestEditNode()
        {
            commandProcessor.ProcessCommand("add_node", "1234567890123456|Original Content");
            string result = commandProcessor.ProcessCommand("edit_node", "1234567890123456|Updated Content");
            Assert.That(result, Is.EqualTo("Node 1234567890123456 updated successfully."));

            var loadedNode = graphFileManager.LoadNode(1234567890123456);
            Assert.That(loadedNode, Is.Not.Null);
            Assert.That((loadedNode as dynamic).Content, Is.EqualTo("Updated Content"));
        }

        [Test]
        public void TestEditEdge()
        {
            commandProcessor.ProcessCommand("add_node", "1234567890123456|Source Node");
            commandProcessor.ProcessCommand("add_node", "6543210987654321|Destination Node");
            commandProcessor.ProcessCommand("add_edge", "1234567890123456|6543210987654321|1.5|Original Edge");
            
            string result = commandProcessor.ProcessCommand("edit_edge", "1234567890123456|6543210987654321|2.0|Updated Edge");
            Assert.That(result, Is.EqualTo("Edge from 1234567890123456 to 6543210987654321 updated successfully."));

            var loadedEdges = graphFileManager.LoadEdges(1234567890123456, true);
            Assert.That(loadedEdges, Has.Count.EqualTo(1));
            Assert.That((loadedEdges[0] as dynamic).Weight, Is.EqualTo(2.0));
            Assert.That((loadedEdges[0] as dynamic).EdgeContent, Is.EqualTo("Updated Edge"));
        }
    }
}