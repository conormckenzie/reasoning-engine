using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ReasoningEngine.GraphFileHandling;
using DebugUtils;

namespace ReasoningEngine.GraphAccess
{
    public class CommandProcessor
    {
        private readonly GraphFileManager graphFileManager;
        private List<MenuItem> menuItems;

        public CommandProcessor(GraphFileManager graphFileManager)
        {
            this.graphFileManager = graphFileManager;

            // Initialize command processor menu items
            menuItems = new List<MenuItem>
            {
                new MenuItem("Process Node Query", "node_query"),
                new MenuItem("Process Edge Query", "edge_query"),
                new MenuItem("Add Node", "add_node"),
                new MenuItem("Delete Node", "delete_node"),
                new MenuItem("Edit Node", "edit_node"),
                new MenuItem("Add Edge", "add_edge"),
                new MenuItem("Delete Edge", "delete_edge"),
                new MenuItem("Edit Edge", "edit_edge"),
                new MenuItem("Test", "test")
            };
        }

        /// <summary>
        /// Displays the command processor menu and handles user input for command processing.
        /// </summary>
        public void ShowCommandProcessorMenu()
        {
            while (true)
            {
                DebugWriter.DebugWriteLine("#CMD001#", "\nCommand Processor Options:");

                // Display menu items dynamically with proper debug codes
                for (int i = 0; i < menuItems.Count; i++)
                {
                    string commandCode;

                    // Handle cases for different ranges
                    if (i + 2 < 10)
                    {
                        commandCode = $"#CMD00{i + 2}#";
                    }
                    else if (i + 2 < 100)
                    {
                        commandCode = $"#CMD0{i + 2}#";
                    }
                    else
                    {
                        commandCode = $"#CMD{i + 2}#"; 
                    }

                    DebugWriter.DebugWriteLine(commandCode, $"{i + 1}. {menuItems[i].Text}");
                }

                DebugWriter.DebugWriteLine("#CMD000#", "0. Back to Main Menu");

                DebugWriter.DebugWrite("#CMD999#", "Enter option: ");
                var option = Console.ReadLine();

                if (option == "0")
                {
                    return; // Go back to main menu
                }
                else if (int.TryParse(option, out int selectedOption) && selectedOption > 0 && selectedOption <= menuItems.Count)
                {
                    var selectedItem = menuItems[selectedOption - 1];
                    string commandResult = ProcessCommand(selectedItem.DebugString, "");
                    DebugWriter.DebugWriteLine("#RES#", commandResult);
                }
                else
                {
                    DebugWriter.DebugWriteLine("#INV002#", "Invalid option. Please try again.");
                }
            }
        }

        // Synchronous command processing
        public string ProcessCommand(string command, string payload)
        {
            switch (command.ToLower())
            {
                case "node_query":
                    return "Node query function is not available right now since I'm still working on it.";
                case "edge_query":
                    return "Edge query function is not available right now since I'm still working on it.";
                case "add_node":
                    return "Add node function is not available right now since I'm still working on it.";
                case "delete_node":
                    return "Delete node function is not available right now since I'm still working on it.";
                case "edit_node":
                    return "Edit node function is not available right now since I'm still working on it.";
                case "add_edge":
                    return "Add edge function is not available right now since I'm still working on it.";
                case "delete_edge":
                    return "Delete edge function is not available right now since I'm still working on it.";
                case "edit_edge":
                    return "Edit edge function is not available right now since I'm still working on it.";
                case "test":
                    return "Test MenuItem.";
                default:
                    return "Unknown command";
            }
        }

        // Asynchronous command processing
        public async Task<string> ProcessCommandAsync(string command, string payload)
        {
            switch (command.ToLower())
            {
                case "node_query":
                    return await Task.FromResult("Node query function is not available right now since I'm still working on it.");
                case "edge_query":
                    return await Task.FromResult("Edge query function is not available right now since I'm still working on it.");
                case "add_node":
                    return await Task.FromResult("Add node function is not available right now since I'm still working on it.");
                case "delete_node":
                    return await Task.FromResult("Delete node function is not available right now since I'm still working on it.");
                case "edit_node":
                    return await Task.FromResult("Edit node function is not available right now since I'm still working on it.");
                case "add_edge":
                    return await Task.FromResult("Add edge function is not available right now since I'm still working on it.");
                case "delete_edge":
                    return await Task.FromResult("Delete edge function is not available right now since I'm still working on it.");
                case "edit_edge":
                    return await Task.FromResult("Edit edge function is not available right now since I'm still working on it.");
                case "test":
                    return await Task.FromResult("Test MenuItem.");
                default:
                    return "Unknown command";
            }
        }
    }
}
