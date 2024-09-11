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
        // List to store main menu items
        private static List<MenuItem> mainMenuItems = new List<MenuItem>
        {
            new MenuItem("Run Setup", "setup"),
            new MenuItem("Save Node", "save_node"),
            new MenuItem("Load Node", "load_node"),
            new MenuItem("Command Processor Options", "command_processor"),
            new MenuItem("Debug Options", "debug_options"),
        };

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
                DebugWriter.DebugWriteLine("#0D7D01#", "\nMain Menu:");

                // Display regular menu items dynamically with unique codes
                for (int i = 0; i < mainMenuItems.Count; i++)
                {
                    string colorCode;

                    if (i + 2 < 10)
                    {
                        colorCode = $"#0D7D0{i + 2}#";
                    }
                    else
                    {
                        colorCode = $"#0D7D{i + 2}#";
                    }

                    DebugWriter.DebugWriteLine(colorCode, $"{i + 1}. {mainMenuItems[i].Text}");
                }

                DebugWriter.DebugWriteLine("#0D7D00#", "0. Exit");

                DebugWriter.DebugWrite("#0D7D00#", "Enter option: ");
                var option = Console.ReadLine();

                if (option == "0")
                {
                    return; // Exit the program
                }
                else if (int.TryParse(option, out int selectedOption) && selectedOption > 0 && selectedOption <= mainMenuItems.Count)
                {
                    var selectedItem = mainMenuItems[selectedOption - 1];

                    switch (selectedItem.DebugString)
                    {
                        case "setup":
                            OneTimeSetup.Initialize();
                            break;
                        case "save_node":
                            DebugWriter.DebugWriteLine("#00SOR1#", "Sorry, this has been disabled for now");
                            break;
                        case "load_node":
                            DebugWriter.DebugWriteLine("#00SOR2#", "Sorry, this has been disabled for now");
                            break;
                        case "command_processor":
                            commandProcessor.ShowCommandProcessorMenu();
                            break;
                        case "debug_options":
                            DebugOptions.ShowDebugOptionsMenu();
                            break;
                        default:
                            DebugWriter.DebugWriteLine("#00INV1#", "Invalid option. Please try again.");
                            break;
                    }
                }
                else
                {
                    DebugWriter.DebugWriteLine("#00INV1#", "Invalid option. Please try again.");
                }
            }
        }
    }
}
