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

                // Read user input
                var option = Console.ReadLine();

                // Handle user input
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
                    default:
                        DebugWriter.DebugWriteLine("#00INV1#", "Invalid option. Please try again."); // Updated to next unique code
                        break;
                }
            }
        }
    }
}
