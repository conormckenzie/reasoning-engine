namespace ReasoningEngine
{
    /// <summary>
    /// Represents an entity that has a version.
    /// Used for tracking the structure version of core components like Nodes and Edges.
    /// </summary>
    public interface IVersioned
    {
        /// <summary>
        /// Gets the version number of the entity's structure.
        /// </summary>
        int Version { get; }
    }
}
