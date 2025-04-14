using System;
using System.Collections.Generic;
using ReasoningEngine.GraphAccess;
using DebugUtils;

namespace ReasoningEngine.GraphAccess
{
    public class GraphOperationsUserMenu
    {
        private readonly CommandProcessor commandProcessor;
        private List<MenuItem> menuItems;

        public GraphOperationsUserMenu(CommandProcessor commandProcessor)
        {
            this.commandProcessor = commandProcessor;

            menuItems = new List<MenuItem>
            {
                new MenuItem("Process Node Query", "#9JA6QJ#", "node_query"),
                new MenuItem("Process Outgoing Edge Query", "#0HB5WR#", "outgoing_edge_query"),
                new MenuItem("Process Incoming Edge Query", "#0JF46M#", "incoming_edge_query"),
                new MenuItem("Add Node", "#AIDHN1#", "add_node"),
                new MenuItem("Delete Node", "#44VK8T#", "delete_node"),
                new MenuItem("Edit Node", "#49ABKG#", "edit_node"),
                new MenuItem("Add Edge", "#XXJ4R5#", "add_edge"),
                new MenuItem("Delete Edge", "#NWBR17#", "delete_edge"),
                new MenuItem("Edit Edge", "#A2F07N#", "edit_edge"),
            };
        }

        public void ShowMenu()
        {
            while (true)
            {
                DebugWriter.DebugWriteLine("#1XJSDP#", "\nGraph Operations Menu:");

                for (int i = 0; i < menuItems.Count; i++)
                {
                    DebugWriter.DebugWriteLine(menuItems[i].DebugString, $"{i + 1}. {menuItems[i].DisplayText}");
                }

                DebugWriter.DebugWriteLine("#CMD999#", "0. Back to Main Menu");

                DebugWriter.DebugWrite("#CMD998#", "Enter option: ");
                var option = Console.ReadLine();

                if (option == "0")
                {
                    return;
                }
                else if (int.TryParse(option, out int selectedOption) && selectedOption > 0 && selectedOption <= menuItems.Count)
                {
                    var selectedItem = menuItems[selectedOption - 1];
                    string payload = GetPayloadForCommand(selectedItem.InternalText);
                    string commandResult = commandProcessor.ProcessCommand(selectedItem.InternalText, payload);
                    DebugWriter.DebugWriteLine("#CMDRES#", commandResult);
                }
                else
                {
                    DebugWriter.DebugWriteLine("#INV002#", "Invalid option. Please try again.");
                }
            }
        }

        private string GetPayloadForCommand(string command)
        {
            switch (command)
            {
                case "node_query":
                case "outgoing_edge_query":
                case "incoming_edge_query":
                case "delete_node":
                    DebugWriter.DebugWrite("#PYIN01#", "Enter node ID: ");
                    return Console.ReadLine() ?? "";

                case "add_node":
                case "edit_node":
                    DebugWriter.DebugWrite("#PYIN02#", "Enter node ID: ");
                    string nodeId = Console.ReadLine() ?? "";
                    DebugWriter.DebugWrite("#PYIN03#", "Enter node content: ");
                    string content = Console.ReadLine() ?? "";

                    // --- Generic Parameter Prompt ---
                    string basePayload = $"{nodeId}|{content}";
                    DebugWriter.DebugWrite("#PYIN10#", "Enter additional parameters (e.g., Role=Variable|VariableDomainType=Truth|FunctionParams=Key:Val;Key2:Val2) [Optional]: ");
                    string additionalParams = Console.ReadLine() ?? "";

                    if (!string.IsNullOrWhiteSpace(additionalParams))
                    {
                        // Append additional params if provided
                        return $"{basePayload}|{additionalParams.Trim()}";
                    }
                    else
                    {
                        // Return only base payload if no additional params entered
                        return basePayload;
                    }
                    // --- End Generic Parameter Prompt ---


                case "add_edge":
                case "edit_edge":
                    DebugWriter.DebugWrite("#PYIN04#", "Enter source node ID: ");
                    string sourceId = Console.ReadLine() ?? "";
                    DebugWriter.DebugWrite("#PYIN05#", "Enter destination node ID: ");
                    string destId = Console.ReadLine() ?? "";
                    DebugWriter.DebugWrite("#PYIN06#", "Enter edge weight: ");
                    string weight = Console.ReadLine() ?? "";
                    DebugWriter.DebugWrite("#PYIN07#", "Enter edge content: ");
                    string edgeContent = Console.ReadLine() ?? "";
                    return $"{sourceId}|{destId}|{weight}|{edgeContent}";

                case "delete_edge":
                    DebugWriter.DebugWrite("#PYIN08#", "Enter source node ID: ");
                    string delSourceId = Console.ReadLine() ?? "";
                    DebugWriter.DebugWrite("#PYIN09#", "Enter destination node ID: ");
                    string delDestId = Console.ReadLine() ?? "";
                    return $"{delSourceId}|{delDestId}";

                default:
                    return "";
            }
        }
    }
}
