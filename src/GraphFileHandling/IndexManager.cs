using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace GraphFileHandling
{
    public class IndexManager
    {
        private string indexFilePath;
        private Index indexData;

        public IndexManager(string indexFilePath)
        {
            this.indexFilePath = indexFilePath;
            LoadIndex();
        }

        private void LoadIndex()
        {
            if (File.Exists(indexFilePath))
            {
                string json = File.ReadAllText(indexFilePath);
                indexData = JsonConvert.DeserializeObject<Index>(json);
            }
            else
            {
                indexData = new Index { Nodes = new List<NodeInfo>() };
            }
        }

        private void SaveIndex()
        {
            string json = JsonConvert.SerializeObject(indexData, Formatting.Indented);
            File.WriteAllText(indexFilePath, json);
        }

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

        public List<long> GetNodeIds()
        {
            return new List<long>(indexData.Nodes.ConvertAll(n => n.NodeId));
        }

        public int GetTotalNodes() => indexData.TotalNodes;

        public int GetTotalEdges() => indexData.TotalEdges;

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
        public List<NodeInfo> Nodes { get; set; }
    }

    public class NodeInfo
    {
        public long NodeId { get; set; }
        public string FilePath { get; set; }
        public int EdgeCount { get; set; }
    }
}
