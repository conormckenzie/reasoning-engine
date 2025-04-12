using Newtonsoft.Json; // Added for JsonIgnore
using System;
using System.Collections.Generic;

namespace ReasoningEngine
{
    // NodeType enum removed as NodeRole replaces its purpose in V3+

    public abstract class NodeBase : IVersioned
    {
        // Setter protected
        public long Id { get; protected set; } 
        [JsonIgnore] // Ignore Version during serialization/deserialization
        public abstract int Version { get; }
        // public NodeType Type { get; protected set; } // Removed
        public string Content { get; set; } // Kept Content here (public set OK)
        // Property public, setter protected is correct.
        public Dictionary<string, object> ExtendedProperties { get; protected set; } = new Dictionary<string, object>(); 

        // Constructor made public (as in commit b7611d3)
        public NodeBase(long id) 
        {
            Id = id;
            Content = string.Empty; // Initialize Content
            // Type = type; // Removed type assignment
        }

        // Removed abstract UpgradeToLatest as V1/V2 are removed and V3 handles it
        // public abstract NodeBase UpgradeToLatest(); 

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
