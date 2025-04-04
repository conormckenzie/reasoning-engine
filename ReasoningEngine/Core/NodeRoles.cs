namespace ReasoningEngine
{
    /// <summary>
    /// Defines the functional role of a node in the graph.
    /// </summary>
    public enum NodeRole
    {
        /// <summary>
        /// Represents a variable with an associated probability distribution.
        /// </summary>
        Variable,

        /// <summary>
        /// Represents a deterministic or stochastic function/operation.
        /// </summary>
        Function,

        // Future roles could include:
        // Constant, // Represents a fixed value input
        // Evidence // Represents observed data fixed to a variable node
    }
}
