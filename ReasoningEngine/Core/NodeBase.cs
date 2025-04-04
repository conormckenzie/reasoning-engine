using System;
using System; // Added for KeyNotFoundException if not already present
using System.Collections.Generic;

namespace ReasoningEngine
{
    // NodeType enum removed as NodeRole replaces its purpose in V3+

    public abstract class NodeBase : IVersioned
    {
        public long Id { get; protected set; }
        public abstract int Version { get; }
        // public NodeType Type { get; protected set; } // Removed
        public string Content { get; set; } // Kept Content here
        protected Dictionary<string, object> ExtendedProperties { get; } = new Dictionary<string, object>();

        // Updated constructor - no longer takes NodeType
        protected NodeBase(long id) 
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
