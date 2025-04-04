using System;
using System.Collections.Generic;

namespace ReasoningEngine
{
    public abstract class EdgeBase : IVersioned
    {
        public Guid EdgeId { get; protected set; } // Added EdgeId
        public long FromNode { get; protected set; }
        public long ToNode { get; protected set; }
        public abstract int Version { get; }
        protected Dictionary<string, object> ExtendedProperties { get; } = new Dictionary<string, object>();

        // Constructor now generates a Guid
        protected EdgeBase(long fromNode, long toNode) 
        {
            EdgeId = Guid.NewGuid(); // Generate new ID
            FromNode = fromNode;
            ToNode = toNode;
        }

        // Constructor overload to accept an existing Guid (e.g., during loading)
        protected EdgeBase(Guid edgeId, long fromNode, long toNode)
        {
            EdgeId = edgeId;
            FromNode = fromNode;
            ToNode = toNode;
        }


        // Keep UpgradeToLatest for future edge versions
        public abstract EdgeBase UpgradeToLatest(); 

        public void SetExtendedProperty(string key, object value)
        {
            ExtendedProperties[key] = value;
        }

        public T GetExtendedProperty<T>(string key)
        {
            if (ExtendedProperties.TryGetValue(key, out var value) && value is T typedValue)
                return typedValue;
            throw new KeyNotFoundException($"Property '{key}' not found or not of type {typeof(T)}");
        }
    }
}
