using System;
using System.Collections.Generic;
using DotNetEnv;
using GraphFileHandling;
using Newtonsoft.Json;
using DebugUtils;

class Program
{
    // List to store nodes
    public static List<Node> nodes = new List<Node>();
    
    // List to store edges
    public static List<Edge> edges = new List<Edge>();

    // Lists to track changes in nodes and edges
    public static List<Node> nodeChanges = new List<Node>();
    public static List<Edge> edgeChanges = new List<Edge>();

    static void Main(string[] args)
    {
        // Load environment variables from the .env file
        Env.Load();

        // Get the data folder path from environment variables, or throw an exception if not set
        string dataFolderPath = Environment.GetEnvironmentVariable("DATA_FOLDER_PATH") 
                                 ?? throw new Exception("DATA_FOLDER_PATH is not set in the environment variables.");

        // Create an instance of GraphFileManager with the data folder path
        var manager = new GraphFileManager(dataFolderPath);

        // Display the menu and handle user input
        ShowMenu(manager);
    }

    /// <summary>
    /// Displays the menu and handles user input.
    /// </summary>
    /// <param name="manager">The GraphFileManager instance to handle save/load operations.</param>
    static void ShowMenu(GraphFileManager manager)
    {
        while (true)
        {
            // Display menu options
            DebugWriter.DebugWriteLine("#D7D1#", "Select an option:");
            DebugWriter.DebugWriteLine("#D7D2#", "1. Save Node");
            DebugWriter.DebugWriteLine("#D7D3#", "2. Load Node");
            DebugWriter.DebugWriteLine("#D7D4#", "3. Set Debug Mode");
            DebugWriter.DebugWriteLine("#D7D5#", "4. Exit");
            DebugWriter.DebugWrite("#D7D6#", "Enter option: ");

            // Read user input
            var option = Console.ReadLine();

            // Handle user input
            switch (option)
            {
                case "1":
                    manager.SaveNodeWithUserInput();
                    break;
                case "2":
                    manager.LoadNodeWithUserInput();
                    break;
                case "3":
                    DebugOptions.SetDebugMode();
                    break;
                case "4":
                    return; // Exit the loop and end the program
                default:
                    DebugWriter.DebugWriteLine("#D7D7#", "Invalid option. Please try again.");
                    break;
            }
        }
    }
}


// ============================================================================
//                               TODO LIST
// ============================================================================
// ** THIS SECTION IS SPECIFIC TO CONTRIBUTOR: CONOR ONLY **
// ** OTHER CONTRIBUTORS CAN IGNORE THIS SECTION **

/* TODO:

(1,-1) Test functionality so far
(1,-0,1) Fix GraphFileManager.cs, implement functions
(1,-0,2) Move Index class outside of IndexManager since it's a structural spec, effectively
(1,-0,3) Change Nodedata class in IndexManager to not duplicate the structure of a Node
(1) Test Save/Load function a bit better

(2,-1) Figure out if using a certain method of tracking new & changed nodes & edges introduces limitations on how I can update the graph
(2) Track new nodes; edges
(2,1) Track changed nodes; edges
(3) Append new nodes & edges to file
(4) Print all nodes, edges to console
(5) Make sure the program can cleanly handle learge numbers of nodes / edges 
    - files (definitely will need a fix; no clear way to split the edges of a node between multiple files if #edgesPerNode = O(#nodes))
    - storing values; local variables
    - passing values between functions

TO IMPORT TO CLICKUP:
    Future items:
        - Improve file structure to not wastefully overwrite complete file each time an update is made
*/