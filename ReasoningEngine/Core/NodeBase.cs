using System;
using System.Collections.Generic;
using System.Text.Json; // Needed for JsonElement
using System.Text.Json.Serialization;

namespace ReasoningEngine
{
    // NodeType enum removed as NodeRole replaces its purpose in V3+

    public abstract class NodeBase : IVersioned
    {
        // Setter protected
        public long Id { get; protected set; } 
        [JsonIgnore] // Ignore Version during serialization/deserialization (Now using System.Text.Json)
        public abstract int Version { get; }
        // public NodeType Type { get; protected set; } // Removed
        public string Content { get; set; } // Kept Content here (public set OK)
        // Property public, setter protected is correct.
        [JsonInclude] // Allow System.Text.Json to deserialize into this property despite protected setter
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
            if (!ExtendedProperties.TryGetValue(key, out var value))
            {
                throw new KeyNotFoundException($"Property '{key}' not found.");
            }

            if (value is T typedValue)
            {
                return typedValue; // Already the correct type
            }

            // Handle case where value might be a JsonElement after deserialization
            if (value is JsonElement element)
            {
                try
                {
                    // Attempt to deserialize the JsonElement to the requested type T
                    T? deserializedValue = element.Deserialize<T>(); 
                    if (deserializedValue != null)
                    {
                        // Optional: Replace the JsonElement in the dictionary with the actual type 
                        // for future accesses, though this modifies state during a get operation.
                        // ExtendedProperties[key] = deserializedValue; 
                        return deserializedValue;
                    }
                    else
                    {
                         // Handle cases where Deserialize returns null for reference types if that's unexpected
                         throw new InvalidCastException($"Deserialization of JsonElement for key '{key}' to type {typeof(T)} resulted in null.");
                    }
                }
                catch (JsonException ex)
                {
                    throw new InvalidCastException($"Failed to deserialize property '{key}' from JsonElement to type {typeof(T)}. Error: {ex.Message}", ex);
                }
                catch (Exception ex) // Catch other potential exceptions during deserialization
                {
                     throw new InvalidCastException($"Unexpected error deserializing property '{key}' from JsonElement to type {typeof(T)}. Error: {ex.Message}", ex);
                }
            }

            // If the value is not T and not a JsonElement we can convert, it's the wrong type.
            throw new InvalidCastException($"Property '{key}' is of type {value?.GetType().Name ?? "null"} but type {typeof(T)} was requested.");
        }
    }
}
