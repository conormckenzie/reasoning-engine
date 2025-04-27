namespace ReasoningEngine
{
    // DomainInterpretation enum removed as it was unused (2025-04-12 Review)

    /// <summary>
    /// Defines the possible domain types for a Variable node's probability distribution.
    /// </summary>
    public enum DomainType
    {
        /// <summary>
        /// Represents a continuous range of real numbers.
        /// </summary>
        Continuous,
        /// <summary>
        /// Represents a set of specific integer values.
        /// </summary>
        DiscreteInteger,
        /// <summary>
        /// Represents a probability value itself, typically within the range [0, 1].
        /// </summary>
        Truth
    }
}
