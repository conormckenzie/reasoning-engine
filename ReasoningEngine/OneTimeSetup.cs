using System;
using System.IO;
using DebugUtils;

namespace ReasoningEngine.GraphFileHandling
{
    public static class OneTimeSetup
    {
        // Define the path for the main data folder
        private static readonly string baseDir = "data";
        private static readonly string indexFilePath = Path.Combine(baseDir, "index.json");

        /// <summary>
        /// Performs the one-time setup tasks.
        /// </summary>
        public static void Initialize()
        {
            // Create the base data directory if it does not exist
            CreateDirectoryIfNotExists(baseDir);

            // Create the index file if it does not exist
            CreateFileIfNotExists(indexFilePath, "{}");

            DebugWriter.DebugWriteLine("#OTS1#", "One-time setup completed successfully.");
        }

        /// <summary>
        /// Creates a directory if it does not exist.
        /// </summary>
        /// <param name="path">The path of the directory to create.</param>
        private static void CreateDirectoryIfNotExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                DebugWriter.DebugWriteLine("#OTS2#", $"Created directory: {path}");
            }
        }

        /// <summary>
        /// Creates a file with the specified content if it does not exist.
        /// </summary>
        /// <param name="path">The path of the file to create.</param>
        /// <param name="content">The content to write to the file if it does not exist.</param>
        private static void CreateFileIfNotExists(string path, string content)
        {
            if (!File.Exists(path))
            {
                File.WriteAllText(path, content);
                DebugWriter.DebugWriteLine("#OTS3#", $"Created file: {path}");
            }
        }
    }
}
