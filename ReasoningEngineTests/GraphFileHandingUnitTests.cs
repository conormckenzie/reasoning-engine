using NUnit.Framework;
using ReasoningEngine;
using ReasoningEngine.GraphFileHandling;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json; // Changed from Newtonsoft.Json

namespace ReasoningEngineTests
{
    [TestFixture]
    public class GraphFileHandlingUnitTests
    {
        // Justification for null-forgiving operator (!):
        // These fields are initialized in the [SetUp] method, which NUnit guarantees
        // runs before each test execution. Therefore, they will not be null when accessed in tests.
        private FileGraphStorageProvider storageProvider = null!;
        private GraphObjectMapper graphObjectMapper = null!; // Added ObjectMapper
        private string tempDir = null!;

        [SetUp]
        public void Setup()
        {
            tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDir);
            // Instantiate the provider and mapper
            storageProvider = new FileGraphStorageProvider(tempDir); 
            graphObjectMapper = new GraphObjectMapper(storageProvider); // Initialize ObjectMapper
        }

        [TearDown]
        public void TearDown()
        {
            // No null check needed for tempDir as it's initialized in SetUp
            if (Directory.Exists(tempDir)) 
            {
                Directory.Delete(tempDir, true);
            }
        }

        [Test]
        public void TestSaveAndLoadSingleEdge()
        {
            var edge = new Edge(1234567890123456, 6543210987654321, 1.5, "Test Edge");
            var fromNode = new Node(edge.FromNode, "From Node", DomainType.Truth);
            var toNode = new Node(edge.ToNode, "To Node", DomainType.Truth);

            // Create nodes first using the storage provider
            var options = new JsonSerializerOptions { WriteIndented = true }; // Options for System.Text.Json
            string fromNodeData = JsonSerializer.Serialize(fromNode, options);
            string toNodeData = JsonSerializer.Serialize(toNode, options);
            Assert.Multiple(() =>
            {
                Assert.That(storageProvider.SaveNodeDataAsync(fromNode.Id, fromNodeData).Result, Is.True, "Failed to save FromNode"); 
                Assert.That(storageProvider.SaveNodeDataAsync(toNode.Id, toNodeData).Result, Is.True, "Failed to save ToNode"); 
            });

            // Save edge using the storage provider
            string edgeData = JsonSerializer.Serialize(edge, options);
            Assert.That(storageProvider.SaveEdgeDataAsync(edge.EdgeId, edge.FromNode, edge.ToNode, edgeData).Result, Is.True, "Failed to save edge");

            // Load edge data using the provider (by From/To IDs)
            string? loadedEdgeData = storageProvider.GetEdgeDataAsync(edge.FromNode, edge.ToNode).Result; 
            Assert.That(loadedEdgeData, Is.Not.Null.And.Not.Empty, "Failed to load edge data");

            // Deserialize and verify - Target EdgeV2 directly to use the correct constructor
            EdgeV2? loadedEdge = JsonSerializer.Deserialize<EdgeV2>(loadedEdgeData!); 
            if (loadedEdge != null)
            {
                Assert.Multiple(() => // Already wrapped
                {
                    Assert.That(loadedEdge.FromNode, Is.EqualTo(edge.FromNode));
                    Assert.That(loadedEdge.ToNode, Is.EqualTo(edge.ToNode));
                    Assert.That(loadedEdge.Weight, Is.EqualTo(edge.Weight));
                    Assert.That(loadedEdge.EdgeContent, Is.EqualTo(edge.EdgeContent));
                    Assert.That(loadedEdge.EdgeId, Is.EqualTo(edge.EdgeId)); // Verify Guid is preserved
                });
            }
            else
            {
                Assert.Fail("Failed to deserialize loaded edge data");
            }

            // Verify index files exist (using FilePathHelper to get paths)
            string outgoingEdgeFilePath = FilePathHelper.GetEdgeFilePath(tempDir, edge.FromNode, edge.ToNode, true); 
            string? outgoingEdgeDir = Path.GetDirectoryName(outgoingEdgeFilePath);
            string outgoingIndexFilePath = Path.Combine(outgoingEdgeDir ?? "", "index.json");
            Assert.Multiple(() =>
            {
                Assert.That(File.Exists(outgoingIndexFilePath), Is.True, "Outgoing index file missing");

                string incomingEdgeFilePath = FilePathHelper.GetEdgeFilePath(tempDir, edge.ToNode, edge.FromNode, false); // Note: GetEdgeFilePath needs correction for incoming
                string? incomingEdgeDir = Path.GetDirectoryName(incomingEdgeFilePath);
                string incomingIndexFilePath = Path.Combine(incomingEdgeDir ?? "", "index.json");
                 Assert.That(File.Exists(incomingIndexFilePath), Is.True, "Incoming index file missing");

                 // Verify index content (optional, more detailed check)
                 EdgeIndexFileHandler.IndexFile outgoingIndex = LoadIndexFile(outgoingIndexFilePath); // Qualify type
                 Assert.That(outgoingIndex.EdgeFiles, Does.Contain(Path.GetFileName(outgoingEdgeFilePath)), "Outgoing index file does not contain edge"); // Use Does.Contain
                 EdgeIndexFileHandler.IndexFile incomingIndex = LoadIndexFile(incomingIndexFilePath); // Qualify type
                 Assert.That(incomingIndex.EdgeFiles, Does.Contain(Path.GetFileName(incomingEdgeFilePath)), "Incoming index file does not contain edge"); // Use Does.Contain
            });
        }

