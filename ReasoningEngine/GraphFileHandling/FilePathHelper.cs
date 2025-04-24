using System;
using System.IO;
using System.Text;

namespace ReasoningEngine.GraphFileHandling
{
    /// <summary>
    /// Provides static helper methods for generating file and directory paths 
    /// used by the FileGraphStorageProvider.
    /// </summary>
    public static class FilePathHelper
    {
        /// <summary>
        /// Generates the full hierarchical file path for a given node ID.
        /// Example: {baseDir}/1234/12345678/123456789012/1234567890123456.json
        /// </summary>
        public static string GetNodeFilePath(string baseDir, long nodeId)
        {
            string nodeIdStr = nodeId.ToString("D16"); // Ensure consistent padding
            string[] pathSegments = new string[5];
            pathSegments[0] = baseDir;
            pathSegments[1] = nodeIdStr.Substring(0, 4);
            pathSegments[2] = nodeIdStr.Substring(0, 8);
            pathSegments[3] = nodeIdStr.Substring(0, 12);
            pathSegments[4] = nodeIdStr + ".json";
            return Path.Combine(pathSegments);
        }

        /// <summary>
        /// Generates the full hierarchical file path for an edge, either for its outgoing or incoming representation.
        /// Example (outgoing): {baseDir}/edges/outgoing/1111/11112222/111122223333/1111222233334444/1111...-9999/1111...-99998888/.../1111...-9999....json
        /// Example (incoming): {baseDir}/edges/incoming/9999/99998888/999988887777/9999888877776666/9999...-1111/9999...-11112222/.../9999...-1111....json (Note: filename uses dest-source order)
        /// </summary>
        /// <param name="baseDir">The base data directory.</param>
        /// <param name="primaryNodeId">The source node ID if outgoing=true, destination node ID if outgoing=false.</param>
        /// <param name="secondaryNodeId">The destination node ID if outgoing=true, source node ID if outgoing=false.</param>
        /// <param name="outgoing">True to generate the outgoing path, false for the incoming path.</param>
        /// <returns>The full file path for the edge representation.</returns>
        public static string GetEdgeFilePath(string baseDir, long primaryNodeId, long secondaryNodeId, bool outgoing)
        {
            string direction = outgoing ? "outgoing" : "incoming";
            string primaryNodeIdStr = primaryNodeId.ToString("D16");
            string secondaryNodeIdStr = secondaryNodeId.ToString("D16");

            List<string> pathSegments = new List<string>
            {
                baseDir,
                "edges",
                direction,
                primaryNodeIdStr.Substring(0, 4),
                primaryNodeIdStr.Substring(0, 8),
                primaryNodeIdStr.Substring(0, 12),
                primaryNodeIdStr
            };

            // Include SecondaryNodeId segments to further split directories
            // The directory name includes both IDs for clarity within the hierarchy
            pathSegments.Add($"{primaryNodeIdStr}-{secondaryNodeIdStr.Substring(0, 4)}");
            pathSegments.Add($"{primaryNodeIdStr}-{secondaryNodeIdStr.Substring(0, 8)}");
            pathSegments.Add($"{primaryNodeIdStr}-{secondaryNodeIdStr.Substring(0, 12)}");

            // Filename convention depends on direction
            string fileName = outgoing 
                ? $"{primaryNodeIdStr}-{secondaryNodeIdStr}.json" 
                : $"{primaryNodeIdStr}-{secondaryNodeIdStr}.json"; // Note: In FileGraphStorageProvider, incoming used dest-source filename. Standardizing here to primary-secondary. The caller needs to handle this difference if necessary. Let's stick to primary-secondary for consistency in the helper.
                // *** Correction: The original code DID use dest-source for incoming filename. Let's preserve that specific behavior. ***
            fileName = outgoing
                ? $"{primaryNodeIdStr}-{secondaryNodeIdStr}.json" // source-dest
                : $"{primaryNodeIdStr}-{secondaryNodeIdStr}.json"; // dest-source (primary is dest when outgoing=false)

            pathSegments.Add(fileName);

            string finalPath = Path.Combine(pathSegments.ToArray());
            // Optional: Logging could be added here if needed, but better kept in the caller (FileGraphStorageProvider)
            // DebugWriter.DebugWriteLine("#GET_EDGE_PATH#", $"Generated edge path ({direction}): {finalPath}", true, VerbosityLevel.Detailed); 
            return finalPath;
        }


        /// <summary>
        /// Generates the path to the top-level directory containing edges related to a specific node ID (either outgoing or incoming).
        /// Example: {baseDir}/edges/outgoing/1111/11112222/111122223333/1111222233334444/
        /// </summary>
        public static string GetEdgeDirPath(string baseDir, long nodeId, bool outgoing)
        {
            string direction = outgoing ? "outgoing" : "incoming";
            string nodeIdStr = nodeId.ToString("D16");
            string[] pathSegments = new string[7];
            pathSegments[0] = baseDir;
            pathSegments[1] = "edges";
            pathSegments[2] = direction;
            pathSegments[3] = nodeIdStr.Substring(0, 4);
            pathSegments[4] = nodeIdStr.Substring(0, 8);
            pathSegments[5] = nodeIdStr.Substring(0, 12);
            pathSegments[6] = nodeIdStr; // The directory named after the full node ID
            return Path.Combine(pathSegments);
        }

        /// <summary>
        /// Gets the expected path for the index.json file within the same directory as a given edge file path.
        /// </summary>
        /// <param name="edgeFilePath">The full path to an edge file.</param>
        /// <returns>The full path to the corresponding index.json file, or null if the directory path cannot be determined.</returns>
        public static string? GetIndexFilePath(string edgeFilePath)
        {
            string? directoryPath = Path.GetDirectoryName(edgeFilePath);
            if (directoryPath == null)
            {
                return null;
            }
            return Path.Combine(directoryPath, "index.json");
        }

        /// <summary>
        /// Ensures that the directory containing the specified file path exists.
        /// If the directory does not exist, it is created.
        /// </summary>
        /// <param name="filePath">The full path to a file.</param>
        public static void EnsureDirectoryExists(string filePath)
        {
            string? directoryPath = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }
    }
}
