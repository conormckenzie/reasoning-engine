using System;
using System.Threading.Tasks;
using ReasoningEngine.GraphFileHandling;
using DebugUtils;

namespace ReasoningEngine.GraphAccess
{
    public class CommandProcessor
    {
        private readonly GraphFileManager graphFileManager;

        public CommandProcessor(GraphFileManager graphFileManager)
        {
            this.graphFileManager = graphFileManager;
        }

        /// <summary>
        /// Displays the command processor menu and handles user input for command processing.
        /// </summary>
        public void ShowCommandProcessorMenu()
        {
            while (true)
            {
                // Display command processor options
                DebugWriter.DebugWriteLine("#CMD001#", "\nCommand Processor Options:");
                DebugWriter.DebugWriteLine("#CMD002#", "1. Process Node Query");
                DebugWriter.DebugWriteLine("#CMD003#", "2. Process Edge Query");
                DebugWriter.DebugWriteLine("#CMD004#", "3. Add Node");
                DebugWriter.DebugWriteLine("#CMD005#", "4. Delete Node");
                DebugWriter.DebugWriteLine("#CMD006#", "5. Edit Node");
                DebugWriter.DebugWriteLine("#CMD007#", "6. Add Edge");
                DebugWriter.DebugWriteLine("#CMD008#", "7. Delete Edge");
                DebugWriter.DebugWriteLine("#CMD009#", "8. Edit Edge");
                DebugWriter.DebugWriteLine("#CMD010#", "9. Back to Main Menu");
                DebugWriter.DebugWrite("#CMD011#", "Enter option: ");

                // Read user input
                var option = Console.ReadLine();

                // Process the command based on user input
                string commandResult;
                switch (option)
                {
                    case "1":
                        commandResult = ProcessCommand("node_query", "");
                        DebugWriter.DebugWriteLine("#RES001#", commandResult);
                        break;
                    case "2":
                        commandResult = ProcessCommand("edge_query", "");
                        DebugWriter.DebugWriteLine("#RES002#", commandResult);
                        break;
                    case "3":
                        commandResult = ProcessCommand("add_node", "");
                        DebugWriter.DebugWriteLine("#RES003#", commandResult);
                        break;
                    case "4":
                        commandResult = ProcessCommand("delete_node", "");
                        DebugWriter.DebugWriteLine("#RES004#", commandResult);
                        break;
                    case "5":
                        commandResult = ProcessCommand("edit_node", "");
                        DebugWriter.DebugWriteLine("#RES005#", commandResult);
                        break;
                    case "6":
                        commandResult = ProcessCommand("add_edge", "");
                        DebugWriter.DebugWriteLine("#RES006#", commandResult);
                        break;
                    case "7":
                        commandResult = ProcessCommand("delete_edge", "");
                        DebugWriter.DebugWriteLine("#RES007#", commandResult);
                        break;
                    case "8":
                        commandResult = ProcessCommand("edit_edge", "");
                        DebugWriter.DebugWriteLine("#RES008#", commandResult);
                        break;
                    case "9":
                        return; // Go back to the main menu
                    default:
                        DebugWriter.DebugWriteLine("#INV002#", "Invalid option. Please try again.");
                        break;
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
                default:
                    return "Unknown command";
            }
        }
    }
}