        [Test]
        public void TestDeleteEdgeUpdatesIndex()
        {
            var edge = new Edge(1234567890123456, 6543210987654321, 1.5, "Test Edge");
            var fromNode = new Node(edge.FromNode, "From Node", DomainType.Truth);
            var toNode = new Node(edge.ToNode, "To Node", DomainType.Truth);

            // Create nodes first using the storage provider
            var options = new JsonSerializerOptions { WriteIndented = true }; // Options for System.Text.Json
            string fromNodeData = JsonSerializer.Serialize(fromNode, options);
            string toNodeData = JsonSerializer.Serialize(toNode, options);
            Assert.Multiple(() =>
            {
                Assert.That(storageProvider.SaveNodeDataAsync(fromNode.Id, fromNodeData).Result, Is.True, "Failed to save FromNode"); 
                Assert.That(storageProvider.SaveNodeDataAsync(toNode.Id, toNodeData).Result, Is.True, "Failed to save ToNode"); 
            });

            // Save edge using the storage provider
            string edgeData = JsonSerializer.Serialize(edge, options);
            Assert.That(storageProvider.SaveEdgeDataAsync(edge.EdgeId, edge.FromNode, edge.ToNode, edgeData).Result, Is.True, "Failed to save edge");

            // Get file paths using FilePathHelper
            string outgoingEdgeFilePath = FilePathHelper.GetEdgeFilePath(tempDir, edge.FromNode, edge.ToNode, true); 
            string? outgoingEdgeDir = Path.GetDirectoryName(outgoingEdgeFilePath);
            string outgoingIndexFilePath = Path.Combine(outgoingEdgeDir ?? "", "index.json");

            string incomingEdgeFilePath = FilePathHelper.GetEdgeFilePath(tempDir, edge.ToNode, edge.FromNode, false); // Note: GetEdgeFilePath needs correction for incoming
            string? incomingEdgeDir = Path.GetDirectoryName(incomingEdgeFilePath);
            string incomingIndexFilePath = Path.Combine(incomingEdgeDir ?? "", "index.json");

            // Ensure edge and index files exist
            Assert.Multiple(() =>
            {
                Assert.That(File.Exists(outgoingEdgeFilePath), Is.True);
                Assert.That(File.Exists(outgoingIndexFilePath), Is.True);
                Assert.That(File.Exists(incomingEdgeFilePath), Is.True);
                Assert.That(File.Exists(incomingIndexFilePath), Is.True);
            });

            // Delete the edge using the provider
            Assert.That(storageProvider.DeleteEdgeDataAsync(edge.FromNode, edge.ToNode).Result, Is.True); 

            // Edge files should be deleted
            Assert.Multiple(() =>
            {
                Assert.That(File.Exists(outgoingEdgeFilePath), Is.False, "Outgoing edge file not deleted");
                Assert.That(File.Exists(incomingEdgeFilePath), Is.False, "Incoming edge file not deleted");
            });

            // Index files should be updated
            EdgeIndexFileHandler.IndexFile outgoingIndex = LoadIndexFile(outgoingIndexFilePath); // Qualify type
            EdgeIndexFileHandler.IndexFile incomingIndex = LoadIndexFile(incomingIndexFilePath); // Qualify type

            Assert.Multiple(() =>
            {
                Assert.That(outgoingIndex.EdgeFiles, Does.Not.Contain(Path.GetFileName(outgoingEdgeFilePath))); // Use Does.Not.Contain
                Assert.That(incomingIndex.EdgeFiles, Does.Not.Contain(Path.GetFileName(incomingEdgeFilePath))); // Use Does.Not.Contain
            });
        }

