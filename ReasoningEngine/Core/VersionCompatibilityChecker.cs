using System;
using System.Reflection;

namespace ReasoningEngine
{
    public static class VersionCompatibilityChecker
    {
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