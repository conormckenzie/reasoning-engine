using System;
using System.IO;
using DebugUtils;
using DotNetEnv;

namespace ReasoningEngine.GraphFileHandling
{
    public static class OneTimeSetup
    {
        // Load environment variables from the .env file
        static OneTimeSetup()
        {
            Env.Load();
        }

        // Get the data folder path from environment variables, or throw an exception if not set
        private static readonly string baseDir = Environment.GetEnvironmentVariable("DATA_FOLDER_PATH") 
                                                 ?? throw new Exception("DATA_FOLDER_PATH is not set in the environment variables.");
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

            DebugWriter.DebugWriteLine("#00OTS1#", "One-time setup completed successfully.");
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
                DebugWriter.DebugWriteLine("#00OTS2#", $"Created directory: {path}");
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
                DebugWriter.DebugWriteLine("#00OTS3#", $"Created file: {path}");
            }
        }
    }
}