        [Test]
        public void TestLoadEdgesUsingIndexFiles()
        {
            var edge1 = new Edge(1234567890123456, 6543210987654321, 1.0, "Edge 1");
            var edge2 = new Edge(1234567890123456, 7654321098765432, 2.0, "Edge 2");
            var edge3 = new Edge(1234567890123456, 8765432109876543, 3.0, "Edge 3");
            var sourceNode = new Node(1234567890123456, "Source Node", DomainType.Truth);
            var destNode1 = new Node(6543210987654321, "Dest Node 1", DomainType.Truth);
            var destNode2 = new Node(7654321098765432, "Dest Node 2", DomainType.Truth);
            var destNode3 = new Node(8765432109876543, "Dest Node 3", DomainType.Truth);

            // Create nodes first using the storage provider
            var options = new JsonSerializerOptions { WriteIndented = true }; // Options for System.Text.Json
            Assert.Multiple(() =>
            {
                Assert.That(storageProvider.SaveNodeDataAsync(sourceNode.Id, JsonSerializer.Serialize(sourceNode, options)).Result, Is.True);
                Assert.That(storageProvider.SaveNodeDataAsync(destNode1.Id, JsonSerializer.Serialize(destNode1, options)).Result, Is.True);
                Assert.That(storageProvider.SaveNodeDataAsync(destNode2.Id, JsonSerializer.Serialize(destNode2, options)).Result, Is.True);
                Assert.That(storageProvider.SaveNodeDataAsync(destNode3.Id, JsonSerializer.Serialize(destNode3, options)).Result, Is.True);
            });


            // Save edges using provider
            Assert.Multiple(() =>
            {
                Assert.That(storageProvider.SaveEdgeDataAsync(edge1.EdgeId, edge1.FromNode, edge1.ToNode, JsonSerializer.Serialize(edge1, options)).Result, Is.True);
                Assert.That(storageProvider.SaveEdgeDataAsync(edge2.EdgeId, edge2.FromNode, edge2.ToNode, JsonSerializer.Serialize(edge2, options)).Result, Is.True);
                Assert.That(storageProvider.SaveEdgeDataAsync(edge3.EdgeId, edge3.FromNode, edge3.ToNode, JsonSerializer.Serialize(edge3, options)).Result, Is.True);
            });

            // Test GetOutgoingEdgeIdsAsync
            var outgoingEdgeIds = storageProvider.GetOutgoingEdgeIdsAsync(1234567890123456).Result; 
            Assert.Multiple(() => // Already wrapped
            {
                Assert.That(outgoingEdgeIds, Has.Count.EqualTo(3)); 
                Assert.That(outgoingEdgeIds, Contains.Item(edge1.EdgeId));
                Assert.That(outgoingEdgeIds, Contains.Item(edge2.EdgeId));
                Assert.That(outgoingEdgeIds, Contains.Item(edge3.EdgeId));
            });

            // Test GetIncomingEdgeIdsAsync
            var incomingEdgeIds1 = storageProvider.GetIncomingEdgeIdsAsync(6543210987654321).Result; 
            var incomingEdgeIds2 = storageProvider.GetIncomingEdgeIdsAsync(7654321098765432).Result; 
            var incomingEdgeIds3 = storageProvider.GetIncomingEdgeIdsAsync(8765432109876543).Result; 

            Assert.Multiple(() => // Already wrapped
            {
                Assert.That(incomingEdgeIds1, Has.Count.EqualTo(1)); 
                Assert.That(incomingEdgeIds2, Has.Count.EqualTo(1)); 
                Assert.That(incomingEdgeIds3, Has.Count.EqualTo(1)); 
                Assert.That(incomingEdgeIds1, Contains.Item(edge1.EdgeId));
                Assert.That(incomingEdgeIds2, Contains.Item(edge2.EdgeId));
                Assert.That(incomingEdgeIds3, Contains.Item(edge3.EdgeId));
            });

             // Verify index files exist and are correct (similar check as before, using FilePathHelper)
             var edgesToCheck = new List<Edge> { edge1, edge2, edge3 }; 
             foreach (var edge in edgesToCheck)
             {
                 string edgeFilePath = FilePathHelper.GetEdgeFilePath(tempDir, edge.FromNode, edge.ToNode, true); 
                 string? edgeDir = Path.GetDirectoryName(edgeFilePath);
                 string indexFilePath = Path.Combine(edgeDir ?? "", "index.json");
                 Assert.That(File.Exists(indexFilePath), Is.True, $"Index file missing for edge {edge.EdgeId}");
                 EdgeIndexFileHandler.IndexFile indexFile = LoadIndexFile(indexFilePath); // Qualify type
                 Assert.That(indexFile.EdgeFiles, Does.Contain(Path.GetFileName(edgeFilePath)), $"Index file for edge {edge.EdgeId} missing entry"); // Use Does.Contain
             }
        }

