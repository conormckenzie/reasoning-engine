using System;
using System.Collections.Generic;

namespace ReasoningEngine
{
    public enum NodeType
    {
        Standard,
        SIMO,
        MISO
    }

    public abstract class NodeBase : IVersioned
    {
        public long Id { get; protected set; }
        public abstract int Version { get; }
        public NodeType Type { get; protected set; }
        public string Content { get; set; }
        protected Dictionary<string, object> ExtendedProperties { get; } = new Dictionary<string, object>();

        protected NodeBase(long id, NodeType type)
        {
            Id = id;
            Type = type;
        }

        public abstract NodeBase UpgradeToLatest();

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