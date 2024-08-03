using System;
using System.Collections.Generic;
using DotNetEnv;
using GraphFileHandling;
using Newtonsoft.Json;
using DebugUtils; // Ensure you use the correct namespace

class Program
{
    // Initialize nodes and edges
    public static List<Node> nodes = new List<Node>(); // Use Node class instead of tuple
    public static List<Edge> edges = new List<Edge>(); // Use Edge class

    // Lists to track node and edge changes
    public static List<Node> nodeChanges = new List<Node>();
    public static List<Edge> edgeChanges = new List<Edge>();

    static void Main(string[] args)
    {
        // Load the environment variables from the .env file
        Env.Load();

        // Get the folder path and file name from environment variables
        string dataFolderPath = Environment.GetEnvironmentVariable("DATA_FOLDER_PATH") ?? throw new Exception();

        // Create an instance of the GraphFileManager
        var manager = new GraphFileManager(dataFolderPath);

        // Run the SaveLoad test
        // SaveLoadTest.RunTest(manager);

        ShowMenu(manager);
    }

    static void ShowMenu(GraphFileManager manager)
    {
        while (true)
        {
            DebugWriter.DebugWriteLine("#D7D1#", "Select an option:");
            DebugWriter.DebugWriteLine("#D7D2#", "1. Save Node");
            DebugWriter.DebugWriteLine("#D7D3#", "2. Load Node");
            DebugWriter.DebugWriteLine("#D7D4#", "3. Set Debug Mode");
            DebugWriter.DebugWriteLine("#D7D5#", "4. Exit");
            DebugWriter.DebugWrite("#D7D6#", "Enter option: ");

            var option = Console.ReadLine();

            switch (option)
            {
                case "1":
                    SaveNode(manager);
                    break;
                case "2":
                    LoadNode(manager);
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

    static void SaveNode(GraphFileManager manager)
    {
        DebugWriter.DebugWriteLine("#D7D8#", "Select save option:");
        DebugWriter.DebugWriteLine("#D7D9#", "1. Manual entry");
        DebugWriter.DebugWriteLine("#D7DA#", "2. Save from changes list");
        DebugWriter.DebugWriteLine("#D7DB#", "Enter option: ");
        var saveOption = Console.ReadLine();

        if (saveOption == "1")
        {
            DebugWriter.DebugWriteLine("#D7DC#", "Enter node ID: ");
            if (long.TryParse(Console.ReadLine(), out long nodeId))
            {
                DebugWriter.DebugWriteLine("#D7DD#", "Enter node content: ");
                string nodeContent = Console.ReadLine();

                var edges = new List<Edge>();
                while (true)
                {
                    DebugWriter.DebugWriteLine("#D7DE#", "Add an edge? (yes/no): ");
                    if (Console.ReadLine().ToLower() != "yes")
                        break;

                    DebugWriter.DebugWriteLine("#D7DF#", "Enter from node ID: ");
                    long fromNodeId = long.Parse(Console.ReadLine());
                    DebugWriter.DebugWriteLine("#D7E0#", "Enter to node ID: ");
                    long toNodeId = long.Parse(Console.ReadLine());
                    DebugWriter.DebugWriteLine("#D7E1#", "Enter edge weight: ");
                    double weight = double.Parse(Console.ReadLine());
                    DebugWriter.DebugWriteLine("#D7E2#", "Enter edge content: ");
                    string edgeContent = Console.ReadLine();

                    edges.Add(new Edge { FromNode = fromNodeId, ToNode = toNodeId, Weight = weight, EdgeContent = edgeContent });
                }

                if (manager.SaveNode(nodeId, nodeContent, edges))
                {
                    DebugWriter.DebugWriteLine("#D7E3#", $"Node {nodeId} saved successfully.");
                    // Add to changes list
                    nodeChanges.Add(new Node { Id = nodeId, Content = nodeContent });
                    edgeChanges.AddRange(edges);
                }
            }
            else
            {
                DebugWriter.DebugWriteLine("#D7E4#", "Invalid node ID.");
            }
        }
        else if (saveOption == "2")
        {
            foreach (var node in nodeChanges)
            {
                if (manager.SaveNode(node.Id, node.Content, edgeChanges.FindAll(e => e.FromNode == node.Id || e.ToNode == node.Id)))
                {
                    DebugWriter.DebugWriteLine("#D7E5#", $"Node {node.Id} saved successfully from changes list.");
                }
                else
                {
                    DebugWriter.DebugWriteLine("#D7E6#", $"Failed to save node {node.Id} from changes list.");
                }
            }
        }
        else
        {
            DebugWriter.DebugWriteLine("#D7E7#", "Invalid save option. Please try again.");
        }
    }

    static void LoadNode(GraphFileManager manager)
    {
        DebugWriter.DebugWriteLine("#D7E8#", "Enter node ID: ");
        if (long.TryParse(Console.ReadLine(), out long nodeId))
        {
            var result = manager.LoadNode(nodeId);
            if (result.Item1)
            {
                DebugWriter.DebugWriteLine("#D7E9#", $"Node ID: {result.Item2}");
                DebugWriter.DebugWriteLine("#D7EA#", $"Node Content: {result.Item3}");
                DebugWriter.DebugWriteLine("#D7EB#", $"Edges: {JsonConvert.SerializeObject(result.Item4, Formatting.Indented)}");
            }
        }
        else
        {
            DebugWriter.DebugWriteLine("#D7EC#", "Invalid node ID.");
        }
    }
}




// ============================================================================
//                               TODO LIST
// ============================================================================
// ** THIS SECTION IS SPECIFIC TO CONTRIBUTOR: CONOR ONLY **
// ** OTHER CONTRIBUTORS CAN IGNORE THIS SECTION **

/* TODO:

(1.-2) Move Edge, Node classes to own files instead of being from SaveLoad.cs
(1.-2.1) Consider moving more functions out of Program.cs into own files
(1.-1) Address warnings in the compiler
(1) Test Save/Load function a bit better
(1.1) Create Github repo for this project

(2.-1) Figure out if using a certain method of tracking new & changed nodes & edges introduces limitations on how I can update the graph
(2) Track new nodes; edges
(2.1) Track changed nodes; edges
(3) Append new nodes & edges to file
(4) Print all nodes, edges to console

TO IMPORT TO CLICKUP:
    Future items:
        - Improve file structure to not wastefully overwrite complete file each time an update is made
*/