        [Test]
        public void TestSaveAndLoadNodeWithLargeId()
        {
            long largeId = 9223372036854775807; // Max long value
            // Use V3 Variable constructor with default DomainType
            var node = new Node(largeId, "Large ID Node", DomainType.Truth);
            var options = new JsonSerializerOptions { WriteIndented = true }; // Options for System.Text.Json
            string nodeData = JsonSerializer.Serialize(node, options);
            Assert.That(storageProvider.SaveNodeDataAsync(node.Id, nodeData).Result, Is.True);

            string? loadedNodeData = storageProvider.GetNodeDataAsync(largeId).Result;
            Assert.That(loadedNodeData, Is.Not.Null.And.Not.Empty, "Loaded node data should not be null");
            // Deserialize as NodeV3 because that's what GraphObjectMapper does now
            NodeV3? loadedNode = JsonSerializer.Deserialize<NodeV3>(loadedNodeData!); // Changed from JsonConvert and Node to NodeV3
            if (loadedNode != null)
            {
                Assert.Multiple(() =>
                {
                    Assert.That(loadedNode.Id, Is.EqualTo(largeId));
                    Assert.That(loadedNode.Content, Is.EqualTo("Large ID Node"));
                });
            }
            else
            {
                 Assert.Fail("Failed to deserialize loaded node data");
            }
        }

        [Test]
        public void TestSaveAndLoadMultipleNodes()
        {
            // Use V3 Variable constructor with default DomainType
            var nodes = new List<Node> 
            {
                new(1, "Node One", DomainType.Truth), 
                new(2, "Node Two", DomainType.Truth), 
                new(3, "Node Three", DomainType.Truth) // Simplified new()
            };

            var options = new JsonSerializerOptions { WriteIndented = true }; // Options for System.Text.Json
            foreach (var node in nodes)
            {
                 string nodeData = JsonSerializer.Serialize(node, options);
                 Assert.That(storageProvider.SaveNodeDataAsync(node.Id, nodeData).Result, Is.True);
            }

            foreach (var node in nodes)
            {
                string? loadedNodeData = storageProvider.GetNodeDataAsync(node.Id).Result;
                Assert.That(loadedNodeData, Is.Not.Null.And.Not.Empty, $"Loaded node data for {node.Id} should not be null");
                // Deserialize as NodeV3 because that's what GraphObjectMapper does now
                NodeV3? loadedNode = JsonSerializer.Deserialize<NodeV3>(loadedNodeData!); // Changed from JsonConvert and Node to NodeV3
                if (loadedNode != null)
                {
                    Assert.Multiple(() =>
                    {
                        Assert.That(loadedNode.Id, Is.EqualTo(node.Id));
                        Assert.That(loadedNode.Content, Is.EqualTo(node.Content));
                    });
                }
                 else
                {
                    Assert.Fail($"Failed to deserialize loaded node data for node {node.Id}");
                }
            }
        }

