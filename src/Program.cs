// Program.cs
using System;
using System.Collections.Generic;
using DotNetEnv;
using GraphFileHandling;
using Newtonsoft.Json;

class Program
{
    static List<(long, string)> nodeChanges = new List<(long, string)>();
    static List<Edge> edgeChanges = new List<Edge>();
    static bool debugMode = false;

    static void Main(string[] args)
    {
        // Load the environment variables from the .env file
        Env.Load();

        // Get the folder path and file name from environment variables
        string folderPath = Environment.GetEnvironmentVariable("FOLDER_PATH") ?? throw new Exception();
        string fileName = Environment.GetEnvironmentVariable("FILE_NAME") ?? throw new Exception();

        // Combine folder path and file name to get the full file path
        string filePath = System.IO.Path.Combine(folderPath, fileName);

        // Initialize nodes and edges
        var nodes = new List<(int, string)>(); // [0] -> index; [1]-> semantic content of node 
        var edges = new List<Edge>(); // Use Edge class instead of tuple

        // Lists to track node and edge changes

        // Example data to test saving
        long nodeId = 1;
        string nodeContent = "Node 1 content";
        edges.Add(new Edge { FromNode = 1, ToNode = 2, Weight = 0.9, EdgeContent = "edge to node 2" });
        edges.Add(new Edge { FromNode = 3, ToNode = 1, Weight = -0.3, EdgeContent = "edge from node 3" });

        // Define the base directory for the graph files
        string baseDir = folderPath;

        // Create an instance of the GraphFileManager
        var manager = new GraphFileManager(baseDir);

        // Run the SaveLoad test
        // SaveLoadTest.RunTest(manager);

        ShowMenu(manager);
    }

    // Method to show the menu and handle user input
    static void ShowMenu(GraphFileManager manager)
    {
        while (true)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            DebugWriteLine("#MNMU#", "\nMAIN MENU");
            Console.ResetColor();
            Console.WriteLine("Select an option:");
            Console.WriteLine("1. Save Node");
            Console.WriteLine("2. Load Node");
            Console.WriteLine("3. Set Debug Mode");
            Console.WriteLine("4. Run Tests");
            Console.Write("Enter option: ");

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
                    SetDebugMode();
                    break;
                case "4":
                    DebugWriteLine("#T7PC#", "Test non-debug message with inline: true");
                    DebugWriteLine("#T7PD#", "Test non-debug message with inline: false", false);
                    break;
                case "5":
                    return; // Exit the loop and end the program
                default:
                    Console.WriteLine("Invalid option. Please try again.");
                    break;
            }
        }
    }

    static void SetDebugMode()
    {
        Console.WriteLine("Debug mode is currently " + (debugMode ? "ON" : "OFF"));
        Console.Write("Would you like to turn it " + (debugMode ? "OFF" : "ON") + "? (y/n): ");
        string response = Console.ReadLine().ToLower();
        if (response == "y")
        {
            debugMode = !debugMode;
            Console.WriteLine("Debug mode is now " + (debugMode ? "ON" : "OFF"));
        }
        else
        {
            Console.WriteLine("Debug mode unchanged. It is currently " + (debugMode ? "ON" : "OFF"));
        }
    }

    static void DebugWriteLine(string debugMessage, string regularMessage, bool inLine = true)
    {
        if (debugMode)
        {
            if (inLine)
            {
                Console.ForegroundColor = ConsoleColor.Yellow; // Set the color for debug messages
                Console.Write("[DEBUG] { " + debugMessage + " }; ");
                Console.ResetColor(); // Reset the color to default
                Console.WriteLine(regularMessage);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("[DEBUG] " + debugMessage);
                Console.ResetColor();
            }
        }
        Console.WriteLine(regularMessage);
    }


    // Method to save node data
     static void SaveNode(GraphFileManager manager)
    {
        Console.WriteLine("Select save option:");
        Console.WriteLine("1. Manual entry");
        Console.WriteLine("2. Save from changes list");
        Console.Write("Enter option: ");
        var saveOption = Console.ReadLine();

        if (saveOption == "1")
        {
            Console.Write("Enter node ID: ");
            if (long.TryParse(Console.ReadLine(), out long nodeId))
            {
                Console.Write("Enter node content: ");
                string nodeContent = Console.ReadLine();

                var edges = new List<Edge>();
                while (true)
                {
                    Console.Write("Add an edge? (yes/no): ");
                    if (Console.ReadLine().ToLower() != "yes")
                        break;

                    Console.Write("Enter from node ID: ");
                    long fromNodeId = long.Parse(Console.ReadLine());
                    Console.Write("Enter to node ID: ");
                    long toNodeId = long.Parse(Console.ReadLine());
                    Console.Write("Enter edge weight: ");
                    double weight = double.Parse(Console.ReadLine());
                    Console.Write("Enter edge content: ");
                    string edgeContent = Console.ReadLine();

                    edges.Add(new Edge { FromNode = fromNodeId, ToNode = toNodeId, Weight = weight, EdgeContent = edgeContent });
                }

                if (manager.SaveNode(nodeId, nodeContent, edges))
                {
                    Console.WriteLine($"Node {nodeId} saved successfully.");
                    // Add to changes list
                    nodeChanges.Add((nodeId, nodeContent));
                    edgeChanges.AddRange(edges);
                }
            }
            else
            {
                Console.WriteLine("Invalid node ID.");
            }
        }
        else if (saveOption == "2")
        {
            foreach (var (id, content) in nodeChanges)
            {
                if (manager.SaveNode(id, content, edgeChanges.FindAll(e => e.FromNode == id || e.ToNode == id)))
                {
                    Console.WriteLine($"Node {id} saved successfully from changes list.");
                }
                else
                {
                    Console.WriteLine($"Failed to save node {id} from changes list.");
                }
            }
        }
        else
        {
            Console.WriteLine("Invalid save option. Please try again.");
        }
    }

    // Method to load node data
    static void LoadNode(GraphFileManager manager)
    {
        Console.Write("Enter node ID: ");
        if (long.TryParse(Console.ReadLine(), out long nodeId))
        {
            var result = manager.LoadNode(nodeId);
            if (result.Item1)
            {
                Console.WriteLine($"Node ID: {result.Item2}");
                Console.WriteLine($"Node Content: {result.Item3}");
                Console.WriteLine($"Edges: {JsonConvert.SerializeObject(result.Item4, Formatting.Indented)}");
            }
        }
        else
        {
            Console.WriteLine("Invalid node ID.");
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