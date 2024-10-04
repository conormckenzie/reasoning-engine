using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace ReasoningEngine.GraphFileHandling
{
    public class IndexManager
    {
        private Index indexData;
        private readonly string indexFilePath;

        public IndexManager(string indexFilePath)
        {
            this.indexFilePath = indexFilePath;
            this.indexData = new Index { Nodes = new List<NodeInfo>() };
            LoadIndex();
        }

        /// <summary>
        /// Loads the index data from the file, or initializes it if the file does not exist.
        /// </summary>
        private void LoadIndex()
        {
            if (File.Exists(indexFilePath))
            {
                string json = File.ReadAllText(indexFilePath);
                indexData = JsonConvert.DeserializeObject<Index>(json) ?? new Index { Nodes = new List<NodeInfo>() };
            }
            else
            {
                indexData = new Index { Nodes = new List<NodeInfo>() };
            }
        }

        /// <summary>
        /// Saves the index data to the file.
        /// </summary>
        private void SaveIndex()
        {
            string json = JsonConvert.SerializeObject(indexData, Formatting.Indented);
            File.WriteAllText(indexFilePath, json);
        }

        /// <summary>
        /// Adds a new node or updates an existing node in the index.
        /// </summary>
        /// <param name="nodeId">The ID of the node.</param>
        /// <param name="filePath">The file path where the node is stored.</param>
        /// <param name="edgeCount">The number of edges connected to the node.</param>
        public void AddOrUpdateNode(long nodeId, string filePath, int edgeCount)
        {
            var nodeInfo = indexData.Nodes.Find(n => n.NodeId == nodeId);
            if (nodeInfo == null)
            {
                nodeInfo = new NodeInfo { NodeId = nodeId, FilePath = filePath, EdgeCount = edgeCount };
                indexData.Nodes.Add(nodeInfo);
                indexData.TotalNodes++;
            }
            else
            {
                nodeInfo.FilePath = filePath;
                nodeInfo.EdgeCount = edgeCount;
            }
            indexData.TotalEdges = CalculateTotalEdges();
            SaveIndex();
        }

        /// <summary>
        /// Removes a node from the index.
        /// </summary>
        /// <param name="nodeId">The ID of the node to remove.</param>
        public void RemoveNode(long nodeId)
        {
            var nodeInfo = indexData.Nodes.Find(n => n.NodeId == nodeId);
            if (nodeInfo != null)
            {
                indexData.Nodes.Remove(nodeInfo);
                indexData.TotalNodes--;
                indexData.TotalEdges = CalculateTotalEdges();
                SaveIndex();
            }
        }

        /// <summary>
        /// Retrieves the list of node IDs from the index.
        /// </summary>
        /// <returns>A list of node IDs.</returns>
        public List<long> GetNodeIds()
        {
            return new List<long>(indexData.Nodes.ConvertAll(n => n.NodeId));
        }

        /// <summary>
        /// Gets the total number of nodes in the index.
        /// </summary>
        /// <returns>The total number of nodes.</returns>
        public int GetTotalNodes() => indexData.TotalNodes;

        /// <summary>
        /// Gets the total number of edges in the index.
        /// </summary>
        /// <returns>The total number of edges.</returns>
        public int GetTotalEdges() => indexData.TotalEdges;

        /// <summary>
        /// Calculates the total number of edges in the index.
        /// </summary>
        /// <returns>The total number of edges.</returns>
        private int CalculateTotalEdges()
        {
            int totalEdges = 0;
            foreach (var node in indexData.Nodes)
            {
                totalEdges += node.EdgeCount;
            }
            return totalEdges;
        }
    }

    public class Index
    {
        public int TotalNodes { get; set; }
        public int TotalEdges { get; set; }
        public List<NodeInfo> Nodes { get; set; } = new List<NodeInfo>();
    }

    public class NodeInfo
    {
        public long NodeId { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public int EdgeCount { get; set; }
    }
}