        [Test]
        public void TestDeleteNodeWithEdges()
        {
             // Use V3 Variable constructor with default DomainType
            var node1 = new Node(1, "Node One", DomainType.Truth);
            var node2 = new Node(2, "Node Two", DomainType.Truth);
            var edge = new Edge(1, 2, 1.0, "Test Edge");

            var options = new JsonSerializerOptions { WriteIndented = true }; // Options for System.Text.Json
            string node1Data = JsonSerializer.Serialize(node1, options);
            string node2Data = JsonSerializer.Serialize(node2, options);
            string edgeData = JsonSerializer.Serialize(edge, options);
            storageProvider.SaveNodeDataAsync(node1.Id, node1Data).Wait();
            storageProvider.SaveNodeDataAsync(node2.Id, node2Data).Wait();
            storageProvider.SaveEdgeDataAsync(edge.EdgeId, edge.FromNode, edge.ToNode, edgeData).Wait(); 

            Assert.That(storageProvider.DeleteNodeDataAsync(1).Result, Is.True); 

            // Verify node is gone
            Assert.Multiple(() =>
            {
                // Verify node is gone
                Assert.That(storageProvider.GetNodeDataAsync(1).Result, Is.Null); 
                // Verify edge files are gone (as DeleteNodeDataAsync implementation deletes them)
                string outgoingEdgeFilePath = FilePathHelper.GetEdgeFilePath(tempDir, edge.FromNode, edge.ToNode, true); 
                string incomingEdgeFilePath = FilePathHelper.GetEdgeFilePath(tempDir, edge.ToNode, edge.FromNode, false); // Needs correction
                Assert.That(File.Exists(outgoingEdgeFilePath), Is.False, "Outgoing edge file should be deleted with node");
                Assert.That(File.Exists(incomingEdgeFilePath), Is.False, "Incoming edge file should be deleted with node");
                // Verify edge IDs are gone from lists
                Assert.That(storageProvider.GetOutgoingEdgeIdsAsync(1).Result, Is.Empty); 
                Assert.That(storageProvider.GetIncomingEdgeIdsAsync(2).Result, Is.Empty); 
            });
        }

        [Test]
        public void TestSaveAndLoadEdgeWithLargeNodeIds()
        {
            long largeId1 = 9223372036854775806;
            long largeId2 = 9223372036854775807;

             // Use V3 Variable constructor with default DomainType
            var node1 = new Node(largeId1, "Large Node One", DomainType.Truth);
            var node2 = new Node(largeId2, "Large Node Two", DomainType.Truth);
            var edge = new Edge(largeId1, largeId2, 1.0, "Large ID Edge");

            var options = new JsonSerializerOptions { WriteIndented = true }; // Options for System.Text.Json
            string node1Data = JsonSerializer.Serialize(node1, options);
            string node2Data = JsonSerializer.Serialize(node2, options);
            string edgeData = JsonSerializer.Serialize(edge, options);
            storageProvider.SaveNodeDataAsync(node1.Id, node1Data).Wait();
            storageProvider.SaveNodeDataAsync(node2.Id, node2Data).Wait();
            Assert.That(storageProvider.SaveEdgeDataAsync(edge.EdgeId, edge.FromNode, edge.ToNode, edgeData).Result, Is.True); 

            var loadedEdgeIds = storageProvider.GetOutgoingEdgeIdsAsync(largeId1).Result; 
            Assert.Multiple(() =>
            {
                Assert.That(loadedEdgeIds, Has.Count.EqualTo(1));
                Assert.That(loadedEdgeIds, Contains.Item(edge.EdgeId));
            });

            // Optionally load and verify the edge data
            string? loadedEdgeData = storageProvider.GetEdgeDataAsync(edge.EdgeId).Result;
            Assert.That(loadedEdgeData, Is.Not.Null, "Loaded edge data should not be null");
            Edge? loadedEdge = JsonSerializer.Deserialize<Edge>(loadedEdgeData!); // Changed from JsonConvert
             if (loadedEdge != null)
             {
                 Assert.Multiple(() =>
                 {
                    Assert.That(loadedEdge.FromNode, Is.EqualTo(largeId1));
                    Assert.That(loadedEdge.ToNode, Is.EqualTo(largeId2));
                 });
             }
             else
             {
                 Assert.Fail("Failed to deserialize loaded edge data");
             }
        }

