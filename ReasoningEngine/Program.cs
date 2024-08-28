using System;
using System.Collections.Generic;
using DotNetEnv;
using ReasoningEngine.GraphFileHandling;
using ReasoningEngine.GraphAccess; // Added namespace for CommandProcessor
using Newtonsoft.Json;
using DebugUtils;

namespace ReasoningEngine
{
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

            // Create an instance of CommandProcessor with the GraphFileManager
            var commandProcessor = new CommandProcessor(manager); // Added CommandProcessor instance

            // Display the menu and handle user input
            ShowMenu(manager, commandProcessor); // Pass commandProcessor to ShowMenu
        }

        /// <summary>
        /// Displays the menu and handles user input.
        /// </summary>
        /// <param name="manager">The GraphFileManager instance to handle save/load operations.</param>
        /// <param name="commandProcessor">The CommandProcessor instance to handle command processing.</param>
        static void ShowMenu(GraphFileManager manager, CommandProcessor commandProcessor)
        {
            while (true)
            {
                // Display menu options
                DebugWriter.DebugWriteLine("#00D7D1#", "\nMain Menu:");
                DebugWriter.DebugWriteLine("#00D7D2#", "1. Run Setup");
                DebugWriter.DebugWriteLine("#00D7D3#", "2. Save Node");
                DebugWriter.DebugWriteLine("#00D7D4#", "3. Load Node");
                DebugWriter.DebugWriteLine("#00D7D5#", "4. Command Processor Options"); // Updated option for Command Processor
                DebugWriter.DebugWriteLine("#00D7D6#", "5. Debug Options");
                DebugWriter.DebugWriteLine("#00D7D7#", "6. Exit"); // Updated to reflect added Command Processor option
                DebugWriter.DebugWrite("#00D7D8#", "Enter option: "); // Updated to next unique code
                // TODO: Integrate these menu options
                // DebugWriter.DebugWriteLine("#D7D1#", "Select an option:");
                // DebugWriter.DebugWriteLine("#D7D2#", "1. Save Node");
                // DebugWriter.DebugWriteLine("#D7D3#", "2. Load Node");
                // DebugWriter.DebugWriteLine("#D7D4#", "3. Save Edge");
                // DebugWriter.DebugWriteLine("#D7D5#", "4. Load Edges");
                // DebugWriter.DebugWriteLine("#D7D6#", "5. View Nodes and Edges");
                // DebugWriter.DebugWriteLine("#D7D7#", "6. Set Debug Mode");
                // DebugWriter.DebugWriteLine("#D7D8#", "7. Exit");
                // DebugWriter.DebugWrite("#D7D9#", "Enter option: ");

                var option = Console.ReadLine();

                switch (option)
                {
                    case "1":
                        OneTimeSetup.Initialize();
                        break;
                    case "2":
                        // manager.SaveNodeWithUserInput(); // Keeping it commented out
                        DebugWriter.DebugWriteLine("#00SOR1#", "Sorry, this has been disabled for now");
                        break;
                    case "3":
                        // manager.LoadNodeWithUserInput(); // Keeping it commented out
                        DebugWriter.DebugWriteLine("#00SOR2#", "Sorry, this has been disabled for now");
                        break;
                    case "4":
                        commandProcessor.ShowCommandProcessorMenu(); // Added call to show Command Processor menu
                        break;
                    case "5":
                        DebugOptions.ShowDebugOptionsMenu();
                        break;
                    case "6":
                        return; // Exit the loop and end the program
                    // TODO: Integrate these menu options
                    // case "1a":
                    //     SaveNodeWithUserInput(manager);
                    //     break;
                    // case "2a":
                    //     LoadNodeWithUserInput(manager);
                    //     break;
                    // case "3a":
                    //     SaveEdgeWithUserInput(manager);
                    //     break;
                    // case "4a":
                    //     LoadEdgesWithUserInput(manager);
                    //     break;
                    // case "5a":
                    //     ViewNodesAndEdges(manager);
                    //     break;
                    // case "6a":
                    //     DebugOptions.SetDebugMode();
                    //     break;
                    default:
                        DebugWriter.DebugWriteLine("#00INV1#", "Invalid option. Please try again."); // Updated to next unique code
                        break;
                }
            }
        }

        static void SaveNodeWithUserInput(GraphFileManager manager)
        {
            Console.Write("Enter node ID: ");
            if (!long.TryParse(Console.ReadLine(), out long nodeId))
            {
                DebugWriter.DebugWriteLine("#SNE1#", "Invalid node ID. Please enter a valid integer.");
                return;
            }

            Console.Write("Enter node content: ");
            string? nodeContent = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(nodeContent))
            {
                DebugWriter.DebugWriteLine("#SNE2#", "Node content cannot be empty.");
                return;
            }

            bool success = manager.SaveNode(nodeId, nodeContent);
            if (success)
            {
                DebugWriter.DebugWriteLine("#SNS1#", $"Node {nodeId} saved successfully.");
            }
            else
            {
                DebugWriter.DebugWriteLine("#SNE3#", $"Failed to save node {nodeId}.");
            }
        }

        static void LoadNodeWithUserInput(GraphFileManager manager)
        {
            Console.Write("Enter node ID to load: ");
            if (!long.TryParse(Console.ReadLine(), out long nodeId))
            {
                DebugWriter.DebugWriteLine("#LNE1#", "Invalid node ID. Please enter a valid integer.");
                return;
            }

            Node? node = manager.LoadNode(nodeId);
            if (node != null)
            {
                DebugWriter.DebugWriteLine("#LNS1#", $"Node {nodeId} loaded successfully.");
                DebugWriter.DebugWriteLine("#LNS2#", $"Content: {node.Content}");
            }
            else
            {
                DebugWriter.DebugWriteLine("#LNE2#", $"Failed to load node {nodeId}. The node may not exist.");
            }
        }

        static void SaveEdgeWithUserInput(GraphFileManager manager)
        {
            Console.Write("Enter source node ID: ");
            if (!long.TryParse(Console.ReadLine(), out long fromNodeId))
            {
                DebugWriter.DebugWriteLine("#SEE1#", "Invalid source node ID. Please enter a valid integer.");
                return;
            }

            Console.Write("Enter destination node ID: ");
            if (!long.TryParse(Console.ReadLine(), out long toNodeId))
            {
                DebugWriter.DebugWriteLine("#SEE2#", "Invalid destination node ID. Please enter a valid integer.");
                return;
            }

            Console.Write("Enter edge weight: ");
            if (!double.TryParse(Console.ReadLine(), out double weight))
            {
                DebugWriter.DebugWriteLine("#SEE3#", "Invalid weight. Please enter a valid number.");
                return;
            }

            Console.Write("Enter edge content: ");
            string? edgeContent = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(edgeContent))
            {
                DebugWriter.DebugWriteLine("#SEE4#", "Edge content cannot be empty.");
                return;
            }

            var edge = new Edge(fromNodeId, toNodeId, weight, edgeContent);
            bool success = manager.SaveEdge(edge);
            if (success)
            {
                DebugWriter.DebugWriteLine("#SES1#", $"Edge from {fromNodeId} to {toNodeId} saved successfully.");
            }
            else
            {
                DebugWriter.DebugWriteLine("#SEE5#", $"Failed to save edge from {fromNodeId} to {toNodeId}.");
            }
        }

        static void LoadEdgesWithUserInput(GraphFileManager manager)
        {
            Console.Write("Enter node ID to load edges for: ");
            if (!long.TryParse(Console.ReadLine(), out long nodeId))
            {
                DebugWriter.DebugWriteLine("#LEE1#", "Invalid node ID. Please enter a valid integer.");
                return;
            }

            List<Edge> edges = manager.LoadEdges(nodeId);
            if (edges.Count > 0)
            {
                DebugWriter.DebugWriteLine("#LES1#", $"Edges for node {nodeId}:");
                foreach (var edge in edges)
                {
                    DebugWriter.DebugWriteLine("#LES2#", $"To: {edge.ToNode}, Weight: {edge.Weight}, Content: {edge.EdgeContent}");
                }
            }
            else
            {
                DebugWriter.DebugWriteLine("#LEE2#", $"No edges found for node {nodeId}.");
            }
        }

        static void ViewNodesAndEdges(GraphFileManager manager)
        {
            // This method assumes we have a way to get all node IDs. 
            // If not, we'll need to modify GraphFileManager to support this.
            List<long> nodeIds = manager.GetAllNodeIds();

            if (nodeIds.Count == 0)
            {
                DebugWriter.DebugWriteLine("#VNE1#", "No nodes found in the graph.");
                return;
            }

            foreach (long nodeId in nodeIds)
            {
                Node? node = manager.LoadNode(nodeId);
                if (node != null)
                {
                    DebugWriter.DebugWriteLine("#VNS1#", $"Node {nodeId}: {node.Content}");
                    List<Edge> edges = manager.LoadEdges(nodeId);
                    if (edges.Count > 0)
                    {
                        DebugWriter.DebugWriteLine("#VNS2#", $"  Edges:");
                        foreach (var edge in edges)
                        {
                            DebugWriter.DebugWriteLine("#VNS3#", $"    To: {edge.ToNode}, Weight: {edge.Weight}, Content: {edge.EdgeContent}");
                        }
                    }
                    else
                    {
                        DebugWriter.DebugWriteLine("#VNS4#", "  No edges for this node.");
                    }
                }
                else
                {
                    DebugWriter.DebugWriteLine("#VNE2#", $"Failed to load node {nodeId}.");
                }
            }
        }
    }
}
