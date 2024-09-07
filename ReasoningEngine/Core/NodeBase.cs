using System;
using System.Collections.Generic;

namespace ReasoningEngine
{
    public abstract class NodeBase : IVersioned
    {
        public long Id { get; protected set; }
        public abstract int Version { get; }
        protected Dictionary<string, object> ExtendedProperties { get; } = new Dictionary<string, object>();

        protected NodeBase(long id)
        {
            Id = id;
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