        [Test]
        public void TestEdgeConsistencyAfterNodeDeletion()
        {
             // Use V3 Variable constructor with default DomainType
            var node1 = new Node(1, "Node One", DomainType.Truth);
            var node2 = new Node(2, "Node Two", DomainType.Truth);
            var edge = new Edge(1, 2, 1.0, "Test Edge");

            var options = new JsonSerializerOptions { WriteIndented = true }; // Options for System.Text.Json
            string node1Data = JsonSerializer.Serialize(node1, options);
            string node2Data = JsonSerializer.Serialize(node2, options);
            string edgeData = JsonSerializer.Serialize(edge, options);
            storageProvider.SaveNodeDataAsync(node1.Id, node1Data).Wait();
            storageProvider.SaveNodeDataAsync(node2.Id, node2Data).Wait();
            storageProvider.SaveEdgeDataAsync(edge.EdgeId, edge.FromNode, edge.ToNode, edgeData).Wait(); 

            storageProvider.DeleteNodeDataAsync(1).Wait(); // Delete node 1

            // Verify incoming edges to node 2 are gone
            var incomingEdgeIds = storageProvider.GetIncomingEdgeIdsAsync(2).Result; 
            Assert.That(incomingEdgeIds, Is.Empty, "Incoming edges to Node 2 should be empty after deleting Node 1");
        }

        [Test]
        public void TestGetAllNodeIds()
        {
             // Use V3 Variable constructor with default DomainType
            var nodes = new List<Node>
            {
                new(1, "Node One", DomainType.Truth),
                new(2, "Node Two", DomainType.Truth),
                new(3, "Node Three", DomainType.Truth) // Simplified new()
            };

            var options = new JsonSerializerOptions { WriteIndented = true }; // Options for System.Text.Json
            foreach (var node in nodes)
            {
                string nodeData = JsonSerializer.Serialize(node, options);
                storageProvider.SaveNodeDataAsync(node.Id, nodeData).Wait();
            }

            var allNodeIds = storageProvider.GetAllNodeIdsAsync().Result;
            Assert.That(allNodeIds, Is.EquivalentTo(new List<long> { 1, 2, 3 }));

            storageProvider.DeleteNodeDataAsync(2).Wait(); // Delete node 2

            allNodeIds = storageProvider.GetAllNodeIdsAsync().Result; 
            Assert.That(allNodeIds, Is.EquivalentTo(new List<long> { 1, 3 }));
        }

        [Test]
        public void TestSaveEdgeWithNonExistentNodes()
        {
            var edge = new Edge(1, 2, 1.0, "Test Edge");
            // Act: Attempt to save the edge using the GraphObjectMapper, which should perform the node existence check
            bool result = graphObjectMapper.SaveEdgeAsync(edge).Result;

            // Assert: Saving should fail because the ObjectMapper checks for node existence
            Assert.That(result, Is.False, "Saving an edge with non-existent nodes should fail via ObjectMapper");
        }

        [Test]
        public void TestUpdateEdge()
        {
             // Use V3 Variable constructor with default DomainType
            var node1 = new Node(1, "Node One", DomainType.Truth);
            var node2 = new Node(2, "Node Two", DomainType.Truth);
            var edge = new Edge(1, 2, 1.0, "Original Edge");

            var options = new JsonSerializerOptions { WriteIndented = true }; // Options for System.Text.Json
            string node1Data = JsonSerializer.Serialize(node1, options);
            string node2Data = JsonSerializer.Serialize(node2, options);
            string edgeData = JsonSerializer.Serialize(edge, options);
            storageProvider.SaveNodeDataAsync(node1.Id, node1Data).Wait();
            storageProvider.SaveNodeDataAsync(node2.Id, node2Data).Wait();
            storageProvider.SaveEdgeDataAsync(edge.EdgeId, edge.FromNode, edge.ToNode, edgeData).Wait(); 

            // Load the original edge using the ObjectMapper
            EdgeV2? originalEdge = graphObjectMapper.GetEdgeAsync(edge.EdgeId).Result;
            Assert.That(originalEdge, Is.Not.Null, "Failed to load original edge for update");

            // Modify the loaded edge object
            originalEdge!.Weight = 2.0; // Use null-forgiving operator as we asserted Not.Null
            originalEdge.EdgeContent = "Updated Edge";

            // Save the modified edge object using the ObjectMapper
            Assert.That(graphObjectMapper.SaveEdgeAsync(originalEdge).Result, Is.True, "Failed to save updated edge via ObjectMapper");

            // Load the edge data again using the original Guid and verify
            EdgeV2? loadedEdge = graphObjectMapper.GetEdgeAsync(edge.EdgeId).Result; // Use ObjectMapper to load
            Assert.That(loadedEdge, Is.Not.Null, "Loaded edge data should not be null after update");
            
            if (loadedEdge != null)
            {
                Assert.Multiple(() =>
                {
                    Assert.That(loadedEdge.Weight, Is.EqualTo(2.0));
                    Assert.That(loadedEdge.EdgeContent, Is.EqualTo("Updated Edge"));
                    Assert.That(loadedEdge.EdgeId, Is.EqualTo(edge.EdgeId)); // Ensure Guid hasn't changed
                });
            }
             else
            {
             Assert.Fail("Failed to deserialize loaded edge data");
             }
        }

