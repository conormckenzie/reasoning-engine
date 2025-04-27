using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using DebugUtils;

namespace ReasoningEngine.GraphFileHandling
{
    /// <summary>
    /// Handles reading, writing, and updating the index.json files 
    /// used within the edge directory structure of FileGraphStorageProvider.
    /// </summary>
    public class EdgeIndexFileHandler
    {
        // Consider if baseDir is needed here, or if paths are always passed in.
        // For now, assume paths are passed in.

        /// <summary>
        /// Updates the appropriate index.json file to add or remove an edge file entry.
        /// </summary>
        /// <param name="edgeFilePath">The full path to the edge file being added or removed.</param>
        /// <param name="isAdding">True if adding the edge file entry, false if removing.</param>
        /// <returns>True if the index was updated successfully, false otherwise.</returns>
        public bool UpdateEdgeIndex(string edgeFilePath, bool isAdding)
        {
            try
            {
                // Changed to Detailed
                DebugWriter.DebugWriteLine("#4SZT2R#", $"Updating edge index for {edgeFilePath}, isAdding: {isAdding}", true, VerbosityLevel.Detailed); 
                string? directoryPath = Path.GetDirectoryName(edgeFilePath);
                if (string.IsNullOrEmpty(directoryPath))
                {
                    throw new InvalidOperationException("Unable to get directory path for edge file");
                }
                string indexFilePath = Path.Combine(directoryPath, "index.json");
                
                FilePathHelper.EnsureDirectoryExists(indexFilePath); // Use static helper

                IndexFile indexFile = LoadIndexFile(indexFilePath);
                 // Changed to Detailed
                DebugWriter.DebugWriteLine("#JM16CX#", $"Loaded index file: {indexFilePath}, current edge count: {indexFile.EdgeFiles.Count}", true, VerbosityLevel.Detailed);

                string edgeFileName = Path.GetFileName(edgeFilePath);

                if (isAdding)
                {
                    if (!indexFile.EdgeFiles.Contains(edgeFileName))
                    {
                        indexFile.EdgeFiles.Add(edgeFileName);
                         // Changed to Detailed
                        DebugWriter.DebugWriteLine("#21YE2B#", $"Added {edgeFileName} to index", true, VerbosityLevel.Detailed);
                    }
                    else
                    {
                         // Changed to Detailed
                        DebugWriter.DebugWriteLine("#C6B5G3#", $"{edgeFileName} already exists in index", true, VerbosityLevel.Detailed);
                    }
                }
                else
                {
                    indexFile.EdgeFiles.Remove(edgeFileName);
                     // Changed to Detailed
                    DebugWriter.DebugWriteLine("#50XXE9#", $"Removed {edgeFileName} from index", true, VerbosityLevel.Detailed);
                }

                SaveIndexFile(indexFilePath, indexFile);
                 // Changed to Detailed
                DebugWriter.DebugWriteLine("#0LU03E#", $"Saved updated index file: {indexFilePath}, new edge count: {indexFile.EdgeFiles.Count}", true, VerbosityLevel.Detailed);
                return true;
            }
            catch (Exception ex)
            {
                DebugWriter.DebugWriteLine("#00SAV8#", $"Error updating edge index: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Removes an edge file entry from the appropriate index.json file.
        /// </summary>
        /// <param name="edgeFilePath">The full path to the edge file whose entry should be removed.</param>
        public void RemoveEdgeFromIndex(string edgeFilePath)
        {
            // This method now directly calls UpdateEdgeIndex with isAdding = false
            string? indexFilePath = FilePathHelper.GetIndexFilePath(edgeFilePath); // Use static helper
            if (indexFilePath != null)
            {
                UpdateEdgeIndex(edgeFilePath, false);
            }
            else
            {
                 DebugWriter.DebugWriteLine("#REM_IDX_FAIL#", $"Could not determine index file path for edge file {edgeFilePath}. Cannot remove from index.", true, VerbosityLevel.Minimal);
            }
        }

        /// <summary>
        /// Retrieves a list of all edge file paths associated with a given node (either outgoing or incoming).
        /// Traverses the directory structure using index.json files.
        /// </summary>
        /// <param name="baseDir">The base data directory.</param>
        /// <param name="nodeId">The ID of the node.</param>
        /// <param name="outgoing">True to get outgoing edges, false for incoming.</param>
        /// <returns>A list of full paths to the edge files.</returns>
        public List<string> GetAllEdgeFiles(string baseDir, long nodeId, bool outgoing)
        {
            List<string> edgeFiles = new List<string>();
            string edgeDir = FilePathHelper.GetEdgeDirPath(baseDir, nodeId, outgoing); // Use static helper

             // Changed to Detailed
            DebugWriter.DebugWriteLine("#NLAIW7#", $"Getting all edge files for node {nodeId}, outgoing: {outgoing}", true, VerbosityLevel.Detailed);
             // Changed to Detailed
            DebugWriter.DebugWriteLine("#00LOD6#", $"Edge directory: {edgeDir}", true, VerbosityLevel.Detailed);

            if (!Directory.Exists(edgeDir))
            {
                 // Changed to Detailed
                DebugWriter.DebugWriteLine("#00LOD7#", $"Edge directory does not exist: {edgeDir}", true, VerbosityLevel.Detailed);
                return edgeFiles;
            }

            // Recursively search for index files
            SearchDirectoryForEdges(edgeDir, edgeFiles);

             // Changed to Detailed
            DebugWriter.DebugWriteLine("#QSI0XM#", $"Total edge files found: {edgeFiles.Count}", true, VerbosityLevel.Detailed);
            return edgeFiles;
        }

        // --- Private Helper Methods ---

        /// <summary>
        /// Loads and deserializes an IndexFile object from the specified path.
        /// Returns an empty IndexFile if the file doesn't exist.
        /// </summary>
        internal IndexFile LoadIndexFile(string indexFilePath) // Back to internal
        {
            if (File.Exists(indexFilePath))
            {
                try
                {
                    string json = File.ReadAllText(indexFilePath);
                    // Ensure System.Text.Json is used, add case-insensitive option
                    var optionsCI = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    return JsonSerializer.Deserialize<IndexFile>(json, optionsCI) ?? new IndexFile(); 
                }
                catch (Exception ex)
                {
                    DebugWriter.DebugWriteLine("#LOAD_IDX_ERR#", $"Error loading index file {indexFilePath}: {ex.Message}. Returning empty index.", true, VerbosityLevel.Minimal);
                    return new IndexFile(); // Return empty on error
                }
            }
            else
            {
                return new IndexFile();
            }
        }

        /// <summary>
        /// Serializes and saves an IndexFile object to the specified path.
        /// </summary>
        private void SaveIndexFile(string indexFilePath, IndexFile indexFile)
        {
             try
            {
                var options = new JsonSerializerOptions { WriteIndented = true }; // Options for System.Text.Json
                // Ensure System.Text.Json is used
                string json = JsonSerializer.Serialize(indexFile, options); 
                File.WriteAllText(indexFilePath, json);
            }
            catch (Exception ex)
            {
                 DebugWriter.DebugWriteLine("#SAVE_IDX_ERR#", $"Error saving index file {indexFilePath}: {ex.Message}", true, VerbosityLevel.Minimal);
                 // Decide if re-throwing is appropriate or if failure should be handled differently
                 throw; 
            }
        }

        /// <summary>
        /// Recursively searches directories starting from the given path, reads index.json files,
        /// and adds the listed edge file paths (relative to the index file's directory) to the provided list.
        /// </summary>
        private void SearchDirectoryForEdges(string directory, List<string> edgeFiles)
        {
            string indexFilePath = Path.Combine(directory, "index.json");
             // Changed to Detailed
            DebugWriter.DebugWriteLine("#00LOD8#", $"Checking index file: {indexFilePath}", true, VerbosityLevel.Detailed);

            if (File.Exists(indexFilePath))
            {
                IndexFile indexFile = LoadIndexFile(indexFilePath);

                // Add edge files
                foreach (var fileName in indexFile.EdgeFiles)
                {
                    string fullPath = Path.Combine(directory, fileName);
                    edgeFiles.Add(fullPath);
                     // Changed to Detailed
                    DebugWriter.DebugWriteLine("#00LOD9#", $"Added edge file: {fullPath}", true, VerbosityLevel.Detailed);
                }
            }
            else
            {
                 // Changed to Detailed
                DebugWriter.DebugWriteLine("#BCWITX#", $"Index file not found: {indexFilePath}", true, VerbosityLevel.Detailed);
            }

            // Recursively search subdirectories
            try
            {
                foreach (var subDir in Directory.GetDirectories(directory))
                {
                    SearchDirectoryForEdges(subDir, edgeFiles);
                }
            }
            catch (Exception ex)
            {
                 DebugWriter.DebugWriteLine("#SEARCH_SUBDIR_ERR#", $"Error searching subdirectories of {directory}: {ex.Message}", true, VerbosityLevel.Minimal);
                 // Continue searching other branches if possible
            }
        }

        /// <summary>
        /// Represents the structure of the index.json file within edge directories.
        /// </summary>
        public class IndexFile // Made public for test access
        {
            // Subdirectories property seems unused based on current FileGraphStorageProvider logic, omitting for now.
            // public List<string> Subdirectories { get; set; } = new List<string>(); 
            public List<string> EdgeFiles { get; set; } = new List<string>();
        }
    }
}
