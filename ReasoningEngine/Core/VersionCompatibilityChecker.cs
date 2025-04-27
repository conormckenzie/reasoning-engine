using System;
using System.Reflection;

namespace ReasoningEngine
{
    /// <summary>
    /// Provides utility methods for checking version compatibility between versioned elements (Nodes, Edges)
    /// and methods or classes decorated with the VersionCompatibilityAttribute.
    /// </summary>
    public static class VersionCompatibilityChecker
    {
        /// <summary>
        /// Checks if a versioned element (Node or Edge) is compatible with a method based on its VersionCompatibilityAttribute.
        /// </summary>
        /// <param name="element">The versioned element (Node or Edge) to check.</param>
        /// <param name="method">The method to check compatibility against.</param>
        /// <returns>True if the element's version meets or exceeds the minimum required versions specified by the attribute, or if the method has no VersionCompatibilityAttribute; otherwise, false.</returns>
        /// <exception cref="ArgumentException">Thrown if the element is not a known versioned type (NodeBase or EdgeBase).</exception>
        public static bool IsCompatible(IVersioned element, MethodInfo method)
        {
            var attr = method.GetCustomAttribute<VersionCompatibilityAttribute>();
            if (attr == null) return true; // If no attribute, assume compatible with all versions

            if (element is NodeBase)
                return element.Version >= attr.MinimumNodeVersion;
            else if (element is EdgeBase)
                return element.Version >= attr.MinimumEdgeVersion;

            throw new ArgumentException("Unknown element type", nameof(element));
        }
    }
}
