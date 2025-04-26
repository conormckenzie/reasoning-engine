using System;

namespace ReasoningEngine
{
    /// <summary>
    /// Specifies the minimum required versions of Node and Edge structures that a class or method is compatible with.
    /// Used to manage algorithm compatibility with different data versions.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public class VersionCompatibilityAttribute : Attribute
    {
        /// <summary>
        /// Gets the minimum required Node version.
        /// </summary>
        public int MinimumNodeVersion { get; }
        
        /// <summary>
        /// Gets the minimum required Edge version.
        /// </summary>
        public int MinimumEdgeVersion { get; }

        /// <summary>
        /// Initializes a new instance of the VersionCompatibilityAttribute.
        /// </summary>
        /// <param name="minimumNodeVersion">The minimum required Node version.</param>
        /// <param name="minimumEdgeVersion">The minimum required Edge version.</param>
        public VersionCompatibilityAttribute(int minimumNodeVersion, int minimumEdgeVersion)
        {
            MinimumNodeVersion = minimumNodeVersion;
            MinimumEdgeVersion = minimumEdgeVersion;
        }
    }
}
