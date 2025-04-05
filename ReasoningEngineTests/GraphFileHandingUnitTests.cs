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
        // Justification for null-forgiving operator (!):
        // These fields are initialized in the [SetUp] method, which NUnit guarantees
        // runs before each test execution. Therefore, they will not be null when accessed in tests.
        private FileGraphStorageProvider storageProvider = null!;
        private string tempDir = null!;

        [SetUp]
        public void Setup()
        {
            tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDir);
            // Instantiate the provider
            storageProvider = new FileGraphStorageProvider(tempDir); 
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
            string fromNodeData = JsonConvert.SerializeObject(fromNode, Formatting.Indented);
            string toNodeData = JsonConvert.SerializeObject(toNode, Formatting.Indented);
            Assert.Multiple(() =>
            {
                Assert.That(storageProvider.SaveNodeDataAsync(fromNode.Id, fromNodeData).Result, Is.True, "Failed to save FromNode"); 
                Assert.That(storageProvider.SaveNodeDataAsync(toNode.Id, toNodeData).Result, Is.True, "Failed to save ToNode"); 
            });

            // Save edge using the storage provider
            string edgeData = JsonConvert.SerializeObject(edge, Formatting.Indented);
            Assert.That(storageProvider.SaveEdgeDataAsync(edge.EdgeId, edge.FromNode, edge.ToNode, edgeData).Result, Is.True, "Failed to save edge"); 

            // Load edge data using the provider (by From/To IDs)
            string? loadedEdgeData = storageProvider.GetEdgeDataAsync(edge.FromNode, edge.ToNode).Result; 
            Assert.That(loadedEdgeData, Is.Not.Null.And.Not.Empty, "Failed to load edge data");

            // Deserialize and verify
            Edge? loadedEdge = JsonConvert.DeserializeObject<Edge>(loadedEdgeData!); // Added '!' after null check
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

            // Verify index files exist (using provider's helper methods to get paths)
            string outgoingEdgeFilePath = storageProvider.GetEdgeFilePath(edge.FromNode, edge.ToNode, true); 
            string? outgoingEdgeDir = Path.GetDirectoryName(outgoingEdgeFilePath);
            string outgoingIndexFilePath = Path.Combine(outgoingEdgeDir ?? "", "index.json");
            Assert.Multiple(() =>
            {
                Assert.That(File.Exists(outgoingIndexFilePath), Is.True, "Outgoing index file missing");

                string incomingEdgeFilePath = storageProvider.GetEdgeFilePath(edge.ToNode, edge.FromNode, false); // Note: GetEdgeFilePath needs correction for incoming
                string? incomingEdgeDir = Path.GetDirectoryName(incomingEdgeFilePath);
                string incomingIndexFilePath = Path.Combine(incomingEdgeDir ?? "", "index.json");
                Assert.That(File.Exists(incomingIndexFilePath), Is.True, "Incoming index file missing");

                 // Verify index content (optional, more detailed check)
                 IndexFile outgoingIndex = LoadIndexFile(outgoingIndexFilePath);
                 Assert.That(outgoingIndex.EdgeFiles, Does.Contain(Path.GetFileName(outgoingEdgeFilePath)), "Outgoing index file does not contain edge"); // Use Does.Contain
                 IndexFile incomingIndex = LoadIndexFile(incomingIndexFilePath);
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
            string fromNodeData = JsonConvert.SerializeObject(fromNode, Formatting.Indented);
            string toNodeData = JsonConvert.SerializeObject(toNode, Formatting.Indented);
            Assert.Multiple(() =>
            {
                Assert.That(storageProvider.SaveNodeDataAsync(fromNode.Id, fromNodeData).Result, Is.True, "Failed to save FromNode"); 
                Assert.That(storageProvider.SaveNodeDataAsync(toNode.Id, toNodeData).Result, Is.True, "Failed to save ToNode"); 
            });
    
            // Save edge using the storage provider
            string edgeData = JsonConvert.SerializeObject(edge, Formatting.Indented);
            Assert.That(storageProvider.SaveEdgeDataAsync(edge.EdgeId, edge.FromNode, edge.ToNode, edgeData).Result, Is.True, "Failed to save edge"); 

            // Get file paths using the provider's helper methods
            string outgoingEdgeFilePath = storageProvider.GetEdgeFilePath(edge.FromNode, edge.ToNode, true); 
            string? outgoingEdgeDir = Path.GetDirectoryName(outgoingEdgeFilePath);
            string outgoingIndexFilePath = Path.Combine(outgoingEdgeDir ?? "", "index.json");

            string incomingEdgeFilePath = storageProvider.GetEdgeFilePath(edge.ToNode, edge.FromNode, false); // Note: GetEdgeFilePath needs correction for incoming
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
            IndexFile outgoingIndex = LoadIndexFile(outgoingIndexFilePath);
            IndexFile incomingIndex = LoadIndexFile(incomingIndexFilePath);

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
            Assert.Multiple(() =>
            {
                Assert.That(storageProvider.SaveNodeDataAsync(sourceNode.Id, JsonConvert.SerializeObject(sourceNode)).Result, Is.True); 
                Assert.That(storageProvider.SaveNodeDataAsync(destNode1.Id, JsonConvert.SerializeObject(destNode1)).Result, Is.True); 
                Assert.That(storageProvider.SaveNodeDataAsync(destNode2.Id, JsonConvert.SerializeObject(destNode2)).Result, Is.True); 
                Assert.That(storageProvider.SaveNodeDataAsync(destNode3.Id, JsonConvert.SerializeObject(destNode3)).Result, Is.True); 
            });


            // Save edges using provider
            Assert.Multiple(() =>
            {
                Assert.That(storageProvider.SaveEdgeDataAsync(edge1.EdgeId, edge1.FromNode, edge1.ToNode, JsonConvert.SerializeObject(edge1)).Result, Is.True); 
                Assert.That(storageProvider.SaveEdgeDataAsync(edge2.EdgeId, edge2.FromNode, edge2.ToNode, JsonConvert.SerializeObject(edge2)).Result, Is.True); 
                Assert.That(storageProvider.SaveEdgeDataAsync(edge3.EdgeId, edge3.FromNode, edge3.ToNode, JsonConvert.SerializeObject(edge3)).Result, Is.True); 
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

             // Verify index files exist and are correct (similar check as before, using provider paths)
             var edgesToCheck = new List<Edge> { edge1, edge2, edge3 }; 
             foreach (var edge in edgesToCheck)
             {
                 string edgeFilePath = storageProvider.GetEdgeFilePath(edge.FromNode, edge.ToNode, true); 
                 string? edgeDir = Path.GetDirectoryName(edgeFilePath);
                 string indexFilePath = Path.Combine(edgeDir ?? "", "index.json");
                 Assert.That(File.Exists(indexFilePath), Is.True, $"Index file missing for edge {edge.EdgeId}");
                 IndexFile indexFile = LoadIndexFile(indexFilePath);
                 Assert.That(indexFile.EdgeFiles, Does.Contain(Path.GetFileName(edgeFilePath)), $"Index file for edge {edge.EdgeId} missing entry"); // Use Does.Contain
             }
        }

        [Test]
        public void TestSaveAndLoadNodeWithLargeId()
        {
            long largeId = 9223372036854775807; // Max long value
            // Use V3 Variable constructor with default DomainType
            var node = new Node(largeId, "Large ID Node", DomainType.Truth); 
            string nodeData = JsonConvert.SerializeObject(node, Formatting.Indented);
            Assert.That(storageProvider.SaveNodeDataAsync(node.Id, nodeData).Result, Is.True); 

            string? loadedNodeData = storageProvider.GetNodeDataAsync(largeId).Result; 
            Assert.That(loadedNodeData, Is.Not.Null.And.Not.Empty, "Loaded node data should not be null");
            Node? loadedNode = JsonConvert.DeserializeObject<Node>(loadedNodeData!); // Added '!' after null check
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

            foreach (var node in nodes)
            {
                 string nodeData = JsonConvert.SerializeObject(node, Formatting.Indented);
                 Assert.That(storageProvider.SaveNodeDataAsync(node.Id, nodeData).Result, Is.True); 
            }

            foreach (var node in nodes)
            {
                string? loadedNodeData = storageProvider.GetNodeDataAsync(node.Id).Result; 
                Assert.That(loadedNodeData, Is.Not.Null.And.Not.Empty, $"Loaded node data for {node.Id} should not be null");
                Node? loadedNode = JsonConvert.DeserializeObject<Node>(loadedNodeData!); // Added '!' after null check
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

            string node1Data = JsonConvert.SerializeObject(node1, Formatting.Indented);
            string node2Data = JsonConvert.SerializeObject(node2, Formatting.Indented);
            string edgeData = JsonConvert.SerializeObject(edge, Formatting.Indented);
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
                string outgoingEdgeFilePath = storageProvider.GetEdgeFilePath(edge.FromNode, edge.ToNode, true); 
                string incomingEdgeFilePath = storageProvider.GetEdgeFilePath(edge.ToNode, edge.FromNode, false); // Needs correction
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

            string node1Data = JsonConvert.SerializeObject(node1, Formatting.Indented);
            string node2Data = JsonConvert.SerializeObject(node2, Formatting.Indented);
            string edgeData = JsonConvert.SerializeObject(edge, Formatting.Indented);
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
            Edge? loadedEdge = JsonConvert.DeserializeObject<Edge>(loadedEdgeData!); // Added '!' after null check
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

            string node1Data = JsonConvert.SerializeObject(node1, Formatting.Indented);
            string node2Data = JsonConvert.SerializeObject(node2, Formatting.Indented);
            string edgeData = JsonConvert.SerializeObject(edge, Formatting.Indented);
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

            foreach (var node in nodes)
            {
                string nodeData = JsonConvert.SerializeObject(node, Formatting.Indented);
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
            string edgeData = JsonConvert.SerializeObject(edge, Formatting.Indented);
            // SaveEdgeDataAsync itself doesn't check node existence, but the underlying file provider might fail implicitly
            // or succeed but lead to dangling edges. The FileGraphStorageProvider SaveEdgeDataAsync implementation
            // *does* check node existence via a helper. Let's assume it returns false.
            // If the check was removed, this test would need adjustment.
            Assert.That(storageProvider.SaveEdgeDataAsync(edge.EdgeId, edge.FromNode, edge.ToNode, edgeData).Result, Is.False, "Saving an edge with non-existent nodes should fail"); 
        }

        [Test]
        public void TestUpdateEdge()
        {
             // Use V3 Variable constructor with default DomainType
            var node1 = new Node(1, "Node One", DomainType.Truth);
            var node2 = new Node(2, "Node Two", DomainType.Truth);
            var edge = new Edge(1, 2, 1.0, "Original Edge");

            string node1Data = JsonConvert.SerializeObject(node1, Formatting.Indented);
            string node2Data = JsonConvert.SerializeObject(node2, Formatting.Indented);
            string edgeData = JsonConvert.SerializeObject(edge, Formatting.Indented);
            storageProvider.SaveNodeDataAsync(node1.Id, node1Data).Wait(); 
            storageProvider.SaveNodeDataAsync(node2.Id, node2Data).Wait(); 
            storageProvider.SaveEdgeDataAsync(edge.EdgeId, edge.FromNode, edge.ToNode, edgeData).Wait(); 

            // Create updated edge object (constructor generates a new Guid, which is fine for the data part)
            var updatedEdgeDataOnly = new Edge(1, 2, 2.0, "Updated Edge"); 
            string updatedEdgeDataString = JsonConvert.SerializeObject(updatedEdgeDataOnly, Formatting.Indented);
            // Save using the *original* edge's Guid but the *new* data
            Assert.That(storageProvider.SaveEdgeDataAsync(edge.EdgeId, updatedEdgeDataOnly.FromNode, updatedEdgeDataOnly.ToNode, updatedEdgeDataString).Result, Is.True); 

            // Load the edge data using the original Guid and verify
            string? loadedEdgeData = storageProvider.GetEdgeDataAsync(edge.EdgeId).Result; 
            Assert.That(loadedEdgeData, Is.Not.Null, "Loaded edge data should not be null");
            Edge? loadedEdge = JsonConvert.DeserializeObject<Edge>(loadedEdgeData!); // Added '!' after null check
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

        // Make static as it doesn't use instance members
        private static IndexFile LoadIndexFile(string indexFilePath) 
        {
            string json = File.ReadAllText(indexFilePath);
            return JsonConvert.DeserializeObject<IndexFile>(json) ?? new IndexFile();
        }
    }
}
