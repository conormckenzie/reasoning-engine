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
                new MenuItem("Process Node Query", "#CMD001#", "node_query"),
                new MenuItem("Process Outgoing Edge Query", "#CMD002#", "outgoing_edge_query"),
                new MenuItem("Process Incoming Edge Query", "#CMD003#", "incoming_edge_query"),
                new MenuItem("Add Node", "#CMD004#", "add_node"),
                new MenuItem("Delete Node", "#CMD005#", "delete_node"),
                new MenuItem("Edit Node", "#CMD006#", "edit_node"),
                new MenuItem("Add Edge", "#CMD007#", "add_edge"),
                new MenuItem("Delete Edge", "#CMD008#", "delete_edge"),
                new MenuItem("Edit Edge", "#CMD009#", "edit_edge"),
            };
        }

        public void ShowMenu()
        {
            while (true)
            {
                DebugWriter.DebugWriteLine("#CMD000#", "\nGraph Operations Menu:");

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
                    return $"{nodeId}|{content}";

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