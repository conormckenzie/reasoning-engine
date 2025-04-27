using System; // Removed Newtonsoft.Json
using System.Collections.Generic;
using System.IO;
using System.Text.Json; // Added System.Text.Json

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
                // Use System.Text.Json with case-insensitive option
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                indexData = JsonSerializer.Deserialize<Index>(json, options) ?? new Index { Nodes = new List<NodeInfo>() };
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
            // Use System.Text.Json with indented formatting
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(indexData, options);
            File.WriteAllText(indexFilePath, json);
        }

        /// <summary>
        /// Adds a new node or updates an existing node in the index.
        /// </summary>
        /// <param name="nodeId">The ID of the node.</param>
        /// <param name="filePath">The file path where the node is stored.</param>
        // Removed edgeCount parameter as it's unused and unreliable
        public void AddOrUpdateNode(long nodeId, string filePath)
        {
            var nodeInfo = indexData.Nodes.Find(n => n.NodeId == nodeId);
            if (nodeInfo == null)
            {
                // Removed EdgeCount initialization
                nodeInfo = new NodeInfo { NodeId = nodeId, FilePath = filePath };
                indexData.Nodes.Add(nodeInfo);
                indexData.TotalNodes++;
            }
            else
            {
                nodeInfo.FilePath = filePath;
                // Removed EdgeCount update
            }
            // Removed TotalEdges update
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
                // Removed TotalEdges update
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

        // Removed GetTotalEdges and CalculateTotalEdges methods
    }

    public class Index
    {
        public int TotalNodes { get; set; }
        // Removed TotalEdges property
        public List<NodeInfo> Nodes { get; set; } = new List<NodeInfo>();
    }

    public class NodeInfo
    {
        public long NodeId { get; set; }
        public string FilePath { get; set; } = string.Empty;
        // Removed EdgeCount property
    }
}
