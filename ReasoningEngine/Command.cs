using System;
using System.Threading.Tasks;
using ReasoningEngine.GraphFileHandling;

namespace ReasoningEngine.Communication
{
    public class CommandProcessor
    {
        private readonly GraphFileManager graphFileManager;

        public CommandProcessor(GraphFileManager graphFileManager)
        {
            this.graphFileManager = graphFileManager;
        }

        /// <summary>
        /// Displays the command menu and processes user input.
        /// </summary>
        public void ShowCommandMenu()
        {
            while (true)
            {
                Console.WriteLine("\nCommand Menu:");
                Console.WriteLine("1. Node Query");
                Console.WriteLine("2. Edge Query");
                Console.WriteLine("3. Add Node");
                Console.WriteLine("4. Delete Node");
                Console.WriteLine("5. Edit Node");
                Console.WriteLine("6. Add Edge");
                Console.WriteLine("7. Delete Edge");
                Console.WriteLine("8. Edit Edge");
                Console.WriteLine("9. Exit");
                Console.Write("Enter your choice: ");

                var choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        ExecuteCommand("node_query");
                        break;
                    case "2":
                        ExecuteCommand("edge_query");
                        break;
                    case "3":
                        ExecuteCommand("add_node");
                        break;
                    case "4":
                        ExecuteCommand("delete_node");
                        break;
                    case "5":
                        ExecuteCommand("edit_node");
                        break;
                    case "6":
                        ExecuteCommand("add_edge");
                        break;
                    case "7":
                        ExecuteCommand("delete_edge");
                        break;
                    case "8":
                        ExecuteCommand("edit_edge");
                        break;
                    case "9":
                        return; // Exit the loop
                    default:
                        Console.WriteLine("Invalid option. Please try again.");
                        break;
                }
            }
        }

        /// <summary>
        /// Executes a command synchronously based on user input.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        private void ExecuteCommand(string command)
        {
            Console.Write("Enter payload: ");
            var payload = Console.ReadLine();
            var result = ProcessCommand(command, payload);
            Console.WriteLine($"Result: {result}");
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
        /* The following codes are placeholder for now
         * 
                // Node Query
                private string ProcessNodeQuery(string payload) 
                {
                    // Implementation to query node(s)
                    return "Node query processed";
                }

                private async Task<string> ProcessNodeQueryAsync(string payload) 
                {
                    return await Task.Run(() => ProcessNodeQuery(payload));
                }

                // Edge Query
                private string ProcessEdgeQuery(string payload) 
                {
                    // Implementation to query edge(s)
                    return "Edge query processed";
                }

                private async Task<string> ProcessEdgeQueryAsync(string payload) 
                {
                    return await Task.Run(() => ProcessEdgeQuery(payload));
                }

                // Add Node
                private string ProcessAddNode(string payload)
                {
                    // Implementation to add a new node
                    return "Node added successfully";
                }

                private async Task<string> ProcessAddNodeAsync(string payload)
                {
                    return await Task.Run(() => ProcessAddNode(payload));
                }

                // Delete Node
                private string ProcessDeleteNode(string payload)
                {
                    // Implementation to delete a node
                    return "Node deleted successfully";
                }

                private async Task<string> ProcessDeleteNodeAsync(string payload)
                {
                    return await Task.Run(() => ProcessDeleteNode(payload));
                }

                // Edit Node
                private string ProcessEditNode(string payload)
                {
                    // Implementation to edit a node
                    return "Node edited successfully";
                }

                private async Task<string> ProcessEditNodeAsync(string payload)
                {
                    return await Task.Run(() => ProcessEditNode(payload));
                }

                // Add Edge
                private string ProcessAddEdge(string payload)
                {
                    // Implementation to add a new edge
                    return "Edge added successfully";
                }

                private async Task<string> ProcessAddEdgeAsync(string payload)
                {
                    return await Task.Run(() => ProcessAddEdge(payload));
                }

                // Delete Edge
                private string ProcessDeleteEdge(string payload)
                {
                    // Implementation to delete an edge
                    return "Edge deleted successfully";
                }

                private async Task<string> ProcessDeleteEdgeAsync(string payload)
                {
                    return await Task.Run(() => ProcessDeleteEdge(payload));
                }

                // Edit Edge
                private string ProcessEditEdge(string payload)
                {
                    // Implementation to edit an edge
                    return "Edge edited successfully";
                }

                private async Task<string> ProcessEditEdgeAsync(string payload)
                {
                    return await Task.Run(() => ProcessEditEdge(payload));
                }
                */
    }
}
