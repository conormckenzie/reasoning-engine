using System;
using System.Collections.Generic;
using DotNetEnv;
using ReasoningEngine.GraphFileHandling;
using ReasoningEngine.GraphAccess;
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
                DebugWriter.DebugWriteLine("#00D7D8#", "2. Command Processor Options");
                DebugWriter.DebugWriteLine("#00D7D9#", "3. Debug Options");
                DebugWriter.DebugWriteLine("#00D7DA#", "0. Exit");
                DebugWriter.DebugWrite("#00D7D0#", "Enter option: ");

                var option = Console.ReadLine();

                switch (option)
                {
                    case "1":
                        OneTimeSetup.Initialize();
                        break;
                    case "2":
                        commandProcessor.ShowCommandProcessorMenu();
                        break;
                    case "3":
                        DebugOptions.ShowDebugOptionsMenu();
                        break;
                    case "0":
                        return;
                    default:
                        DebugWriter.DebugWriteLine("#00INV1#", "Invalid option. Please try again.");
                        break;
                }
            }
        }
    }
}
