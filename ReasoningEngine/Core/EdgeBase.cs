using Newtonsoft.Json; // Added for JsonIgnore
using System;
using System.Collections.Generic;

namespace ReasoningEngine
{
    public abstract class EdgeBase : IVersioned
    {
        // Setters protected
        public Guid EdgeId { get; protected set; } 
        public long FromNode { get; protected set; }
        public long ToNode { get; protected set; }
        [JsonIgnore] // Ignore Version during serialization/deserialization
        public abstract int Version { get; }
        // Property public, setter protected is correct.
        public Dictionary<string, object> ExtendedProperties { get; protected set; } = new Dictionary<string, object>(); 

        // Constructor now generates a Guid
        // Made public (as in commit b7611d3)
        public EdgeBase(long fromNode, long toNode) 
        {
            EdgeId = Guid.NewGuid(); // Generate new ID
            FromNode = fromNode;
            ToNode = toNode;
        }

        // Constructor overload to accept an existing Guid (e.g., during loading)
        // Made public (as in commit b7611d3)
        public EdgeBase(Guid edgeId, long fromNode, long toNode)
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