        [Test]
        public void TestSaveAndLoadVariableNodeWithDistribution()
        {
            // Arrange
            var node = new Node(101, "Variable Node", DomainType.Continuous);
            node.Distribution?.AddRange(0.0, 1.0, 0.3); // Use null-conditional access
            node.Distribution?.AddRange(1.0 + 1e-9, 2.0, 0.7); // Add another range

            // Act: Save using ObjectMapper
            Assert.That(graphObjectMapper.SaveNodeAsync(node).Result, Is.True, "Failed to save node via ObjectMapper");

            // Act: Load using ObjectMapper
            NodeV3? loadedNode = graphObjectMapper.GetNodeAsync(node.Id).Result;

            // Assert
            Assert.That(loadedNode, Is.Not.Null, "Loaded node should not be null");
            if (loadedNode != null)
            {
                Assert.Multiple(() =>
                {
                    Assert.That(loadedNode.Id, Is.EqualTo(node.Id));
                    Assert.That(loadedNode.Content, Is.EqualTo(node.Content));
                    Assert.That(loadedNode.Role, Is.EqualTo(NodeRole.Variable));
                    Assert.That(loadedNode.Distribution, Is.Not.Null, "Loaded distribution should not be null");
                    Assert.That(loadedNode.Distribution?.DomainType, Is.EqualTo(DomainType.Continuous)); // Use null-conditional
                    Assert.That(loadedNode.Distribution?.Distribution, Has.Count.EqualTo(2), "Distribution should have 2 ranges"); // Use null-conditional

                    if (loadedNode.Distribution?.Distribution.Count == 2) // Check count before accessing elements
                    {
                        var loadedRanges = loadedNode.Distribution.Distribution;
                        Assert.That(loadedRanges[0].LowerBound, Is.EqualTo(0.0).Within(1e-12));
                        Assert.That(loadedRanges[0].UpperBound, Is.EqualTo(1.0).Within(1e-12));
                        Assert.That(loadedRanges[0].Probability, Is.EqualTo(0.3).Within(1e-12));
                        Assert.That(loadedRanges[1].LowerBound, Is.EqualTo(1.0 + 1e-9).Within(1e-12));
                        Assert.That(loadedRanges[1].UpperBound, Is.EqualTo(2.0).Within(1e-12));
                        Assert.That(loadedRanges[1].Probability, Is.EqualTo(0.7).Within(1e-12));
                    }
                });
            }
        }

