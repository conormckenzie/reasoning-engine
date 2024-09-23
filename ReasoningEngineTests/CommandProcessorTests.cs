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
            // Add nodes
            var addSourceNodeResult = commandProcessor.ProcessCommand("add_node", "1234567890123456|Source Node");
            Assert.That(addSourceNodeResult, Is.EqualTo("Node 1234567890123456 added successfully."), "Failed to add source node");

            var addDestNodeResult = commandProcessor.ProcessCommand("add_node", "6543210987654321|Destination Node");
            Assert.That(addDestNodeResult, Is.EqualTo("Node 6543210987654321 added successfully."), "Failed to add destination node");

            // Add edge
            string addEdgeResult = commandProcessor.ProcessCommand("add_edge", "1234567890123456|6543210987654321|1.5|Test Edge");
            Assert.That(addEdgeResult, Is.EqualTo("Edge from 1234567890123456 to 6543210987654321 added successfully."), "Failed to add edge");

            // Load and check outgoing edges
            var loadedOutgoingEdges = graphFileManager.LoadEdges(1234567890123456, true);
            Console.WriteLine($"Loaded outgoing edges count: {loadedOutgoingEdges.Count}");
            Assert.That(loadedOutgoingEdges, Has.Count.EqualTo(1), "Failed to load outgoing edge");

            if (loadedOutgoingEdges.Count > 0)
            {
                Assert.That(loadedOutgoingEdges[0].ToNode, Is.EqualTo(6543210987654321), "Incorrect destination node for outgoing edge");
                Assert.That((loadedOutgoingEdges[0] as dynamic).Weight, Is.EqualTo(1.5), "Incorrect weight for outgoing edge");
                Assert.That((loadedOutgoingEdges[0] as dynamic).EdgeContent, Is.EqualTo("Test Edge"), "Incorrect content for outgoing edge");
            }
            else
            {
                Console.WriteLine("No outgoing edges found. Checking file system...");
                string outgoingEdgeDir = graphFileManager.GetEdgeDirPath(1234567890123456, true);
                Console.WriteLine($"Outgoing edge directory: {outgoingEdgeDir}");
                if (Directory.Exists(outgoingEdgeDir))
                {
                    Console.WriteLine("Directory exists. Listing contents:");
                    foreach (var file in Directory.GetFiles(outgoingEdgeDir, "*", SearchOption.AllDirectories))
                    {
                        Console.WriteLine(file);
                    }
                }
                else
                {
                    Console.WriteLine("Outgoing edge directory does not exist.");
                }
            }

            // Load and check incoming edges
            var loadedIncomingEdges = graphFileManager.LoadEdges(6543210987654321, false);
            Console.WriteLine($"Loaded incoming edges count: {loadedIncomingEdges.Count}");
            Assert.That(loadedIncomingEdges, Has.Count.EqualTo(1), "Failed to load incoming edge");

            if (loadedIncomingEdges.Count > 0)
            {
                Assert.That(loadedIncomingEdges[0].FromNode, Is.EqualTo(1234567890123456), "Incorrect source node for incoming edge");
                Assert.That((loadedIncomingEdges[0] as dynamic).Weight, Is.EqualTo(1.5), "Incorrect weight for incoming edge");
                Assert.That((loadedIncomingEdges[0] as dynamic).EdgeContent, Is.EqualTo("Test Edge"), "Incorrect content for incoming edge");
            }
            else
            {
                Console.WriteLine("No incoming edges found. Checking file system...");
                string incomingEdgeDir = graphFileManager.GetEdgeDirPath(6543210987654321, false);
                Console.WriteLine($"Incoming edge directory: {incomingEdgeDir}");
                if (Directory.Exists(incomingEdgeDir))
                {
                    Console.WriteLine("Directory exists. Listing contents:");
                    foreach (var file in Directory.GetFiles(incomingEdgeDir, "*", SearchOption.AllDirectories))
                    {
                        Console.WriteLine(file);
                    }
                }
                else
                {
                    Console.WriteLine("Incoming edge directory does not exist.");
                }
            }
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

        [Test]
        public void TestAddEdgeWithLargeNodeIds()
        {
            long largeId1 = 9223372036854775807; // Max long value
            long largeId2 = 9223372036854775806;
            
            commandProcessor.ProcessCommand("add_node", $"{largeId1}|Large Node 1");
            commandProcessor.ProcessCommand("add_node", $"{largeId2}|Large Node 2");
            string result = commandProcessor.ProcessCommand("add_edge", $"{largeId1}|{largeId2}|1.5|Large Edge");
            
            Assert.That(result, Is.EqualTo($"Edge from {largeId1} to {largeId2} added successfully."));
            
            var loadedEdges = graphFileManager.LoadEdges(largeId1, true);
            Assert.That(loadedEdges, Has.Count.EqualTo(1));
        }

        [Test]
        public void TestAddEdgeToNonExistentNode()
        {
            commandProcessor.ProcessCommand("add_node", "1|Existing Node");
            string result = commandProcessor.ProcessCommand("add_edge", "1|2|1.0|Invalid Edge");
            
            Assert.That(result, Does.Contain("Failed to add edge"));
            Assert.That(result, Does.Not.Contain("added successfully"));

            var loadedEdges = graphFileManager.LoadEdges(1, true);
            Assert.That(loadedEdges, Is.Empty);
        }

        [Test]
        public void TestDeleteNodeWithMultipleEdges()
        {
            commandProcessor.ProcessCommand("add_node", "1|Central Node");
            commandProcessor.ProcessCommand("add_node", "2|Node 2");
            commandProcessor.ProcessCommand("add_node", "3|Node 3");
            
            commandProcessor.ProcessCommand("add_edge", "1|2|1.0|Edge 1-2");
            commandProcessor.ProcessCommand("add_edge", "1|3|1.0|Edge 1-3");
            commandProcessor.ProcessCommand("add_edge", "2|1|1.0|Edge 2-1");
            
            string result = commandProcessor.ProcessCommand("delete_node", "1");
            Assert.That(result, Does.Contain("deleted successfully"));
            
            var loadedEdgesFrom2 = graphFileManager.LoadEdges(2, true);
            Assert.That(loadedEdgesFrom2, Is.Empty);
        }

        [Test]
        public void TestEdgeConsistency()
        {
            commandProcessor.ProcessCommand("add_node", "1|Node 1");
            commandProcessor.ProcessCommand("add_node", "2|Node 2");
            commandProcessor.ProcessCommand("add_edge", "1|2|1.0|Test Edge");
            
            var outgoingEdges = graphFileManager.LoadEdges(1, true);
            var incomingEdges = graphFileManager.LoadEdges(2, false);
            
            Assert.That(outgoingEdges, Has.Count.EqualTo(1));
            Assert.That(incomingEdges, Has.Count.EqualTo(1));
            Assert.That(outgoingEdges[0].ToNode, Is.EqualTo(2));
            Assert.That(incomingEdges[0].FromNode, Is.EqualTo(1));
        }
    }
}