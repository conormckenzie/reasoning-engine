using System;

namespace ReasoningEngine
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public class VersionCompatibilityAttribute : Attribute
    {
        public int MinimumNodeVersion { get; }
        public int MinimumEdgeVersion { get; }

        public VersionCompatibilityAttribute(int minimumNodeVersion, int minimumEdgeVersion)
        {
            MinimumNodeVersion = minimumNodeVersion;
            MinimumEdgeVersion = minimumEdgeVersion;
        }
    }
}