        [Test]
        public void TestSaveAndLoadFunctionNodeWithParams()
        {
            // Arrange
            var funcParams = new Dictionary<string, object>
            {
                { "Weights", new List<double> { 0.5, -0.2 } },
                { "Bias", 1.5 }
            };
            var node = new Node(102, "Function Node", FunctionType.Linear, funcParams);

            // Act: Save using ObjectMapper
            Assert.That(graphObjectMapper.SaveNodeAsync(node).Result, Is.True, "Failed to save node via ObjectMapper");

            // Act: Load using ObjectMapper
            NodeV3? loadedNode = graphObjectMapper.GetNodeAsync(node.Id).Result;

            // Assert
            Assert.That(loadedNode, Is.Not.Null, "Loaded node should not be null");
            if (loadedNode != null)
            {
                Assert.Multiple(() =>
                {
                    Assert.That(loadedNode.Id, Is.EqualTo(node.Id));
                    Assert.That(loadedNode.Content, Is.EqualTo(node.Content));
                    Assert.That(loadedNode.Role, Is.EqualTo(NodeRole.Function));
                    Assert.That(loadedNode.Function, Is.EqualTo(FunctionType.Linear));
                    Assert.That(loadedNode.FunctionParams, Is.Not.Null, "Loaded FunctionParams should not be null");

                    if (loadedNode.FunctionParams != null)
                    {
                        // Verify Weights (needs careful handling due to object type)
                        Assert.That(loadedNode.FunctionParams.ContainsKey("Weights"), Is.True, "Loaded params missing 'Weights'");
                        Assert.That(loadedNode.FunctionParams["Weights"], Is.InstanceOf<List<double>>(), "'Weights' should be List<double>");
                        if (loadedNode.FunctionParams["Weights"] is List<double> loadedWeights)
                        {
                            Assert.That(loadedWeights, Is.EqualTo(new List<double> { 0.5, -0.2 }), "Weights mismatch");
                        }

                        // Verify Bias
                        Assert.That(loadedNode.FunctionParams.ContainsKey("Bias"), Is.True, "Loaded params missing 'Bias'");
                        Assert.That(loadedNode.FunctionParams["Bias"], Is.InstanceOf<double>(), "'Bias' should be double");
                        Assert.That(loadedNode.FunctionParams["Bias"], Is.EqualTo(1.5).Within(1e-12), "Bias mismatch");
                    }
                });
            }
        }

        [Test]
        public void TestSaveAndLoadEdgeWithExtendedProperties()
        {
            // Arrange
            var fromNode = new Node(201, "From Node Ext", DomainType.Truth);
            var toNode = new Node(202, "To Node Ext", DomainType.Truth);
            var edge = new Edge(fromNode.Id, toNode.Id, 1.0, "Edge With Ext Props");
            edge.SetExtendedProperty("SourceSystem", "SystemA");
            edge.SetExtendedProperty("Confidence", 0.95);
            edge.SetExtendedProperty("IsTemporary", false);

            // Save nodes first
            Assert.That(graphObjectMapper.SaveNodeAsync(fromNode).Result, Is.True);
            Assert.That(graphObjectMapper.SaveNodeAsync(toNode).Result, Is.True);

            // Act: Save edge using ObjectMapper
            Assert.That(graphObjectMapper.SaveEdgeAsync(edge).Result, Is.True, "Failed to save edge via ObjectMapper");

            // Act: Load edge using ObjectMapper
            EdgeV2? loadedEdge = graphObjectMapper.GetEdgeAsync(edge.EdgeId).Result;

            // Assert
            Assert.That(loadedEdge, Is.Not.Null, "Loaded edge should not be null");
            if (loadedEdge != null)
            {
                Assert.Multiple(() =>
                {
                    Assert.That(loadedEdge.EdgeId, Is.EqualTo(edge.EdgeId));
                    Assert.That(loadedEdge.FromNode, Is.EqualTo(edge.FromNode));
                    Assert.That(loadedEdge.ToNode, Is.EqualTo(edge.ToNode));
                    Assert.That(loadedEdge.ExtendedProperties, Is.Not.Null, "Loaded ExtendedProperties should not be null");
                    Assert.That(loadedEdge.ExtendedProperties.Count, Is.EqualTo(3), "ExtendedProperties count mismatch");

                    // Verify specific properties using the GetExtendedProperty<T> helper, which handles JsonElement conversion
                    Assert.That(loadedEdge.GetExtendedProperty<string>("SourceSystem"), Is.EqualTo("SystemA"));
                    Assert.That(loadedEdge.GetExtendedProperty<double>("Confidence"), Is.EqualTo(0.95).Within(1e-12));
                    Assert.That(loadedEdge.GetExtendedProperty<bool>("IsTemporary"), Is.EqualTo(false));
                });
            }
        }


        // Make static as it doesn't use instance members
        private static EdgeIndexFileHandler.IndexFile LoadIndexFile(string indexFilePath) // Use the public nested class from EdgeIndexFileHandler
        {
            string json = File.ReadAllText(indexFilePath);
            // Changed from JsonConvert
            return JsonSerializer.Deserialize<EdgeIndexFileHandler.IndexFile>(json) ?? new EdgeIndexFileHandler.IndexFile(); // Use the public nested class from EdgeIndexFileHandler
        }
    }
}
