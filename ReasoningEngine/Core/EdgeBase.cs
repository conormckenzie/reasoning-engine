using System;
using System.Collections.Generic;
using System.Text.Json; // Needed for JsonElement
using System.Text.Json.Serialization;

namespace ReasoningEngine
{
    /// <summary>
    /// Abstract base class for all edge types in the reasoning graph.
    /// Provides common properties and methods for edges.
    /// </summary>
    public abstract class EdgeBase : IVersioned
    {
        /// <summary>
        /// Gets the unique identifier for the edge.
        /// </summary>
        public Guid EdgeId { get; protected set; }
        
        /// <summary>
        /// Gets the ID of the source node for this edge.
        /// </summary>
        public long FromNode { get; protected set; }
        
        /// <summary>
        /// Gets the ID of the destination node for this edge.
        /// </summary>
        public long ToNode { get; protected set; }
        
        /// <summary>
        /// Gets the version of the edge structure.
        /// </summary>
        [JsonIgnore] // Ignore Version during serialization/deserialization (Now using System.Text.Json)
        public abstract int Version { get; }
        
        /// <summary>
        /// Gets or sets a dictionary for storing arbitrary additional metadata for the edge.
        /// </summary>
        // Property public, setter protected is correct.
        [JsonInclude] // Allow System.Text.Json to deserialize into this property despite protected setter
        public Dictionary<string, object> ExtendedProperties { get; protected set; } = new Dictionary<string, object>();

        /// <summary>
        /// Initializes a new instance of the EdgeBase class with a newly generated unique identifier.
        /// </summary>
        /// <param name="fromNode">The ID of the source node.</param>
        /// <param name="toNode">The ID of the destination node.</param>
        // Constructor now generates a Guid
        // Made public (as in commit b7611d3)
        public EdgeBase(long fromNode, long toNode)
        {
            EdgeId = Guid.NewGuid(); // Generate new ID
            FromNode = fromNode;
            ToNode = toNode;
        }

        /// <summary>
        /// Initializes a new instance of the EdgeBase class with a specified unique identifier (used during loading).
        /// </summary>
        /// <param name="edgeId">The unique identifier for the edge.</param>
        /// <param name="fromNode">The ID of the source node.</param>
        /// <param name="toNode">The ID of the destination node.</param>
        // Constructor overload to accept an existing Guid (e.g., during loading)
        // Made public (as in commit b7611d3)
        public EdgeBase(Guid edgeId, long fromNode, long toNode)
        {
            EdgeId = edgeId;
            FromNode = fromNode;
            ToNode = toNode;
        }

        /// <summary>
        /// Abstract method to upgrade this edge instance to the latest version.
        /// Concrete implementations must provide the logic for upgrading.
        /// </summary>
        /// <returns>A new EdgeBase instance representing the upgraded version.</returns>
        // Keep UpgradeToLatest for future edge versions
        public abstract EdgeBase UpgradeToLatest();

        /// <summary>
        /// Sets or updates an extended property for the edge.
        /// </summary>
        /// <param name="key">The key of the property.</param>
        /// <param name="value">The value of the property.</param>
        public void SetExtendedProperty(string key, object value)
        {
            ExtendedProperties[key] = value;
        }

        /// <summary>
        /// Gets an extended property by key, attempting to cast or deserialize it to the specified type.
        /// Handles cases where the value might be a JsonElement after deserialization.
        /// </summary>
        /// <typeparam name="T">The target type for the property value.</typeparam>
        /// <param name="key">The key of the property.</param>
        /// <returns>The property value cast or deserialized to the specified type.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if the property key is not found.</exception>
        /// <exception cref="InvalidCastException">Thrown if the property value cannot be converted to the specified type.</exception>
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
